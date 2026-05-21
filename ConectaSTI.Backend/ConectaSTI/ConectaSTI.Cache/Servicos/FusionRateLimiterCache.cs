using ConectaSTI.Dominio.Interfaces;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.RateLimiting;
using ZiggyCreatures.Caching.Fusion;

namespace ConectaSTI.Cache.Servicos;

public class FusionRateLimiterCache : IRateLimiterCache
{
    private sealed class RateLimiterHolder : IDisposable
    {
        public RateLimiterHolder(FixedWindowRateLimiter limiter)
        {
            Limiter = limiter;
        }

        public FixedWindowRateLimiter Limiter { get; }

        public void Dispose()
        {
            Limiter.Dispose();
        }
    }

    private static readonly TimeSpan CleanupInterval = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan MinimumTtl = TimeSpan.FromMinutes(5);

    private readonly IFusionCache _cache;
    private readonly ConcurrentDictionary<string, Lazy<RateLimiterHolder>> _limiters = new(StringComparer.Ordinal);
    private readonly object _cleanupLock = new();
    private long _nextCleanupTicks;

    public FusionRateLimiterCache(IFusionCache cache)
    {
        _cache = cache;
    }

    public FixedWindowRateLimiter GetOrCreate(string key, int permitLimit, TimeSpan window)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("A chave do rate limiter deve ser informada.", nameof(key));
        }

        if (permitLimit <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(permitLimit), "O limite de permissoes deve ser maior que zero.");
        }

        if (window <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(window), "A janela do rate limiter deve ser maior que zero.");
        }

        RefreshMarker(key, window);
        CleanupExpiredLimitersIfNeeded();

        var lazyHolder = _limiters.GetOrAdd(key, _ => new Lazy<RateLimiterHolder>(
            () => new RateLimiterHolder(CreateLimiter(permitLimit, window)),
            LazyThreadSafetyMode.ExecutionAndPublication));

        try
        {
            return lazyHolder.Value.Limiter;
        }
        catch
        {
            if (_limiters.TryRemove(key, out var removed) && removed.IsValueCreated)
            {
                removed.Value.Dispose();
            }

            throw;
        }
    }

    public void Remove(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        _cache.Remove(GetMarkerKey(key));

        if (_limiters.TryRemove(key, out var lazyHolder) && lazyHolder.IsValueCreated)
        {
            lazyHolder.Value.Dispose();
        }
    }

    private void CleanupExpiredLimitersIfNeeded()
    {
        long nowTicks = DateTimeOffset.UtcNow.UtcTicks;
        long nextCleanupTicks = Interlocked.Read(ref _nextCleanupTicks);

        if (nowTicks < nextCleanupTicks)
        {
            return;
        }

        lock (_cleanupLock)
        {
            nowTicks = DateTimeOffset.UtcNow.UtcTicks;
            nextCleanupTicks = Interlocked.Read(ref _nextCleanupTicks);

            if (nowTicks < nextCleanupTicks)
            {
                return;
            }

            foreach (var entry in _limiters)
            {
                string markerKey = GetMarkerKey(entry.Key);
                bool isActive = _cache.GetOrDefault<bool?>(markerKey, null) == true;
                if (isActive)
                {
                    continue;
                }

                if (_limiters.TryRemove(entry.Key, out var lazyHolder) && lazyHolder.IsValueCreated)
                {
                    lazyHolder.Value.Dispose();
                }
            }

            Interlocked.Exchange(ref _nextCleanupTicks, DateTimeOffset.UtcNow.Add(CleanupInterval).UtcTicks);
        }
    }

    private void RefreshMarker(string key, TimeSpan window)
    {
        _cache.Set(GetMarkerKey(key), true, new FusionCacheEntryOptions(GetEntryTtl(window)));
    }

    private static TimeSpan GetEntryTtl(TimeSpan window)
    {
        TimeSpan doubledWindow = TimeSpan.FromTicks(window.Ticks * 2);
        return doubledWindow > MinimumTtl ? doubledWindow : MinimumTtl;
    }

    private static string GetMarkerKey(string key)
    {
        return $"ratelimit-marker:{key}";
    }

    private static FixedWindowRateLimiter CreateLimiter(int permitLimit, TimeSpan window)
    {
        return new FixedWindowRateLimiter(new FixedWindowRateLimiterOptions
        {
            PermitLimit = permitLimit,
            Window = window,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0,
            AutoReplenishment = true
        });
    }
}
