using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace AbpCachingPlayground.Caching
{
    public class CacheTestService : AbpCachingPlaygroundAppService, ICacheTestService
    {
        private readonly IDistributedCache _distributedCache;

        public CacheTestService(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }
    }
}
