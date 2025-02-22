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
    public class CreateModalModel : CreateModalModelBase
    {
        public CreateModalModel(IProductsAppService productsAppService)
            : base(productsAppService)
        {
        }
    }
}