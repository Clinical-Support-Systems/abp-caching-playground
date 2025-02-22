using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;
using Volo.Abp.AspNetCore.Mvc.UI.Bootstrap.TagHelpers.Form;
using AbpCachingPlayground.Products;
using AbpCachingPlayground.Shared;

namespace AbpCachingPlayground.Web.Pages.Products
{
    public abstract class IndexModelBase : AbpPageModel
    {
        public string? NameFilter { get; set; }
        public string? DescriptionFilter { get; set; }
        public decimal? PriceFilterMin { get; set; }

        public decimal? PriceFilterMax { get; set; }

        protected IProductsAppService _productsAppService;

        public IndexModelBase(IProductsAppService productsAppService)
        {
            _productsAppService = productsAppService;
        }

        public virtual async Task OnGetAsync()
        {

            await Task.CompletedTask;
        }
    }
}