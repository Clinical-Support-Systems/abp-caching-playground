using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using ZiggyCreatures.Caching.Fusion;
using Volo.Abp.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AbpFusionCache
{
    public interface IFusionCacheManager
    {
        Task<T> GetOrAddAsync<T>(
            string key,
            Func<Task<T>> factory,
            FusionCacheEntryOptions options = null,
            CancellationToken token = default);

        Task<T> GetOrAddAsync<T>(
            string key,
            Func<Task<T>> factory,
            TimeSpan? slidingExpiration,
            CancellationToken token = default);

        ValueTask RemoveAsync(string key, CancellationToken token = default);
        ValueTask ClearAsync(CancellationToken token = default);
    }
    public class FusionCacheManager : IFusionCacheManager, ITransientDependency
    {
        protected IFusionCache Cache { get; }
        protected AbpFusionCacheOptions Options { get; }
        protected string KeyPrefix { get; }

        public FusionCacheManager(
            IFusionCache cache,
            IOptions<AbpFusionCacheOptions> options)
        {
            Cache = cache;
            Options = options.Value;
            KeyPrefix = Options.KeyPrefix ?? string.Empty;
        }

        public virtual async Task<T> GetOrAddAsync<T>(
            string key,
            Func<Task<T>> factory,
            FusionCacheEntryOptions options = null,
            CancellationToken token = default)
        {
            if (!Options.IsEnabled)
            {
                return await factory();
            }

            return await Cache.GetOrSetAsync(
                NormalizeKey(key),
                (ctx, ct) => factory(),
                MaybeValue<T>.None,
                options ?? GetDefaultOptions(),
                null,
                token
            );
        }

        public virtual Task<T> GetOrAddAsync<T>(
            string key,
            Func<Task<T>> factory,
            TimeSpan? slidingExpiration,
            CancellationToken token = default)
        {
            var options = new FusionCacheEntryOptions
            {
                Duration = slidingExpiration ?? Options.DefaultSlidingExpiration
            };

            return GetOrAddAsync(key, factory, options, token);
        }

        public virtual ValueTask RemoveAsync(string key, CancellationToken token = default)
        {
            return Cache.ExpireAsync(NormalizeKey(key), token: token);
        }

        public virtual ValueTask ClearAsync(CancellationToken token = default)
        {
            return Cache.ClearAsync(token: token);
        }

        protected virtual string NormalizeKey(string key)
        {
            return KeyPrefix + key;
        }

        protected virtual FusionCacheEntryOptions GetDefaultOptions()
        {
            return new FusionCacheEntryOptions
            {
                Duration = Options.DefaultSlidingExpiration,
                IsFailSafeEnabled = true
            };
        }
    }
}
