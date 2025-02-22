using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;
using AbpCachingPlayground.Shared;

namespace AbpCachingPlayground.Products
{
    public partial interface IProductsAppService : IApplicationService
    {

        Task<PagedResultDto<ProductDto>> GetListAsync(GetProductsInput input);

        Task<ProductDto> GetAsync(Guid id);

        Task DeleteAsync(Guid id);

        Task<ProductDto> CreateAsync(ProductCreateDto input);

        Task<ProductDto> UpdateAsync(Guid id, ProductUpdateDto input);

        Task<IRemoteStreamContent> GetListAsExcelFileAsync(ProductExcelDownloadDto input);
        Task DeleteByIdsAsync(List<Guid> productIds);

        Task DeleteAllAsync(GetProductsInput input);
        Task<AbpCachingPlayground.Shared.DownloadTokenResultDto> GetDownloadTokenAsync();

    }
}