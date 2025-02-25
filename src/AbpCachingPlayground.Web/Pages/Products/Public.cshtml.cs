using AbpCachingPlayground.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

namespace AbpCachingPlayground.Web.Pages.Products
{
    [AllowAnonymous]
    public class PublicProductsModel : AbpPageModel
    {
        private readonly IProductsAppService _productAppService;

        public PublicProductsModel(IProductsAppService productAppService)
        {
            _productAppService = productAppService;
            Products = new PagedResultDto<ProductDto>(0, new List<ProductDto>());
        }

        public PagedResultDto<ProductDto> Products { get; set; }

        public async Task OnGetAsync()
        {
            Products = await _productAppService.GetListAsync(new GetProductsInput()
                {
                    MaxResultCount = 1000,
                    SkipCount = 0
                });
        }
    }
}
