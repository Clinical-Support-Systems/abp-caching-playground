using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Caching;
using Volo.Abp.Caching.Hybrid;
using Volo.Abp.Modularity;
using ZiggyCreatures.Caching.Fusion;

namespace AbpFusionCache
{
    [DependsOn(typeof(AbpCachingModule))]
    public class AbpFusionCacheModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            var configuration = context.Services.GetConfiguration();

            context.Services.AddHybridCache().AddSerializerFactory<AbpHybridCacheJsonSerializerFactory>();
            context.Services.AddSingleton(typeof(IHybridCache<>), typeof(AbpFusionCache<>));
            context.Services.AddSingleton(typeof(IHybridCache<,>), typeof(AbpFusionCache<,>));

            context.Services.AddFusionCache()
                .TryWithAutoSetup()
                .WithDefaultEntryOptions(new FusionCacheEntryOptions
                {
                    Duration = TimeSpan.FromMinutes(30)
                });

            context.Services.Configure<AbpFusionCacheOptions>(configuration.GetSection("FusionCache"));
            context.Services.AddTransient<IFusionCacheManager, FusionCacheManager>();
        }
    }
}
