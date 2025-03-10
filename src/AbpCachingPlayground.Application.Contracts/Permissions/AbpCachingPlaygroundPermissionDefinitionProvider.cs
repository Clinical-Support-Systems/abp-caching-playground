using AbpCachingPlayground.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace AbpCachingPlayground.Permissions;

public class AbpCachingPlaygroundPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(AbpCachingPlaygroundPermissions.GroupName);

        myGroup.AddPermission(AbpCachingPlaygroundPermissions.Dashboard.Host, L("Permission:Dashboard"), MultiTenancySides.Host);
        myGroup.AddPermission(AbpCachingPlaygroundPermissions.Dashboard.Tenant, L("Permission:Dashboard"), MultiTenancySides.Tenant);

        //Define your own permissions here. Example:
        //myGroup.AddPermission(AbpCachingPlaygroundPermissions.MyPermission1, L("Permission:MyPermission1"));

        var productPermission = myGroup.AddPermission(AbpCachingPlaygroundPermissions.Products.Default, L("Permission:Products"));
        productPermission.AddChild(AbpCachingPlaygroundPermissions.Products.Create, L("Permission:Create"));
        productPermission.AddChild(AbpCachingPlaygroundPermissions.Products.Edit, L("Permission:Edit"));
        productPermission.AddChild(AbpCachingPlaygroundPermissions.Products.Delete, L("Permission:Delete"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<AbpCachingPlaygroundResource>(name);
    }
}