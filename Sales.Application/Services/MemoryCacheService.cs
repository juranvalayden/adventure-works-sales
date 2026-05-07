using System.Collections.Concurrent;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sales.Application.Configurations;
using Sales.Application.Interfaces;

namespace Sales.Application.Services;

public sealed class MemoryCacheService : ICacheService
{
    private readonly SalesCachingOptions _options;
    private readonly ILogger<MemoryCacheService> _logger;
    private readonly IMemoryCache _memoryCache;
    private readonly Counter<int> _cacheHits;
    private readonly Counter<int> _cacheMisses;
    private readonly ConcurrentDictionary<string, byte> _trackedKeys = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, Lazy<Task<object>>> _activeTasks = new(StringComparer.Ordinal);
    private bool _disposed;

    public MemoryCacheService(ILogger<MemoryCacheService> logger, IMemoryCache memoryCache, IOptions<SalesCachingOptions> options, Meter meter)
    {
        ArgumentNullException.ThrowIfNull(meter);

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        _cacheHits = meter.CreateCounter<int>("cache.hits", "hits");
        _cacheMisses = meter.CreateCounter<int>("cache.misses", "misses");
    }

    public async Task<T?> GetOrAddAsync<T>(string cacheKey,
        Func<Task<T>> factory,
        TimeSpan? slidingExpiration,
        TimeSpan? absoluteExpiration,
        int? size,
        Action? onHit,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cacheKey);
        ArgumentNullException.ThrowIfNull(factory);
        ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var hasCacheItem = GetFromCache(cacheKey, out T? cachedItem, onHit);
            if (hasCacheItem && cachedItem is not null)
            {
                _logger.LogDebug("Cache hit for key {CacheKey}", cacheKey);
                return cachedItem;
            }

            _logger.LogDebug("Cache miss for key {CacheKey}", cacheKey);
            _cacheMisses.Add(1);

            var lazyTask = _activeTasks.GetOrAdd(cacheKey, _ => CreateLazyFactory(cacheKey, factory, slidingExpiration, absoluteExpiration, size));

            var boxed = await lazyTask.Value.ConfigureAwait(false);
            return (T)boxed;
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("GetOrAddAsync cancelled for key {CacheKey}", cacheKey);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetOrAddAsync failed for key {CacheKey}", cacheKey);
            throw;
        }
        finally
        {
            _activeTasks.TryRemove(cacheKey, out _);
        }
    }

    public async Task RemoveAsync(string cacheKey, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cacheKey);
        ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            _memoryCache.Remove(cacheKey);
            _trackedKeys.TryRemove(cacheKey, out _);
            _cacheMisses.Add(1);
            _logger.LogDebug("Removed cache key {CacheKey}", cacheKey);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("RemoveAsync cancelled for key {CacheKey}", cacheKey);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to remove cache key {CacheKey}", cacheKey);
        }

        await Task.CompletedTask.ConfigureAwait(false);
    }

    public async Task InvalidateAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default)
    {
        var cacheKeys = keys.ToList();

        if (cacheKeys.Count == 0) return;

        ThrowIfDisposed();

        foreach (var key in cacheKeys)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(key)) continue;

            try
            {
                _memoryCache.Remove(key);
                _trackedKeys.TryRemove(key, out _);
                _cacheMisses.Add(1);
                _logger.LogDebug("Invalidated cache key {CacheKey}", key);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("InvalidateAsync cancelled while processing key {CacheKey}", key);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to invalidate cache key {CacheKey}", key);
            }
        }

        await Task.CompletedTask.ConfigureAwait(false);
    }

    public async Task InvalidateByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prefix)) return;
        ThrowIfDisposed();

        var keys = _trackedKeys.Keys.Where(k => k.StartsWith(prefix, StringComparison.Ordinal)).ToArray();
        if (keys.Length == 0) return;

        foreach (var key in keys)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                _memoryCache.Remove(key);
                _trackedKeys.TryRemove(key, out _);
                _cacheMisses.Add(1);
                _logger.LogDebug("Invalidated cache key by prefix {CacheKey}", key);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("InvalidateByPrefixAsync cancelled while processing key {CacheKey}", key);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to invalidate cache key {CacheKey}", key);
            }
        }

        await Task.CompletedTask.ConfigureAwait(false);
    }

    public async Task InvalidateAllAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        var keys = _trackedKeys.Keys.ToArray();
        foreach (var key in keys)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                _memoryCache.Remove(key);
                _trackedKeys.TryRemove(key, out _);
                _cacheMisses.Add(1);
                _logger.LogDebug("Invalidated cache key {CacheKey}", key);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("InvalidateAllAsync cancelled while processing key {CacheKey}", key);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to invalidate cache key {CacheKey}", key);
            }
        }

        await Task.CompletedTask.ConfigureAwait(false);
    }

    public void Dispose()
    {
        if (_disposed) return;

        _disposed = true;
        _activeTasks.Clear();
        _trackedKeys.Clear();
        _logger.LogDebug("MemoryCacheService disposed");
    }

    private bool GetFromCache<T>(string cacheKey, out T? value, Action? onHit = null)
    {
        try
        {
            if (_memoryCache.TryGetValue(cacheKey, out T? cacheItem) && cacheItem is not null)
            {
                try { onHit?.Invoke(); } catch (Exception ex) { _logger.LogError(ex, "onHit callback failed for {CacheKey}", cacheKey); }
                _cacheHits.Add(1);
                value = cacheItem;
                return true;
            }

            value = default;
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetFromCache failed for key {CacheKey}", cacheKey);
            value = default;
            return false;
        }
    }

    private Lazy<Task<object>> CreateLazyFactory<T>(string cacheKey, Func<Task<T>> factory, TimeSpan? slidingExpiration, TimeSpan? absoluteExpiration, int? size = null)
    {
        return new Lazy<Task<object>>(async () =>
        {
            try
            {
                var createdCachedItem = await factory().ConfigureAwait(false);
                var memoryEntryOptions = CreateMemoryEntryOptions(cacheKey, slidingExpiration, absoluteExpiration, size);
                _memoryCache.Set(cacheKey, createdCachedItem, memoryEntryOptions);
                _logger.LogDebug("Cached item for key {CacheKey}", cacheKey);
                return createdCachedItem!;
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Factory cancelled for key {CacheKey}", cacheKey);
                _activeTasks.TryRemove(cacheKey, out _);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Factory failed for key {CacheKey}", cacheKey);
                _activeTasks.TryRemove(cacheKey, out _);
                throw;
            }
        });
    }

    private MemoryCacheEntryOptions CreateMemoryEntryOptions(string cacheKey, TimeSpan? slidingExpiration, TimeSpan? absoluteExpiration, int? size = null)
    {
        var memoryCacheEntryOptions = new MemoryCacheEntryOptions
        {
            SlidingExpiration = slidingExpiration ?? _options.SlidingExpiration,
            AbsoluteExpirationRelativeToNow = absoluteExpiration ?? _options.AbsoluteExpiration
        };

        if (size.HasValue) memoryCacheEntryOptions.SetSize(size.Value);

        _trackedKeys.TryAdd(cacheKey, 0);

        memoryCacheEntryOptions.RegisterPostEvictionCallback((evictedKeyObject, _, evictionReason, _) =>
        {
            try
            {
                if (evictedKeyObject is string evictedKey)
                {
                    _trackedKeys.TryRemove(evictedKey, out _);
                }

                _logger.LogInformation("Cache entry evicted. Key={Key}, Reason={Reason}", evictedKeyObject, evictionReason);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Eviction callback error for {Key}", evictedKeyObject);
            }
        });

        return memoryCacheEntryOptions;
    }

    private void ThrowIfDisposed()
    {
        if (!_disposed) return;
        throw new ObjectDisposedException(nameof(MemoryCacheService));
    }
}
