using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Hybrid;
using Volo.Abp.Caching.Hybrid;

namespace AbpFusionCache
{
    public class AbpFusionCache<TCacheItem> : IHybridCache<TCacheItem>
        where TCacheItem : class
    {
        public Task<TCacheItem> GetOrCreateAsync(string key, Func<Task<TCacheItem>> factory, Func<HybridCacheEntryOptions> optionsFactory = null, bool? hideErrors = null,
            bool considerUow = false, CancellationToken token = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task SetAsync(string key, TCacheItem value, HybridCacheEntryOptions options = null, bool? hideErrors = null,
            bool considerUow = false, CancellationToken token = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task RemoveAsync(string key, bool? hideErrors = null, bool considerUow = false,
            CancellationToken token = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task RemoveManyAsync(IEnumerable<string> keys, bool? hideErrors = null, bool considerUow = false,
            CancellationToken token = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public IHybridCache<TCacheItem, string> InternalCache { get; }
    }

    /// <summary>
    /// Represents a hybrid cache of <typeparamref name="TCacheItem"/> items.
    /// Uses <typeparamref name="TCacheKey"/> as the key type.
    /// </summary>
    /// <typeparam name="TCacheItem">The type of cache item being cached.</typeparam>
    /// <typeparam name="TCacheKey">The type of cache key being used.</typeparam>
    public class AbpFusionCache<TCacheItem, TCacheKey> : IHybridCache<TCacheItem, TCacheKey>
        where TCacheItem : class
        where TCacheKey : notnull
    {
        public Task<TCacheItem> GetOrCreateAsync(TCacheKey key, Func<Task<TCacheItem>> factory, Func<HybridCacheEntryOptions> optionsFactory = null, bool? hideErrors = null,
            bool considerUow = false, CancellationToken token = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task SetAsync(TCacheKey key, TCacheItem value, HybridCacheEntryOptions options = null, bool? hideErrors = null,
            bool considerUow = false, CancellationToken token = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task RemoveAsync(TCacheKey key, bool? hideErrors = null, bool considerUow = false,
            CancellationToken token = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task RemoveManyAsync(IEnumerable<TCacheKey> keys, bool? hideErrors = null, bool considerUow = false,
            CancellationToken token = new CancellationToken())
        {
            throw new NotImplementedException();
        }
    }
}
