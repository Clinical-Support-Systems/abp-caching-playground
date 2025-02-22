using System;

namespace AbpCachingPlayground.Products
{
    public abstract class ProductExcelDtoBase
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
    }
}