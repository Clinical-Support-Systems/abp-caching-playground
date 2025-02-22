using Volo.Abp.Modularity;

namespace AbpCachingPlayground;

public abstract class AbpCachingPlaygroundApplicationTestBase<TStartupModule> : AbpCachingPlaygroundTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
