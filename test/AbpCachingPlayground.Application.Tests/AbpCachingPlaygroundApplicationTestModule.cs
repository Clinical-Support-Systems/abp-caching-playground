using Volo.Abp.Modularity;

namespace AbpCachingPlayground;

[DependsOn(
    typeof(AbpCachingPlaygroundApplicationModule),
    typeof(AbpCachingPlaygroundDomainTestModule)
)]
public class AbpCachingPlaygroundApplicationTestModule : AbpModule
{

}
