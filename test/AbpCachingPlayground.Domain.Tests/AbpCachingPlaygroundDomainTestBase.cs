using Volo.Abp.Modularity;

namespace AbpCachingPlayground;

/* Inherit from this class for your domain layer tests. */
public abstract class AbpCachingPlaygroundDomainTestBase<TStartupModule> : AbpCachingPlaygroundTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
