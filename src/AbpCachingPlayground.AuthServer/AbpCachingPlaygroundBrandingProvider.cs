using Microsoft.Extensions.Localization;
using AbpCachingPlayground.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace AbpCachingPlayground;

[Dependency(ReplaceServices = true)]
public class AbpCachingPlaygroundBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<AbpCachingPlaygroundResource> _localizer;

    public AbpCachingPlaygroundBrandingProvider(IStringLocalizer<AbpCachingPlaygroundResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
