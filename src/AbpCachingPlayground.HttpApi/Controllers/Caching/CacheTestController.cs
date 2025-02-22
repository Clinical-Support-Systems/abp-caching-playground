using AbpCachingPlayground.Caching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AbpCachingPlayground.Controllers.Caching
{
    public class CacheTestController : AbpCachingPlaygroundController, ICacheTestService
    {
        private readonly ICacheTestService _service;

        public CacheTestController(ICacheTestService service)
        {
            _service = service;
        }
    }
}
