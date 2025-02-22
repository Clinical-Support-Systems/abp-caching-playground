using System;
using System.Linq;
using Shouldly;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Modularity;
using Xunit;

namespace AbpCachingPlayground.Products
{
    public abstract class ProductsAppServiceTests<TStartupModule> : AbpCachingPlaygroundApplicationTestBase<TStartupModule>
        where TStartupModule : IAbpModule
    {
        private readonly IProductsAppService _productsAppService;
        private readonly IRepository<Product, Guid> _productRepository;

        public ProductsAppServiceTests()
        {
            _productsAppService = GetRequiredService<IProductsAppService>();
            _productRepository = GetRequiredService<IRepository<Product, Guid>>();
        }

        [Fact]
        public async Task GetListAsync()
        {
            // Act
            var result = await _productsAppService.GetListAsync(new GetProductsInput());

            // Assert
            result.TotalCount.ShouldBe(2);
            result.Items.Count.ShouldBe(2);
            result.Items.Any(x => x.Id == Guid.Parse("e4e70e03-5940-4d1c-861b-f241e52e9c46")).ShouldBe(true);
            result.Items.Any(x => x.Id == Guid.Parse("f43678c4-14e6-4ae3-9c69-0d2ac0113372")).ShouldBe(true);
        }

        [Fact]
        public async Task GetAsync()
        {
            // Act
            var result = await _productsAppService.GetAsync(Guid.Parse("e4e70e03-5940-4d1c-861b-f241e52e9c46"));

            // Assert
            result.ShouldNotBeNull();
            result.Id.ShouldBe(Guid.Parse("e4e70e03-5940-4d1c-861b-f241e52e9c46"));
        }

        [Fact]
        public async Task CreateAsync()
        {
            // Arrange
            var input = new ProductCreateDto
            {
                Name = "a90f34cd2726442ba699a9a7901efd5776fd629dc0b343788a8faa082ee4346f8d53d84e2f084",
                Description = "5c5e2269cb3a43d19f5a72420ee20af653d89fa20e4547a3a4043fb106e3b31234a1f28871d746b28414dfa1d",
                Price = 934304853
            };

            // Act
            var serviceResult = await _productsAppService.CreateAsync(input);

            // Assert
            var result = await _productRepository.FindAsync(c => c.Id == serviceResult.Id);

            result.ShouldNotBe(null);
            result.Name.ShouldBe("a90f34cd2726442ba699a9a7901efd5776fd629dc0b343788a8faa082ee4346f8d53d84e2f084");
            result.Description.ShouldBe("5c5e2269cb3a43d19f5a72420ee20af653d89fa20e4547a3a4043fb106e3b31234a1f28871d746b28414dfa1d");
            result.Price.ShouldBe(934304853);
        }

        [Fact]
        public async Task UpdateAsync()
        {
            // Arrange
            var input = new ProductUpdateDto()
            {
                Name = "231650772503",
                Description = "a3754a0f57ce40baba61686149de2b00655fa7eac6104d939604987a80f03ba6801303a569dc41a383eba",
                Price = 750050744
            };

            // Act
            var serviceResult = await _productsAppService.UpdateAsync(Guid.Parse("e4e70e03-5940-4d1c-861b-f241e52e9c46"), input);

            // Assert
            var result = await _productRepository.FindAsync(c => c.Id == serviceResult.Id);

            result.ShouldNotBe(null);
            result.Name.ShouldBe("231650772503");
            result.Description.ShouldBe("a3754a0f57ce40baba61686149de2b00655fa7eac6104d939604987a80f03ba6801303a569dc41a383eba");
            result.Price.ShouldBe(750050744);
        }

        [Fact]
        public async Task DeleteAsync()
        {
            // Act
            await _productsAppService.DeleteAsync(Guid.Parse("e4e70e03-5940-4d1c-861b-f241e52e9c46"));

            // Assert
            var result = await _productRepository.FindAsync(c => c.Id == Guid.Parse("e4e70e03-5940-4d1c-861b-f241e52e9c46"));

            result.ShouldBeNull();
        }
    }
}