using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using StackExchange.Redis.Profiling;

namespace AbpCachingPlayground;

public static class RedisTelemetryExtensions
{
    public static IHostApplicationBuilder AddRedisMetrics(this IHostApplicationBuilder builder)
    {
        // Register the Redis metrics service
        builder.Services.AddSingleton<RedisMetricsCollector>();
        builder.Services.AddHostedService(sp => sp.GetRequiredService<RedisMetricsCollector>());

        // Add OpenTelemetry metrics for Redis
        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddMeter(RedisMetricsCollector.MeterName);
            });

        return builder;
    }
}

public class RedisMetricsCollector : BackgroundService
{
    public const string MeterName = "Redis.Metrics";
    private readonly Counter<long> _commandCounter;
    private readonly Histogram<double> _commandDuration;
    private readonly ILogger<RedisMetricsCollector> _logger;
    private readonly IConnectionMultiplexer _redis;
    private ProfilingSession? _currentSession;

    public RedisMetricsCollector(
        IConnectionMultiplexer redis,
        ILogger<RedisMetricsCollector> logger)
    {
        _redis = redis;
        _logger = logger;

        // Create meter and instruments
        var meter = new Meter(MeterName, "1.0.0");

        // Counter for total commands
        _commandCounter = meter.CreateCounter<long>(
            "redis.commands.total",
            "commands",
            "Total number of Redis commands executed");

        // Histogram for command duration
        _commandDuration = meter.CreateHistogram<double>(
            "redis.commands.duration",
            "ms",
            "Duration of Redis commands");

        if (CanExecuteInfoCommand())
        {
            // Observable gauges for Redis server stats
            meter.CreateObservableGauge(
                "redis.clients.connected",
                GetConnectedClients,
                "clients",
                "Number of connected clients");

            meter.CreateObservableGauge(
                "redis.memory.used_bytes",
                GetMemoryUsage,
                "bytes",
                "Redis memory usage in bytes");

            meter.CreateObservableGauge(
                "redis.keys.total",
                GetTotalKeys,
                "keys",
                "Total number of keys in Redis database");

            meter.CreateObservableGauge(
                "redis.cache.hits",
                GetCacheHits,
                "hits",
                "Number of successful key lookups");

            meter.CreateObservableGauge(
                "redis.cache.misses",
                GetCacheMisses,
                "misses",
                "Number of failed key lookups");

            meter.CreateObservableGauge(
                "redis.cache.evictions",
                GetEvictions,
                "evictions",
                "Number of keys that have been evicted due to memory limit");

            meter.CreateObservableGauge(
                "redis.cache.hit_ratio",
                GetCacheHitRatio,
                "{percent}",
                "Percentage of cache hits out of total lookups");

            _logger.LogInformation("Redis server INFO metrics enabled");
        }
        else
        {
            _logger.LogWarning("Redis server INFO metrics not available - admin permissions required");
        }

        // Enable profiling if connection supports it
        if (_redis is not ConnectionMultiplexer multiplexer)
        {
            return;
        }

        _currentSession = new ProfilingSession();
        multiplexer.RegisterProfiler(() => _currentSession);
    }

    private double GetCacheHitRatio()
    {
        try
        {
            var server = _redis.GetServer(_redis.GetEndPoints()[0]);
            var info = server.Info("stats");
            long hits = 0;
            long misses = 0;

            foreach (var group in info)
            {
                foreach (var stat in group)
                {
                    if (stat.Key == "keyspace_hits")
                    {
                        hits = long.Parse(stat.Value);
                    }
                    else if (stat.Key == "keyspace_misses")
                    {
                        misses = long.Parse(stat.Value);
                    }
                }
            }

            // Calculate hit ratio (avoid division by zero)
            long total = hits + misses;
            return total > 0 ? (double)hits / total * 100 : 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating Redis cache hit ratio metric");
        }

        return 0;
    }

    private long GetCacheHits()
    {
        try
        {
            var server = _redis.GetServer(_redis.GetEndPoints()[0]);
            var info = server.Info("stats");

            foreach (var group in info)
            {
                foreach (var stat in group)
                {
                    if (stat.Key == "keyspace_hits")
                    {
                        return long.Parse(stat.Value);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error collecting Redis cache hits metric");
        }

        return 0;
    }

    private long GetCacheMisses()
    {
        try
        {
            var server = _redis.GetServer(_redis.GetEndPoints()[0]);
            var info = server.Info("stats");

            foreach (var group in info)
            {
                foreach (var stat in group)
                {
                    if (stat.Key == "keyspace_misses")
                    {
                        return long.Parse(stat.Value);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error collecting Redis cache misses metric");
        }

        return 0;
    }

    private long GetEvictions()
    {
        try
        {
            var server = _redis.GetServer(_redis.GetEndPoints()[0]);
            var info = server.Info("stats");

            foreach (var group in info)
            {
                foreach (var stat in group)
                {
                    if (stat.Key == "evicted_keys")
                    {
                        return long.Parse(stat.Value);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error collecting Redis evictions metric");
        }

        return 0;
    }

    private void ProcessProfiledCommands(IEnumerable<IProfiledCommand> commands)
    {
        if (commands == null)
        {
            return;
        }

        foreach (var command in commands)
        {
            _commandCounter.Add(1, new KeyValuePair<string, object?>("command", command.Command));
            _commandDuration.Record(command.ElapsedTime.TotalMilliseconds,
                new KeyValuePair<string, object?>("command", command.Command),
                new KeyValuePair<string, object?>("db", command.Db),
                new KeyValuePair<string, object?>("endpoint", command.EndPoint?.ToString() ?? "unknown"));

            _logger.LogTrace("Redis Command: {Command}, Duration: {Duration}ms, Endpoint: {Endpoint}",
                command.Command, command.ElapsedTime.TotalMilliseconds, command.EndPoint);
        }
    }

    private bool CanExecuteInfoCommand()
    {
        try
        {
            if (_redis.GetEndPoints().Length == 0)
            {
                return false;
            }

            var server = _redis.GetServer(_redis.GetEndPoints()[0]);
            // Just try to get a small section of info to test permissions
            var info = server.Info("server");
            return true;
        }
        catch (RedisCommandException ex) when (ex.Message.Contains("admin mode"))
        {
            _logger.LogWarning("Redis INFO command requires admin privileges: {Message}", ex.Message);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Cannot execute Redis INFO command: {Message}", ex.Message);
            return false;
        }
    }

    private long GetConnectedClients()
    {
        try
        {
            var server = _redis.GetServer(_redis.GetEndPoints()[0]);
            var info = server.Info();

            foreach (var group in info)
            {
                foreach (var stat in group)
                {
                    if (stat.Key == "connected_clients")
                    {
                        return long.Parse(stat.Value);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error collecting Redis connected clients metric");
        }

        return 0;
    }

    private double GetMemoryUsage()
    {
        try
        {
            var server = _redis.GetServer(_redis.GetEndPoints()[0]);
            var info = server.Info();

            foreach (var group in info)
            {
                foreach (var stat in group)
                {
                    if (stat.Key == "used_memory")
                    {
                        return double.Parse(stat.Value);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error collecting Redis memory usage metric");
        }

        return 0;
    }

    private long GetTotalKeys()
    {
        try
        {
            // Get key counts for each database
            var keyCount = 0L;
            foreach (var endpoint in _redis.GetEndPoints())
            {
                var svr = _redis.GetServer(endpoint);
                if (!svr.IsConnected)
                {
                    continue;
                }

                for (var i = 0; i < 16; i++) // Default Redis has 16 databases
                {
                    try
                    {
                        keyCount += svr.DatabaseSize(i);
                    }
                    catch
                    {
                        // Database might not exist
                    }
                }
            }

            return keyCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error collecting Redis key count metric");
        }

        return 0;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Redis metrics collector started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Create a new profiling session
                if (_currentSession == null)
                {
                    _currentSession = new ProfilingSession();

                    // Register our profiler function that returns the current session
                    if (_redis is ConnectionMultiplexer multiplexer)
                    {
                        multiplexer.RegisterProfiler(() => _currentSession);
                    }
                }

                // Wait for some time to collect profiling data
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

                // Finish profiling and process the commands
                if (_currentSession != null)
                {
                    var profiledCommands = _currentSession.FinishProfiling();
                    ProcessProfiledCommands(profiledCommands);

                    // Create a new session for the next interval
                    _currentSession = new ProfilingSession();
                    if (_redis is ConnectionMultiplexer multiplexer)
                    {
                        multiplexer.RegisterProfiler(() => _currentSession);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Redis metrics collector");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        _logger.LogInformation("Redis metrics collector stopped");
    }
}