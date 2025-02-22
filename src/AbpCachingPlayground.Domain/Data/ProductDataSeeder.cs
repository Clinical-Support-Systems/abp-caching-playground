using AbpCachingPlayground.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Uow;

namespace AbpCachingPlayground.Data
{
    public class ProductDataSeeder : IDataSeedContributor, ITransientDependency
    {
        private readonly IRepository<Product, Guid> _productRepository;
        private readonly IGuidGenerator _guidGenerator;

        public ProductDataSeeder(IRepository<Product, Guid> productRepository,IGuidGenerator guidGenerator)
        {
            _productRepository = productRepository;
            _guidGenerator = guidGenerator;
        }

        public async Task SeedAsync(DataSeedContext context)
        {
            if (await _productRepository.GetCountAsync() > 0)
            {
                return;
            }

            var products = new List<Product>();
            const int batchSize = 1000;

            for (var i = 0; i < 100000; i++)
            {
                var price = (decimal)Random.Shared.Next(1, 1001) + (decimal)Random.Shared.Next(0, 100) / 100;
                products.Add(new Product(
                    _guidGenerator.Create(),
                    $"Product {i}",
                    price,
                    $"Description for product {i}"
                ));

                if (products.Count < batchSize)
                {
                    continue;
                }

                await _productRepository.InsertManyAsync(products);
                products.Clear();
            }

            if (products.Count > 0)
            {
                await _productRepository.InsertManyAsync(products);
            }
        }
    }
}
