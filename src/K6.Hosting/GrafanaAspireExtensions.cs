using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace K6.Hosting;

/// <summary>
///     Extensions for adding Grafana to visualize k6 results
/// </summary>
public static class GrafanaAspireExtensions
{
    /// <summary>
    ///     Adds a Grafana resource to the application model
    /// </summary>
    /// <param name="builder">The distributed application builder</param>
    /// <param name="name">The name of the resource</param>
    /// <param name="dashboardsPath">Optional path to Grafana dashboards</param>
    /// <returns>A resource builder for further configuration</returns>
    public static IResourceBuilder<GrafanaResource> AddGrafana(this IDistributedApplicationBuilder builder, string name,
        string? dashboardsPath = null)
    {
        var resource = new GrafanaResource(name);
        var resourceBuilder = builder.AddResource(resource)
            .WithImage(ContainerImageTags.GrafanaImage)
            .WithImageRegistry(ContainerImageTags.GrafanaRegistry)
            .WithImageTag(ContainerImageTags.GrafanaTag)
            .WithEnvironment("GF_AUTH_ANONYMOUS_ORG_ROLE", "Admin")
            .WithEnvironment("GF_AUTH_ANONYMOUS_ENABLED", "true")
            .WithEnvironment("GF_AUTH_BASIC_ENABLED", "false")
            .WithEnvironment("GF_SERVER_SERVE_FROM_SUB_PATH", "true")
            .WithEndpoint(0, 3000, name: "http", scheme: "http");

        if (!string.IsNullOrEmpty(dashboardsPath))
        {
            resourceBuilder.WithBindMount(Path.GetFullPath(dashboardsPath), "/var/lib/grafana/dashboards");
        }

        return resourceBuilder;
    }

    /// <summary>
    ///     Configures Grafana with InfluxDB as data source
    /// </summary>
    /// <param name="builder">The resource builder</param>
    /// <param name="influxDbResource">The InfluxDB resource reference</param>
    /// <param name="datasourceConfigPath">Optional path to datasource config yaml</param>
    /// <returns>The resource builder for further configuration</returns>
    public static IResourceBuilder<GrafanaResource> WithInfluxDataSource(
        this IResourceBuilder<GrafanaResource> builder,
        IResourceBuilder<InfluxDbResource> influxDbResource,
        string? datasourceConfigPath = null)
    {
        builder.WithReference(influxDbResource);

        // If a datasource config is provided, mount it
        if (!string.IsNullOrEmpty(datasourceConfigPath))
        {
            builder.WithBindMount(
                Path.GetFullPath(datasourceConfigPath),
                "/etc/grafana/provisioning/datasources/datasource.yaml");
        }

        return builder;
    }

    /// <summary>
    ///     Adds dashboard configuration to Grafana
    /// </summary>
    /// <param name="builder">The resource builder</param>
    /// <param name="dashboardConfigPath">Path to dashboard config yaml</param>
    /// <returns>The resource builder for further configuration</returns>
    public static IResourceBuilder<GrafanaResource> WithDashboardConfig(
        this IResourceBuilder<GrafanaResource> builder,
        string dashboardConfigPath)
    {
        builder.WithBindMount(
            Path.GetFullPath(dashboardConfigPath),
            "/etc/grafana/provisioning/dashboards/dashboard.yaml");

        return builder;
    }
}

public sealed class GrafanaResource : ContainerResource
{
    public GrafanaResource(string name) : base(name)
    {
    }
}