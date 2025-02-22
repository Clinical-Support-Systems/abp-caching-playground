using AbpCachingPlayground.Samples;
using Xunit;

namespace AbpCachingPlayground.EntityFrameworkCore.Applications;

[Collection(AbpCachingPlaygroundTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<AbpCachingPlaygroundEntityFrameworkCoreTestModule>
{

}
