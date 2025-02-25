using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace K6.Hosting;

/// <summary>
///     Extensions for adding InfluxDB to host k6 results
/// </summary>
public static class InfluxDbAspireExtensions
{
    private const int InfluxDbPort = 8086;

    /// <summary>
    /// Adds an InfluxDB resource to the application model
    /// </summary>
    /// <param name="builder">The distributed application builder</param>
    /// <param name="name">The name of the resource</param>
    /// <returns>A resource builder for further configuration</returns>
    public static IResourceBuilder<InfluxDbResource> AddInfluxDb(this IDistributedApplicationBuilder builder,
        string name)
    {
        var resource = new InfluxDbResource(name);

        return builder.AddResource(resource)
            .WithImage(ContainerImageTags.InfluxDbImage)
            .WithImageRegistry(ContainerImageTags.InfluxDbRegistry)
            .WithImageTag(ContainerImageTags.InfluxDbTag)
            .WithEnvironment("INFLUXDB_DB", "k6")
            .WithHttpEndpoint(0, InfluxDbPort, name: InfluxDbResource.PrimaryEndpointName);
    }
}

/// <summary>
/// The InfluxDb container resource 
/// </summary>
public sealed class InfluxDbResource : ContainerResource, IResourceWithConnectionString
{
    internal const string PrimaryEndpointName = "http";

    public InfluxDbResource(string name) : base(name)
    {
    }

    private EndpointReference? _primaryEndpoint;
    public EndpointReference PrimaryEndpoint => _primaryEndpoint ??= new EndpointReference(this, PrimaryEndpointName);

    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create(
            $"http://{PrimaryEndpoint.Property(EndpointProperty.Host)}:{PrimaryEndpoint.Property(EndpointProperty.Port)}/k6"
        );
}