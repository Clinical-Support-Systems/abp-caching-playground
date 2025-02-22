using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using AbpCachingPlayground.EntityFrameworkCore;

namespace AbpCachingPlayground.Products
{
    public class EfCoreProductRepository : EfCoreProductRepositoryBase, IProductRepository
    {
        public EfCoreProductRepository(IDbContextProvider<AbpCachingPlaygroundDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }
    }
}