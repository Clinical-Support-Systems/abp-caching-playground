using Volo.Abp.Modularity;

namespace AbpCachingPlayground;

[DependsOn(
    typeof(AbpCachingPlaygroundDomainModule),
    typeof(AbpCachingPlaygroundTestBaseModule)
)]
public class AbpCachingPlaygroundDomainTestModule : AbpModule
{

}
