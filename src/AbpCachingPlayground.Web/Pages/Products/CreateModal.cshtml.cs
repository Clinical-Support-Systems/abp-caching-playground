using AbpCachingPlayground.Shared;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volo.Abp.Application.Dtos;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AbpCachingPlayground.Products;

namespace AbpCachingPlayground.Web.Pages.Products
{
    public abstract class CreateModalModelBase : AbpCachingPlaygroundPageModel
    {

        [BindProperty]
        public ProductCreateViewModel Product { get; set; }

        protected IProductsAppService _productsAppService;

        public CreateModalModelBase(IProductsAppService productsAppService)
        {
            _productsAppService = productsAppService;

            Product = new();
        }

        public virtual async Task OnGetAsync()
        {
            Product = new ProductCreateViewModel();

            await Task.CompletedTask;
        }

        public virtual async Task<IActionResult> OnPostAsync()
        {

            await _productsAppService.CreateAsync(ObjectMapper.Map<ProductCreateViewModel, ProductCreateDto>(Product));
            return NoContent();
        }
    }

    public class ProductCreateViewModel : ProductCreateDto
    {
    }
}