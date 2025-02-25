using AbpFusionCache;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace AbpCachingPlayground.Fusion.CachingModule
{
    [DependsOn(typeof(AbpFusionCacheModule))]
    public class PlaygroundFusionCachingModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            
        }
    }
}
