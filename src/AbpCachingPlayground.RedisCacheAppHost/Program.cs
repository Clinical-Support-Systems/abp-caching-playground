using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var sql = builder
    .AddSqlServer("sql")
    .WithLifetime(ContainerLifetime.Persistent);

var db = sql.AddDatabase("AbpCachingPlayground");

var redis = builder.AddRedis("redis");

// DbMigrator  
if (builder.Environment.IsDevelopment())
{
    builder
        .AddProject<Projects.AbpCachingPlayground_DbMigrator>("dbMigrator")
        .WithReference(db, "Default").WaitFor(db)
        .WithReference(redis, "Redis").WaitFor(redis)
        .WithReplicas(1);
}

// AuthServer  
var authServerLaunchProfile = "AbpCachingPlayground.AuthServer";
builder
    .AddProject<Projects.AbpCachingPlayground_AuthServer>("authserver", launchProfileName: authServerLaunchProfile)
    .WithExternalHttpEndpoints()
    .WithReference(db, "Default").WaitFor(db)
    .WithReference(redis).WaitFor(redis);

// HttpApi.Host  
var httpApiHostLaunchProfile = "AbpCachingPlayground.HttpApi.Host";
builder
    .AddProject<Projects.AbpCachingPlayground_HttpApi_Host>("httpapihost", launchProfileName: httpApiHostLaunchProfile)
    .WithExternalHttpEndpoints()
    .WithReference(db, "Default").WaitFor(db)
    .WithReference(redis).WaitFor(redis);

// Web  
builder
    .AddProject<Projects.AbpCachingPlayground_Web>("web", "AbpCachingPlayground.Web")
    .WithReference(redis).WaitFor(redis);

builder.Build().Run();