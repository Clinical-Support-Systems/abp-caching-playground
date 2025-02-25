using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using ZiggyCreatures.Caching.Fusion;

namespace AbpFusionCache
{
    public static class AbpFusionCacheExtensions
    {
        public static IServiceCollection AddFusionCacheDistributedImplementation(
            this IServiceCollection services,
            Action<FusionCacheOptions> setupAction = null)
        {
            // Add FusionCache services
            services.AddFusionCache()
                .WithSystemTextJsonSerializer();

            if (setupAction != null)
            {
                services.Configure(setupAction);
            }

            return services;
        }

        public static FusionCacheEntryOptions ToFusionCacheEntryOptions(
            this DistributedCacheEntryOptions options)
        {
            var fusionOptions = new FusionCacheEntryOptions();

            //if (options.AbsoluteExpiration.HasValue)
            //{
            //    fusionOptions.SetDuration(options.AbsoluteExpiration.Value);
            //}
            //else if (options.AbsoluteExpirationRelativeToNow.HasValue)
            //{
            //    fusionOptions.SetFailSafe(options.AbsoluteExpirationRelativeToNow.Value);
            //}

            //if (options.SlidingExpiration.HasValue)
            //{
            //    fusionOptions.SetSlidingExpiration(options.SlidingExpiration.Value);
            //}

            return fusionOptions;
        }
    }
}