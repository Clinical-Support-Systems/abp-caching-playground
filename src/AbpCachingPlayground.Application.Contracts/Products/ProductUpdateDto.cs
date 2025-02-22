using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities;

namespace AbpCachingPlayground.Products
{
    public abstract class ProductUpdateDtoBase : IHasConcurrencyStamp
    {
        [Required]
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }

        public string ConcurrencyStamp { get; set; } = null!;
    }
}