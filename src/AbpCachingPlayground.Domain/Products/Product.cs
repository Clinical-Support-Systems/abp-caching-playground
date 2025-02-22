using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;
using JetBrains.Annotations;

using Volo.Abp;

namespace AbpCachingPlayground.Products
{
    public abstract class ProductBase : AuditedAggregateRoot<Guid>
    {
        [NotNull]
        public virtual string Name { get; set; }

        [CanBeNull]
        public virtual string? Description { get; set; }

        [DataType(DataType.Currency)]
        public virtual decimal Price { get; set; }

        protected ProductBase()
        {

        }

        public ProductBase(Guid id, string name, decimal price, string? description = null)
        {

            Id = id;
            Check.NotNull(name, nameof(name));
            Name = name;
            Price = price;
            Description = description;
        }

    }
}