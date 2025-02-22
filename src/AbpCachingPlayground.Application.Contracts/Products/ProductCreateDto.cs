using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace AbpCachingPlayground.Products
{
    public abstract class ProductCreateDtoBase
    {
        [Required]
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
    }
}