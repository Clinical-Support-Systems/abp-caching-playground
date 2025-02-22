using AbpCachingPlayground.Samples;
using Xunit;

namespace AbpCachingPlayground.EntityFrameworkCore.Domains;

[Collection(AbpCachingPlaygroundTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<AbpCachingPlaygroundEntityFrameworkCoreTestModule>
{

}
