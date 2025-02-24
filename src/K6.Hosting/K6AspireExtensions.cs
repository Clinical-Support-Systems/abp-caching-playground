using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

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

        // let's use the k6 docker image here
        return builder.AddResource(resource)
            .WithImage(ContainerImageTags.K6Image)
            .WithImageRegistry(ContainerImageTags.K6Registry)
            .WithImageTag(ContainerImageTags.K6Tag)
            .WithEnvironment("K6_INSECURE_SKIP_TLS_VERIFY", "true")
            .WithEndpoint(0, 6565, name: "k6-api")
            .WithBindMount(Path.GetDirectoryName(Path.GetFullPath(scriptPath)), "/scripts")
            .WithArgs("run", $"/scripts/{scriptFileName}", "--out", "influxdb=http://influxdb:8086/k6");
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
    private readonly string _scriptPath;

    public K6Resource(string name, string scriptPath) : base(name)
    {
        _scriptPath = scriptPath ?? throw new ArgumentNullException(nameof(scriptPath));
    }
}

