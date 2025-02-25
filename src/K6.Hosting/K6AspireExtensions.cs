using System.Diagnostics;
using System.Net;
using System.Text;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace K6.Hosting;

public static class K6AspireExtensions
{
    private const int K6Port = 6565;

    /// <summary>
    ///     Adds a k6 container resource to the <see cref="IDistributedApplicationBuilder" />.
    /// </summary>
    /// <param name="builder">
    ///     The <see cref="IDistributedApplicationBuilder" /> to which the k6 container resource will be
    ///     added.
    /// </param>
    /// <param name="name">The name of the k6 container resource.</param>
    /// <param name="scriptPath">The path to the k6 script file. The script file must be located on the host machine.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{K6Resource}" /> for further resource configuration.</returns>
    public static IResourceBuilder<K6Resource> AddK6(this IDistributedApplicationBuilder builder, string name,
        string scriptPath)
    {
        var scriptDir = Path.GetDirectoryName(Path.GetFullPath(scriptPath));
        var scriptFileName = Path.GetFileName(scriptPath);

        var resource = new K6Resource(name, scriptPath)
        {
            // Store the configuration in the resource for later use when running the test
            ScriptDirectory = scriptDir,
            ScriptFileName = scriptFileName,
            ImageRegistry = ContainerImageTags.K6Registry,
            ImageName = ContainerImageTags.K6Image,
            ImageTag = ContainerImageTags.K6Tag
        };

        // let's use the k6 docker image here
        var resourceBuilder = builder.AddResource(resource)
            .WithImage(ContainerImageTags.K6Image)
            .WithImageRegistry(ContainerImageTags.K6Registry)
            .WithImageTag(ContainerImageTags.K6Tag)
            .WithEnvironment("K6_INSECURE_SKIP_TLS_VERIFY", "true")
            .WithEndpoint(0, K6Port, name: "k6-api")
            .WithBindMount(scriptDir, "/scripts")
            .WithArgs("run", "/scripts/verify.js")
            .WithTestCommand(scriptFileName);

        return resourceBuilder;
    }

    private static IResourceBuilder<K6Resource> WithTestCommand(this IResourceBuilder<K6Resource> builder,
        string testScriptFile)
    {
        builder.Resource.ScriptPath = testScriptFile;

        builder.WithCommand(
            "run-cache-test",
            "Run Cache Test",
            context => OnRunTestCommandAsync(builder, context),
            OnUpdateResourceState);

        return builder;
    }

    private static ResourceCommandState OnUpdateResourceState(UpdateCommandStateContext context)
    {
        return context.ResourceSnapshot.State switch
        {
            { Text: "Stopped" } or
                { Text: "Exited" } or
                { Text: "Finished" } or
                { Text: "FailedToStart" } => ResourceCommandState.Enabled,
            _ => ResourceCommandState.Disabled
        };
    }

    private static async Task<ExecuteCommandResult> OnRunTestCommandAsync(IResourceBuilder<K6Resource> builder,
        ExecuteCommandContext context)
    {
        var logger = context.ServiceProvider.GetRequiredService<ResourceLoggerService>().GetLogger(builder.Resource);
        var notificationService = context.ServiceProvider.GetRequiredService<ResourceNotificationService>();
        var k6Resource = builder.Resource;

        await notificationService.PublishUpdateAsync(k6Resource, state => state with
        {
            State = new ResourceStateSnapshot("Starting k6 test...", KnownResourceStates.Starting)
        }).ConfigureAwait(false);

        // Create a unique container name for this test run
        var containerName = $"{k6Resource.Name}-{Guid.NewGuid().ToString("N").Substring(0, 8)}";

        // Get the host machine's IP address that will be accessible from containers
        var hostIp = await GetHostIpAddressAsync(logger);
        logger.LogInformation("Using host IP address: {HostIp}", hostIp);

        logger.LogInformation("Creating new k6 container {ContainerName} to run test", containerName);

        // Build the docker run command with all necessary parameters
        var dockerArgs = new StringBuilder();
        dockerArgs.Append("run --rm "); // Remove container when done
        dockerArgs.Append($"--name {containerName} ");

        // Add volume mount for scripts
        dockerArgs.Append($"-v \"{k6Resource.ScriptDirectory}:/scripts\" ");

        // Add environment variables
        dockerArgs.Append("-e K6_INSECURE_SKIP_TLS_VERIFY=true ");

        // Add the APP_HOST environment variable (important for test script)
        // For Windows hosts use 'host.docker.internal', for Linux use the actual IP
        if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
        {
            // On Windows, use host.docker.internal but ensure it's properly mapped
            dockerArgs.Append($"--add-host=host.docker.internal:{hostIp} ");
            dockerArgs.Append("-e APP_HOST=\"https://host.docker.internal:44319\" ");
        }
        else
        {
            // On Linux, use the actual IP address
            dockerArgs.Append($"--add-host=host.docker.internal:{hostIp} ");
            dockerArgs.Append($"-e APP_HOST=\"https://{hostIp}:44319\" ");
        }

        // Add any custom environment variables
        foreach (var env in k6Resource.EnvironmentVariables)
        {
            dockerArgs.Append($"-e {env.Key}=\"{env.Value}\" ");
        }

        // Add any InfluxDB configuration if present
        if (!string.IsNullOrEmpty(k6Resource.InfluxDbUrl))
        {
            dockerArgs.Append($"-e K6_OUT=\"{k6Resource.InfluxDbUrl}\" ");
        }

        // Determine which network mode to use
        if (k6Resource.UseHostNetwork)
        {
            // Use host network mode - this gives direct access to host services but breaks container DNS
            dockerArgs.Append("--network host ");

            // When using host network, InfluxDB needs to be addressed via localhost:port
            if (!string.IsNullOrEmpty(k6Resource.InfluxDbUrl) && k6Resource.InfluxDbPort > 0)
            {
                // Update the InfluxDB URL to use localhost instead of container name
                var influxDbUrl = $"influxdb=http://localhost:{k6Resource.InfluxDbPort}/k6";
                dockerArgs.Append($"-e K6_OUT=\"{influxDbUrl}\" ");
            }
        }
        else if (!string.IsNullOrEmpty(k6Resource.NetworkName))
        {
            // Use the specified Docker network
            dockerArgs.Append($"--network {k6Resource.NetworkName} ");
        }
        else
        {
            // Try to detect the Aspire network by listing Docker networks
            var networkName = await DetectAspireNetworkAsync(logger);
            if (!string.IsNullOrEmpty(networkName))
            {
                dockerArgs.Append($"--network {networkName} ");
                k6Resource.NetworkName = networkName;
            }
        }

        // Add image information
        var imageName = $"{k6Resource.ImageRegistry}/{k6Resource.ImageName}:{k6Resource.ImageTag}";
        dockerArgs.Append(imageName);

        // Add the run command with script
        dockerArgs.Append($" run /scripts/{k6Resource.ScriptFileName}");

        var p = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = dockerArgs.ToString(),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        logger.LogInformation("Executing command: docker {Arguments}", dockerArgs);

        p.OutputDataReceived += async (_, args) =>
        {
            if (string.IsNullOrEmpty(args.Data))
            {
                return;
            }

            logger.LogInformation("{Data}", args.Data);
            await notificationService.PublishUpdateAsync(k6Resource, state => state with
            {
                State = new ResourceStateSnapshot(args.Data, KnownResourceStates.Starting)
            }).ConfigureAwait(false);
        };

        p.ErrorDataReceived += async (_, args) =>
        {
            if (string.IsNullOrEmpty(args.Data))
            {
                return;
            }

            logger.LogError("{Data}", args.Data);
            await notificationService.PublishUpdateAsync(k6Resource, state => state with
            {
                State = new ResourceStateSnapshot(args.Data, KnownResourceStates.Starting)
            }).ConfigureAwait(false);
        };

        p.Start();
        p.BeginOutputReadLine();
        p.BeginErrorReadLine();

        await p.WaitForExitAsync(context.CancellationToken).ConfigureAwait(false);

        if (p.ExitCode == 0)
        {
            await notificationService.PublishUpdateAsync(k6Resource, state => state with
            {
                State = new ResourceStateSnapshot("K6 test completed successfully", KnownResourceStates.Running)
            }).ConfigureAwait(false);

            return new ExecuteCommandResult { Success = true };
        }

        await notificationService.PublishUpdateAsync(k6Resource, state => state with
        {
            State = new ResourceStateSnapshot($"k6 exited with {p.ExitCode}", KnownResourceStates.FailedToStart)
        }).ConfigureAwait(false);

        return new ExecuteCommandResult { Success = false, ErrorMessage = "Failed to run k6 script" };
    }

    /// <summary>
    /// Gets the host IP address that will be accessible from containers
    /// </summary>
    private static async Task<string> GetHostIpAddressAsync(ILogger logger)
    {
        try
        {
            // First try to get the Docker host IP from the special gateway address
            try
            {
                var p = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "docker",
                        Arguments = "network inspect bridge --format='{{range .IPAM.Config}}{{.Gateway}}{{end}}'",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                p.Start();
                var gatewayIp = (await p.StandardOutput.ReadToEndAsync()).Trim().Trim('\'');
                await p.WaitForExitAsync();

                if (!string.IsNullOrEmpty(gatewayIp) && IPAddress.TryParse(gatewayIp, out _))
                {
                    logger.LogInformation("Using Docker gateway IP: {GatewayIp}", gatewayIp);
                    return gatewayIp;
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to get Docker gateway IP");
            }

            // Fallback: Get a local IP address that might be accessible from containers
            // Find an IP address that's not a loopback or link-local address
            var hostEntry = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in hostEntry.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork &&
                    !IPAddress.IsLoopback(ip) &&
                    !ip.ToString().StartsWith("169.254."))
                {
                    logger.LogInformation("Using local network IP: {LocalIp}", ip);
                    return ip.ToString();
                }
            }

            // Last resort: try to use the Docker Desktop special IP (Windows/Mac)
            logger.LogInformation("Using Docker Desktop default host IP");
            return "host-gateway";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error determining host IP address, falling back to 'host-gateway'");
            return "host-gateway";
        }
    }

    /// <summary>
    /// Attempts to detect the Aspire network by querying Docker networks
    /// </summary>
    private static async Task<string> DetectAspireNetworkAsync(ILogger logger)
    {
        try
        {
            // Run docker network ls to get a list of networks
            var p = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "docker",
                    Arguments = "network ls --format \"{{.Name}}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            p.Start();
            var output = await p.StandardOutput.ReadToEndAsync();
            await p.WaitForExitAsync();

            // Look for a network that contains "aspire" in the name
            var networks = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var aspireNetwork = networks.FirstOrDefault(n => n.Contains("aspire", StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(aspireNetwork))
            {
                logger.LogInformation("Detected Aspire network: {NetworkName}", aspireNetwork);
                return aspireNetwork;
            }

            // If no "aspire" network found, look for "host" network
            var hostNetwork = networks.FirstOrDefault(n => n.Contains("host", StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(hostNetwork))
            {
                logger.LogInformation("Detected Host network: {NetworkName}", hostNetwork);
                return hostNetwork;
            }

            logger.LogWarning("Could not detect Aspire network. Available networks: {Networks}", string.Join(", ", networks));
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error detecting Aspire network");
            return null;
        }
    }

    /// <summary>
    /// Configures the K6 resource to output results to InfluxDB
    /// </summary>
    /// <param name="builder">The <see cref="IDistributedApplicationBuilder" /> to which the influxdb container resource will be added.</param>
    /// <param name="influxDbResource">The InfluxDB resource reference</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{K6Resource}" /> for further resource configuration.</returns>
    public static IResourceBuilder<K6Resource> WithInfluxDbOutput(this IResourceBuilder<K6Resource> builder,
        IResourceBuilder<InfluxDbResource> influxDbResource)
    {
        // Add an endpoint reference instead of a direct resource reference
        var endpointReference = influxDbResource.GetEndpoint(InfluxDbResource.PrimaryEndpointName);

        var influxDbUrl = $"influxdb=http://influxdb:{endpointReference.TargetPort}/k6";
        builder.Resource.InfluxDbUrl = influxDbUrl;

        builder.WithReference(endpointReference)
            .WithEnvironment("K6_OUT", influxDbUrl);

        return builder;
    }
}

public sealed class K6Resource : ContainerResource
{
    public K6Resource(string name, string scriptPath) : base(name)
    {
        ScriptPath = scriptPath ?? throw new ArgumentNullException(nameof(scriptPath));
    }

    public string ScriptPath { get; set; }
    public string? ScriptDirectory { get; set; }
    public string? ScriptFileName { get; set; }
    public string ImageRegistry { get; set; } = ContainerImageTags.K6Registry;
    public string ImageName { get; set; } = ContainerImageTags.K6Image;
    public string ImageTag { get; set; } = ContainerImageTags.K6Tag;
    public string? InfluxDbUrl { get; set; }
    public int InfluxDbPort { get; set; }
    public string? NetworkName { get; set; }
    public bool UseHostNetwork { get; set; } = true;
    public Dictionary<string, string> EnvironmentVariables { get; set; } = new Dictionary<string, string>();
}