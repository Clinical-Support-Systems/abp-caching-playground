using System.Threading.Tasks;

namespace AbpCachingPlayground.Data;

public interface IAbpCachingPlaygroundDbSchemaMigrator
{
    Task MigrateAsync();
}
