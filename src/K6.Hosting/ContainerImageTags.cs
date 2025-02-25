namespace K6.Hosting;

internal static class ContainerImageTags
{
    // K6 image details
    internal const string K6Registry = "docker.io";
    internal const string K6Image = "grafana/k6";
    internal const string K6Tag = "latest";

    // InfluxDB image details
    internal const string InfluxDbRegistry = "docker.io";
    internal const string InfluxDbImage = "influxdb";
    internal const string InfluxDbTag = "1.8";

    // Grafana image details
    internal const string GrafanaRegistry = "docker.io";
    internal const string GrafanaImage = "grafana/grafana";
    internal const string GrafanaTag = "latest";
}