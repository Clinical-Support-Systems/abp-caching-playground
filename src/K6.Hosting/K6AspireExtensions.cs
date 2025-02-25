using System.Diagnostics;
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

        logger.LogInformation("Creating new k6 container {ContainerName} to run test", containerName);

        // Build the docker run command with all necessary parameters
        var dockerArgs = new StringBuilder();
        dockerArgs.Append("run --rm "); // Remove container when done
        dockerArgs.Append($"--name {containerName} ");

        // Add volume mount for scripts
        dockerArgs.Append($"-v \"{k6Resource.ScriptDirectory}:/scripts\" ");

        // Add environment variables
        dockerArgs.Append("-e K6_INSECURE_SKIP_TLS_VERIFY=true ");

        // Add any InfluxDB configuration if present
        if (!string.IsNullOrEmpty(k6Resource.InfluxDbUrl))
        {
            dockerArgs.Append($"-e K6_OUT=\"{k6Resource.InfluxDbUrl}\" ");
        }

        // Add network configuration (use host network to access services)
        dockerArgs.Append("--network host ");

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
    ///     Configures the K6 resource to output results to InfluxDB
    /// </summary>
    /// <param name="builder">
    ///     The <see cref="IDistributedApplicationBuilder" /> to which the influxdb container resource will
    ///     be added.
    /// </param>
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
}