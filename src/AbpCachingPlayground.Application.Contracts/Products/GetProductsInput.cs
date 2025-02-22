using Volo.Abp.Application.Dtos;
using System;

namespace AbpCachingPlayground.Products
{
    public abstract class GetProductsInputBase : PagedAndSortedResultRequestDto
    {

        public string? FilterText { get; set; }

        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? PriceMin { get; set; }
        public decimal? PriceMax { get; set; }

        public GetProductsInputBase()
        {

        }
    }
}