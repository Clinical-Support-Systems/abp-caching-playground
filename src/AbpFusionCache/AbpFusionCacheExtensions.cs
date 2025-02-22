using System;
using System.Threading;
using System.Threading.Tasks;

namespace AbpFusionCache
{
    public static class AbpFusionCacheExtensions
    {
        public static async Task<T> GetOrAddAsync<T>(
            this IFusionCacheManager cacheManager,
            string key,
            Func<Task<T>> factory,
            TimeSpan? slidingExpiration = null)
        {
            return await cacheManager.GetOrAddAsync(
                key,
                factory,
                slidingExpiration,
                CancellationToken.None
            );
        }
    }
}