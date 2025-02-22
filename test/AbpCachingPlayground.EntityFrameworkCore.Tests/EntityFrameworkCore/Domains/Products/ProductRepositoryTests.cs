using Shouldly;
using System;
using System.Linq;
using System.Threading.Tasks;
using AbpCachingPlayground.Products;
using AbpCachingPlayground.EntityFrameworkCore;
using Xunit;

namespace AbpCachingPlayground.EntityFrameworkCore.Domains.Products
{
    public class ProductRepositoryTests : AbpCachingPlaygroundEntityFrameworkCoreTestBase
    {
        private readonly IProductRepository _productRepository;

        public ProductRepositoryTests()
        {
            _productRepository = GetRequiredService<IProductRepository>();
        }

        [Fact]
        public async Task GetListAsync()
        {
            // Arrange
            await WithUnitOfWorkAsync(async () =>
            {
                // Act
                var result = await _productRepository.GetListAsync(
                    name: "79f407aaca44414a9ba9cd0af653b320eea6977134b24f4da124642adf4a51d7",
                    description: "d45feb908b8f41e29d974f18c789"
                );

                // Assert
                result.Count.ShouldBe(1);
                result.FirstOrDefault().ShouldNotBe(null);
                result.First().Id.ShouldBe(Guid.Parse("e4e70e03-5940-4d1c-861b-f241e52e9c46"));
            });
        }

        [Fact]
        public async Task GetCountAsync()
        {
            // Arrange
            await WithUnitOfWorkAsync(async () =>
            {
                // Act
                var result = await _productRepository.GetCountAsync(
                    name: "afa4f4545c164ae4a1e57f25d38484fd044bba002ea049ad808095ca0d702e4de727",
                    description: "57544cb0709d47288ba680999"
                );

                // Assert
                result.ShouldBe(1);
            });
        }
    }
}