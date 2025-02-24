using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using AbpCachingPlayground.Permissions;
using AbpCachingPlayground.Products;
using MiniExcelLibs;
using Volo.Abp.Content;
using Volo.Abp.Authorization;
using Volo.Abp.Caching;
using Microsoft.Extensions.Caching.Distributed;
using AbpCachingPlayground.Shared;

namespace AbpCachingPlayground.Products
{
    [RemoteService(IsEnabled = false)]
    [Authorize(AbpCachingPlaygroundPermissions.Products.Default)]
    public abstract class ProductsAppServiceBase : AbpCachingPlaygroundAppService
    {
        protected IDistributedCache<ProductDownloadTokenCacheItem, string> _downloadTokenCache;
        protected IDistributedCache<List<ProductDto>, string> _productListCache;
        protected IProductRepository _productRepository;
        protected ProductManager _productManager;

        public ProductsAppServiceBase(IProductRepository productRepository, ProductManager productManager, IDistributedCache<ProductDownloadTokenCacheItem, string> downloadTokenCache, IDistributedCache<List<ProductDto>, string> productListCache)
        {
            _downloadTokenCache = downloadTokenCache;
            _productListCache = productListCache;
            _productRepository = productRepository;
            _productManager = productManager;

        }

        private string GenerateCacheKey(GetProductsInput input)
        {
            // Exclude paging parameters from the cache key
            // Only filter parameters should affect the cache key
            return $"Products:{input.FilterText}:{input.Name}:{input.Description}:{input.PriceMin}:{input.PriceMax}:{input.Sorting}";
        }

        private async Task<List<ProductDto>> GetCachedProductListAsync(GetProductsInput input)
        {
            // Check if cache has been invalidated recently
            var cacheInvalidationKey = "Products:LastInvalidationTime";
            var lastInvalidationTime = await _productListCache.GetAsync(cacheInvalidationKey);

            var cacheKey = GenerateCacheKey(input);
            var cachedList = await _productListCache.GetAsync(cacheKey);

            // If cache is invalidated or we don't have the data, fetch from repository
            if (cachedList == null)
            {
                // Get ALL items matching the filter criteria without paging
                var items = await _productRepository.GetListAsync(
                    input.FilterText,
                    input.Name,
                    input.Description,
                    input.PriceMin,
                    input.PriceMax,
                    input.Sorting); 

                cachedList = ObjectMapper.Map<List<Product>, List<ProductDto>>(items);

                await _productListCache.SetAsync(
                    cacheKey,
                    cachedList,
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                        SlidingExpiration = null
                    });
            }

            return cachedList;
        }

        private async Task InvalidateProductCacheAsync()
        {
            // In a real distributed system, you might want to use a more sophisticated approach
            // such as cache tags or a dedicated cache invalidation service

            // Since we don't have access to all cache keys directly in ABP's distributed cache,
            // we'll need to use a cache reset pattern

            // Option 1: If cache volume is low, you could clear the entire product cache
            await _productListCache.RemoveAsync("Products:*");
        }

        public virtual async Task<PagedResultDto<ProductDto>> GetListAsync(GetProductsInput input)
        {
            // Get all items from cache matching filter criteria
            var allItems = await GetCachedProductListAsync(input);

            // Calculate total count from the cached list
            var totalCount = allItems.Count;

            // Apply paging to the cached list in memory
            var pagedItems = allItems
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToList();

            return new PagedResultDto<ProductDto>
            {
                TotalCount = totalCount,
                Items = pagedItems
            };
        }

        public virtual async Task<ProductDto> GetAsync(Guid id)
        {
            return ObjectMapper.Map<Product, ProductDto>(await _productRepository.GetAsync(id));
        }

        [Authorize(AbpCachingPlaygroundPermissions.Products.Delete)]
        public virtual async Task DeleteAsync(Guid id)
        {
            await _productRepository.DeleteAsync(id);
            await InvalidateProductCacheAsync();
        }

        [Authorize(AbpCachingPlaygroundPermissions.Products.Create)]
        public virtual async Task<ProductDto> CreateAsync(ProductCreateDto input)
        {

            var product = await _productManager.CreateAsync(
            input.Name, input.Price, input.Description
            );
            await InvalidateProductCacheAsync();

            return ObjectMapper.Map<Product, ProductDto>(product);
        }

        [Authorize(AbpCachingPlaygroundPermissions.Products.Edit)]
        public virtual async Task<ProductDto> UpdateAsync(Guid id, ProductUpdateDto input)
        {

            var product = await _productManager.UpdateAsync(
            id,
            input.Name, input.Price, input.Description, input.ConcurrencyStamp
            );

            await InvalidateProductCacheAsync();

            return ObjectMapper.Map<Product, ProductDto>(product);
        }

        [AllowAnonymous]
        public virtual async Task<IRemoteStreamContent> GetListAsExcelFileAsync(ProductExcelDownloadDto input)
        {
            var downloadToken = await _downloadTokenCache.GetAsync(input.DownloadToken);
            if (downloadToken == null || input.DownloadToken != downloadToken.Token)
            {
                throw new AbpAuthorizationException("Invalid download token: " + input.DownloadToken);
            }

            var items = await _productRepository.GetListAsync(input.FilterText, input.Name, input.Description, input.PriceMin, input.PriceMax);

            var memoryStream = new MemoryStream();
            await memoryStream.SaveAsAsync(ObjectMapper.Map<List<Product>, List<ProductExcelDto>>(items));
            memoryStream.Seek(0, SeekOrigin.Begin);

            return new RemoteStreamContent(memoryStream, "Products.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }

        [Authorize(AbpCachingPlaygroundPermissions.Products.Delete)]
        public virtual async Task DeleteByIdsAsync(List<Guid> productIds)
        {
            await _productRepository.DeleteManyAsync(productIds);
            await InvalidateProductCacheAsync();
        }

        [Authorize(AbpCachingPlaygroundPermissions.Products.Delete)]
        public virtual async Task DeleteAllAsync(GetProductsInput input)
        {
            await _productRepository.DeleteAllAsync(input.FilterText, input.Name, input.Description, input.PriceMin, input.PriceMax);
            await InvalidateProductCacheAsync();
        }
        public virtual async Task<AbpCachingPlayground.Shared.DownloadTokenResultDto> GetDownloadTokenAsync()
        {
            var token = Guid.NewGuid().ToString("N");

            await _downloadTokenCache.SetAsync(
                token,
                new ProductDownloadTokenCacheItem { Token = token },
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30)
                });

            return new AbpCachingPlayground.Shared.DownloadTokenResultDto
            {
                Token = token
            };
        }
    }
}