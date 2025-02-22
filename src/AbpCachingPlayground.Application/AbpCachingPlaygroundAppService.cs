using AbpCachingPlayground.Localization;
using Volo.Abp.Application.Services;

namespace AbpCachingPlayground;

/* Inherit your application services from this class.
 */
public abstract class AbpCachingPlaygroundAppService : ApplicationService
{
    protected AbpCachingPlaygroundAppService()
    {
        LocalizationResource = typeof(AbpCachingPlaygroundResource);
    }
}
