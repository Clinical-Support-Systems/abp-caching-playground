﻿using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using AbpCachingPlayground.Data;
using Volo.Abp.DependencyInjection;

namespace AbpCachingPlayground.EntityFrameworkCore;

public class EntityFrameworkCoreAbpCachingPlaygroundDbSchemaMigrator
    : IAbpCachingPlaygroundDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreAbpCachingPlaygroundDbSchemaMigrator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolving the AbpCachingPlaygroundDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<AbpCachingPlaygroundDbContext>()
            .Database
            .MigrateAsync();
    }
}
