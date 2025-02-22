using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace AbpCachingPlayground.Products
{
    public partial interface IProductRepository : IRepository<Product, Guid>
    {

        Task DeleteAllAsync(
            string? filterText = null,
            string? name = null,
            string? description = null,
            decimal? priceMin = null,
            decimal? priceMax = null,
            CancellationToken cancellationToken = default);
        Task<List<Product>> GetListAsync(
                    string? filterText = null,
                    string? name = null,
                    string? description = null,
                    decimal? priceMin = null,
                    decimal? priceMax = null,
                    string? sorting = null,
                    int maxResultCount = int.MaxValue,
                    int skipCount = 0,
                    CancellationToken cancellationToken = default
                );

        Task<long> GetCountAsync(
            string? filterText = null,
            string? name = null,
            string? description = null,
            decimal? priceMin = null,
            decimal? priceMax = null,
            CancellationToken cancellationToken = default);
    }
}