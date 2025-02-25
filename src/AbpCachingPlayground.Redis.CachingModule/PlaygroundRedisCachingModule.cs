using Volo.Abp.Caching.StackExchangeRedis;
using Volo.Abp.Modularity;

namespace AbpCachingPlayground.Redis.CachingModule
{
    [DependsOn(typeof(AbpCachingStackExchangeRedisModule))]
    public class PlaygroundRedisCachingModule : AbpModule
    {

    }
}
