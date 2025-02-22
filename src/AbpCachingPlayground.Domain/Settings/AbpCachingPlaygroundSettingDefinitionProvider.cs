using Volo.Abp.Settings;

namespace AbpCachingPlayground.Settings;

public class AbpCachingPlaygroundSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(AbpCachingPlaygroundSettings.MySetting1));
    }
}
