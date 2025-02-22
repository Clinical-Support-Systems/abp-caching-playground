using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AbpCachingPlayground.EntityFrameworkCore;

/* This class is needed for EF Core console commands
 * (like Add-Migration and Update-Database commands) */
public class AbpCachingPlaygroundDbContextFactory : IDesignTimeDbContextFactory<AbpCachingPlaygroundDbContext>
{
    public AbpCachingPlaygroundDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();
        
        AbpCachingPlaygroundEfCoreEntityExtensionMappings.Configure();

        var builder = new DbContextOptionsBuilder<AbpCachingPlaygroundDbContext>()
            .UseSqlServer(configuration.GetConnectionString("Default"));
        
        return new AbpCachingPlaygroundDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../AbpCachingPlayground.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false);

        return builder.Build();
    }
}
