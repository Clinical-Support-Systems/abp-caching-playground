using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AbpCachingPlayground.Caching
{
    public interface ICachePlaygroundModule
    {
        void ConfigureCache(IServiceCollection services, IConfiguration configuration);
    }
}
