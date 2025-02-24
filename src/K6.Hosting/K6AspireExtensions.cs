using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace K6.Hosting;

public static class K6AspireExtensions
{
    /// <summary>
    /// Adds a k6 container resource to the <see cref="IDistributedApplicationBuilder" />.
    /// </summary>
    /// <param name="builder">The <see cref="IDistributedApplicationBuilder"/> to which the k6 container resource will be added.</param>
    /// <param name="name">The name of the k6 container resource.</param>
    /// <param name="scriptPath">The path to the k6 script file. The script file must be located on the host machine.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{K6Resource}"/> for further resource configuration.</returns>
    public static IResourceBuilder<K6Resource> AddK6(this IDistributedApplicationBuilder builder, string name,
        string scriptPath)
    {
        var resource = new K6Resource(name, scriptPath);
        var scriptFileName = Path.GetFileName(scriptPath);

        //// Get string[] of scripts in scriptPath which aren't verify.js
        //var testScriptFiles = Directory.GetFiles(Path.GetDirectoryName(scriptPath), "*.js")
        //    .Where(file => file != scriptPath)
        //    .Select(Path.GetFileName)
        //    .ToArray();

        // let's use the k6 docker image here
        var resourceBuilder = builder.AddResource(resource)
            .WithImage(ContainerImageTags.K6Image)
            .WithImageRegistry(ContainerImageTags.K6Registry)
            .WithImageTag(ContainerImageTags.K6Tag)
            .WithEnvironment("K6_INSECURE_SKIP_TLS_VERIFY", "true")
            .WithEndpoint(0, 6565, name: "k6-api")
            .WithBindMount(Path.GetDirectoryName(Path.GetFullPath(scriptPath)), "/scripts")
            .WithArgs("run", $"/scripts/{scriptFileName}", "--out", "influxdb=http://influxdb:8086/k6");

        //.WithArgs("k6", "run", $"/scripts/verify.js")
        //.WithTestCommand(scriptFileName);

        //builder.Eventing.Subscribe<AfterEndpointsAllocatedEvent>(async (e, ct) => { 
        //    resourceBuilder.WithArgs("k6", "run", $"/scripts/{scriptFileName}");
        //});

        return resourceBuilder;
    }

    public static IResourceBuilder<K6Resource> WithTestCommand(this IResourceBuilder<K6Resource> builder, string testScriptFile)
    {
        builder.Resource.ScriptPath = testScriptFile;

        builder.WithCommand(
            name: "run-test",
            displayName: "Run Test",
            executeCommand: context => OnRunTestCommandAsync(builder, context),
            updateState: OnUpdateResourceState);

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

    private static async Task<ExecuteCommandResult> OnRunTestCommandAsync(IResourceBuilder<K6Resource> builder, ExecuteCommandContext context)
    {
        var logger = context.ServiceProvider.GetRequiredService<ResourceLoggerService>().GetLogger(builder.Resource);
        var notificationService = context.ServiceProvider.GetRequiredService<ResourceNotificationService>();


        var p = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "k6",
                Arguments = $"run --insecure-skip-tls-verify /scripts/{Path.GetFileName(builder.Resource.ScriptPath)}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        p.OutputDataReceived += async (_, args) =>
        {
            if (string.IsNullOrEmpty(args.Data))
            {
                return;
            }

            logger.LogInformation("{Data}", args.Data);
            await notificationService.PublishUpdateAsync(builder.Resource, state => state with { State = new(args.Data, KnownResourceStates.Starting) }).ConfigureAwait(false);
        };

        p.ErrorDataReceived += async (_, args) =>
        {
            if (string.IsNullOrEmpty(args.Data))
            {
                return;
            }

            logger.LogError("{Data}", args.Data);
            await notificationService.PublishUpdateAsync(builder.Resource, state => state with { State = new(args.Data, KnownResourceStates.Starting) }).ConfigureAwait(false);
        };

        p.Start();
        p.BeginOutputReadLine();
        p.BeginErrorReadLine();

        await p.WaitForExitAsync(context.CancellationToken).ConfigureAwait(false);

        if (p.ExitCode == 0)
        {
            return new ExecuteCommandResult() { Success = true };
        }

        await notificationService.PublishUpdateAsync(builder.Resource, state => state with
        {
            State = new($"k6 exited with {p.ExitCode}", KnownResourceStates.FailedToStart)
        }).ConfigureAwait(false);

        return new ExecuteCommandResult() { Success = false, ErrorMessage = "Failed to run k6 script" };
    }

    /// <summary>
    /// Configures the K6 resource to output results to InfluxDB
    /// </summary>
    /// <param name="builder">The <see cref="IDistributedApplicationBuilder"/> to which the influxdb container resource will be added.</param>
    /// <param name="influxDbResource">The InfluxDB resource reference</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{K6Resource}"/> for further resource configuration.</returns>
    public static IResourceBuilder<K6Resource> WithInfluxDbOutput(this IResourceBuilder<K6Resource> builder, IResourceBuilder<InfluxDbResource> influxDbResource)
    {
        // Add an endpoint reference instead of a direct resource reference
        var endpointReference = influxDbResource.GetEndpoint("http");

        builder.WithReference(endpointReference)
            .WithEnvironment("K6_OUT", $"influxdb=http://{endpointReference}/k6");

        return builder;
    }

    /// <summary>
    /// Adds access to host gateway to the k6 container
    /// </summary>
    /// <param name="builder">The resource builder</param>
    /// <param name="hostName">Optional host name (defaults to localhost)</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{K6Resource}"/> for further resource configuration.</returns>
    public static IResourceBuilder<K6Resource> WithHostGatewayAccess(this IResourceBuilder<K6Resource> builder,
        string hostName = "localhost")
    {
        builder.WithArgs("extra_hosts", $"{hostName}:host-gateway");
        return builder;
    }
}

public sealed class K6Resource : ContainerResource
{
    public string ScriptPath { get; set; }

    public K6Resource(string name, string scriptPath) : base(name)
    {
        ScriptPath = scriptPath ?? throw new ArgumentNullException(nameof(scriptPath));
    }
}

