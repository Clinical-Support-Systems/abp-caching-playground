using AbpCachingPlayground.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace AbpCachingPlayground.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class AbpCachingPlaygroundController : AbpControllerBase
{
    protected AbpCachingPlaygroundController()
    {
        LocalizationResource = typeof(AbpCachingPlaygroundResource);
    }
}
