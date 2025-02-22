using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace AbpCachingPlayground.Caching;

public class MetricsDistributedCache : IDistributedCache
{
    private readonly IDistributedCache _cache;
    private readonly CacheMetrics _metrics;

    public MetricsDistributedCache(
        IDistributedCache cache,
        CacheMetrics metrics)
    {
        _cache = cache;
        _metrics = metrics;
    }

    public byte[] Get(string key)
    {
        var sw = Stopwatch.StartNew();
        var value = _cache.Get(key);
        _metrics.RecordDuration(sw.ElapsedMilliseconds);

        if (value != null)
        {
            _metrics.RecordHit();
        }
        else
        {
            _metrics.RecordMiss();
        }

        return value;
    }

    public async Task<byte[]?> GetAsync(string key, CancellationToken token = default)
    {
        var sw = Stopwatch.StartNew();
        var value = await _cache.GetAsync(key, token);
        _metrics.RecordDuration(sw.ElapsedMilliseconds);

        if (value != null)
        {
            _metrics.RecordHit();
        }
        else
        {
            _metrics.RecordMiss();
        }

        return value;
    }

    public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
    {
        var sw = Stopwatch.StartNew();

        try
        {
            _cache.Set(key, value, options);
            _metrics.RecordDuration(sw.ElapsedMilliseconds);
        }
        catch (Exception)
        {
            _metrics.RecordError();
            throw;
        }
    }

    public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options,
        CancellationToken token = default)
    {
        var sw = Stopwatch.StartNew();

        try
        {
            await _cache.SetAsync(key, value, options, token);
            _metrics.RecordDuration(sw.ElapsedMilliseconds);
        }
        catch (Exception)
        {
            _metrics.RecordError();
            throw;
        }
    }

    public void Refresh(string key)
    {
        var sw = Stopwatch.StartNew();

        try
        {
            _cache.Refresh(key);
            _metrics.RecordDuration(sw.ElapsedMilliseconds);
        }
        catch (Exception)
        {
            _metrics.RecordError();
            throw;
        }
    }

    public async Task RefreshAsync(string key, CancellationToken token = default)
    {
        var sw = Stopwatch.StartNew();

        try
        {
            await _cache.RefreshAsync(key, token);
            _metrics.RecordDuration(sw.ElapsedMilliseconds);
        }
        catch (Exception)
        {
            _metrics.RecordError();
            throw;
        }
    }

    public void Remove(string key)
    {
        var sw = Stopwatch.StartNew();

        try
        {
            _cache.Remove(key);
            _metrics.RecordDuration(sw.ElapsedMilliseconds);
            _metrics.RecordEviction();
        }
        catch (Exception)
        {
            _metrics.RecordError();
            throw;
        }
    }

    public async Task RemoveAsync(string key, CancellationToken token = default)
    {
        var sw = Stopwatch.StartNew();

        try
        {
            await _cache.RemoveAsync(key, token);
            _metrics.RecordDuration(sw.ElapsedMilliseconds);
            _metrics.RecordEviction();
        }
        catch (Exception)
        {
            _metrics.RecordError();
            throw;
        }
    }
}