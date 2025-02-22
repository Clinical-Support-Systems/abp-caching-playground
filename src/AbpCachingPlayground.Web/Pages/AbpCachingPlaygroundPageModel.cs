using AbpCachingPlayground.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

namespace AbpCachingPlayground.Web.Pages;

/* Inherit your Page Model classes from this class.
 */
public abstract class AbpCachingPlaygroundPageModel : AbpPageModel
{
    protected AbpCachingPlaygroundPageModel()
    {
        LocalizationResourceType = typeof(AbpCachingPlaygroundResource);
    }
}
