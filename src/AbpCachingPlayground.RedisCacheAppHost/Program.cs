

using Aspire.Hosting.Lifecycle;
using K6.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Runtime.InteropServices;

var builder = DistributedApplication.CreateBuilder(args);

var sql = builder
    .AddSqlServer("sql")
    .WithLifetime(ContainerLifetime.Persistent);

var db = sql.AddDatabase("AbpCachingPlayground");

var redis = builder.AddRedis("redis").WithRedisInsight();
var influxDb = builder.AddInfluxDb("influxdb");

var grafana = builder.AddGrafana("grafana", dashboardsPath: "./../../test/AbpCachingPlayground.k6/dashboards")
    .WithReference(influxDb).WaitFor(influxDb)
    .WithInfluxDataSource(influxDb, datasourceConfigPath: "./../../test/AbpCachingPlayground.k6/grafana-datasource.yaml")
    .WithDashboardConfig("./../../test/AbpCachingPlayground.k6/grafana-dashboard.yaml");

var k6 = builder.AddK6("k6", scriptPath: "./../../test/AbpCachingPlayground.k6/scenarios/basic-crud.js")
    .WithInfluxDbOutput(influxDb)
    .WithHostGatewayAccess();

// DbMigrator  
IResourceBuilder<ProjectResource>? migration = null;
if (builder.Environment.IsDevelopment())
{
    migration = builder
        .AddProject<Projects.AbpCachingPlayground_DbMigrator>("dbMigrator")
        .WithReference(db, "Default").WaitFor(db)
        .WithReplicas(1);
}

// AuthServer  
var authServerLaunchProfile = "AbpCachingPlayground.AuthServer";
var authserver = builder
    .AddProject<Projects.AbpCachingPlayground_AuthServer>("authserver", launchProfileName: authServerLaunchProfile)
    .WithExternalHttpEndpoints()
    .WithReference(db, "Default").WaitFor(db)
    .WithReference(redis).WaitFor(redis);

if (migration != null)
{
    authserver.WaitForCompletion(migration);
}

// HttpApi.Host  
var httpApiHostLaunchProfile = "AbpCachingPlayground.HttpApi.Host";
var apiHost = builder
    .AddProject<Projects.AbpCachingPlayground_HttpApi_Host>("httpapihost", launchProfileName: httpApiHostLaunchProfile)
    .WithExternalHttpEndpoints()
    .WithReference(db, "Default").WaitFor(db)
    .WithReference(redis).WaitFor(redis);

if (migration != null)
{
    apiHost.WaitForCompletion(migration);
}

// Web  
builder
    .AddProject<Projects.AbpCachingPlayground_Web>("web", "AbpCachingPlayground.Web")
    .WithReference(redis).WaitFor(redis);

builder.Build().Run();