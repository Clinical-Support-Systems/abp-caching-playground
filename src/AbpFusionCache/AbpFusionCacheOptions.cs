using System;
using System.Collections.Generic;
using System.Text;

namespace AbpFusionCache
{
    public class AbpFusionCacheOptions
    {
        public bool IsEnabled { get; set; } = true;
        public string KeyPrefix { get; set; }
        public TimeSpan DefaultSlidingExpiration { get; set; } = TimeSpan.FromHours(1);
        public TimeSpan? DefaultAbsoluteExpiration { get; set; }
    }
}
