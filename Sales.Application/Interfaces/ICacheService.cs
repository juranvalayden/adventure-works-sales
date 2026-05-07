namespace Sales.Application.Interfaces;

public interface ICacheService : IDisposable
{
    Task<T?> GetOrAddAsync<T>(
        string cacheKey,
        Func<Task<T>> factory,
        TimeSpan? slidingExpiration,
        TimeSpan? absoluteExpiration,
        int? size,
        Action? onHit,
        CancellationToken cancellationToken = default);

    Task RemoveAsync(string cacheKey, CancellationToken cancellationToken = default);

    Task InvalidateAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default);

    Task InvalidateByPrefixAsync(string prefix, CancellationToken cancellationToken = default);

    Task InvalidateAllAsync(CancellationToken cancellationToken = default);
}