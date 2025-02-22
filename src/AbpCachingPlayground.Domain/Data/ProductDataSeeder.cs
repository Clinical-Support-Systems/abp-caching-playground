using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AbpCachingPlayground.Products;
using Microsoft.Extensions.Logging;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;

namespace AbpCachingPlayground.Data;

public class ProductDataSeeder : IDataSeedContributor, ITransientDependency
{
    private readonly IGuidGenerator _guidGenerator;
    private readonly ILogger<ProductDataSeeder> _logger;
    private readonly IRepository<Product, Guid> _productRepository;

    public ProductDataSeeder(IRepository<Product, Guid> productRepository, IGuidGenerator guidGenerator,
        ILogger<ProductDataSeeder> logger)
    {
        _productRepository = productRepository;
        _guidGenerator = guidGenerator;
        _logger = logger;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        _logger.LogInformation("Starting product seeding...");

        var existingCount = await _productRepository.GetCountAsync();
        if (existingCount > 0)
        {
            _logger.LogInformation("Database already contains {Count} products. Skipping seed.", existingCount);
            return;
        }

        var products = new List<Product>();
        const int batchSize = 100;
        const int totalProducts = 10000;
        var totalBatches = (int)Math.Ceiling((double)totalProducts / batchSize);
        var currentBatch = 0;

        _logger.LogInformation("Beginning to seed {Total} products in batches of {BatchSize}", totalProducts,
            batchSize);

        for (var i = 0; i < totalProducts; i++)
        {
            var price = Random.Shared.Next(1, 1001) + (decimal)Random.Shared.Next(0, 100) / 100;
            products.Add(new Product(
                _guidGenerator.Create(),
                $"Product {i}",
                price,
                $"Description for product {i}"
            ));

            if (products.Count < batchSize && i < totalProducts - 1)
            {
                continue;
            }

            currentBatch++;
            _logger.LogInformation("Inserting batch {Current}/{Total} ({PercentComplete}% complete)",
                currentBatch,
                totalBatches,
                currentBatch * 100 / totalBatches);

            await _productRepository.InsertManyAsync(products);
            products.Clear();
        }

        var finalCount = await _productRepository.GetCountAsync();
        _logger.LogInformation("Product seeding completed. Total products in database: {Count}", finalCount);
    }
}