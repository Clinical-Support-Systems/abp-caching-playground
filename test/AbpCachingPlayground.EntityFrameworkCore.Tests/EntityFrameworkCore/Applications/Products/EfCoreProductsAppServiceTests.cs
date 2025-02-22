using AbpCachingPlayground.Products;
using Xunit;

namespace AbpCachingPlayground.EntityFrameworkCore.Applications.Products;

public class EfCoreProductsAppServiceTests : ProductsAppServiceTests<AbpCachingPlaygroundEntityFrameworkCoreTestModule>
{
}