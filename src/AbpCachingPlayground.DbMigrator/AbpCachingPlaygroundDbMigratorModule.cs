using AbpCachingPlayground.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Caching;
using Volo.Abp.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace AbpCachingPlayground.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(AbpCachingPlaygroundEntityFrameworkCoreModule),
    typeof(AbpCachingPlaygroundApplicationContractsModule)
)]
public class AbpCachingPlaygroundDbMigratorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpDistributedCacheOptions>(options => { options.KeyPrefix = "AbpCachingPlayground:"; });
    }
}
