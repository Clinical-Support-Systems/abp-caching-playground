using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace K6.Hosting;

/// <summary>
///     Extensions for adding InfluxDB to host k6 results
/// </summary>
public static class InfluxDbAspireExtensions
{
    /// <summary>
    ///     Adds an InfluxDB resource to the application model
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
            .WithEndpoint(0, 8086, name: "http");
    }
}
public sealed class InfluxDbResource : ContainerResource, IResourceWithConnectionString
{
    private const string DefaultHost = "influxdb";
    private const int DefaultPort = 8086;

    public InfluxDbResource(string name) : base(name)
    {
        // Define the primary endpoint in the constructor
        PrimaryEndpoint = new EndpointReference(this, "http");
    }

    public EndpointReference PrimaryEndpoint { get; }

    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create(
            $"http://{PrimaryEndpoint.Property(EndpointProperty.Host)}:{PrimaryEndpoint.Property(EndpointProperty.Port)}/k6"
        );
}