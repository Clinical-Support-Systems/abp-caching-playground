using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace AbpCachingPlayground.Data;

/* This is used if database provider does't define
 * IAbpCachingPlaygroundDbSchemaMigrator implementation.
 */
public class NullAbpCachingPlaygroundDbSchemaMigrator : IAbpCachingPlaygroundDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
