using System;
using System.Collections.Generic;

using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;

namespace AbpCachingPlayground.Products
{
    public abstract class ProductDtoBase : AuditedEntityDto<Guid>, IHasConcurrencyStamp
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }

        public string ConcurrencyStamp { get; set; } = null!;

    }
}