using System.Diagnostics.Metrics;
using Microsoft.Extensions.Options;
using Sales.Application.Common;
using Sales.Application.Configurations;
using Sales.Application.Interfaces;
using Sales.Domain.Entities;
using Sales.Domain.Pagination;

namespace Sales.Infrastructure.Repositories;

public class SalesOrderCacheRepository : ISalesOrderRepository
{
    private readonly ICacheService _cacheService;
    private readonly ISalesOrderRepository _innerRepository;
    private readonly TimeSpan _slidingExpiration;
    private readonly TimeSpan _absoluteExpiration;
    private readonly bool _cacheIsDisabled;
    private readonly Counter<long> _cacheHits;
    private readonly Counter<long> _cacheMisses;
    private readonly SalesCachingOptions _options;

    public SalesOrderCacheRepository(ICacheService cacheService,
        ISalesOrderRepository innerRepository,
        Meter meter,
        IOptions<SalesCachingOptions> options)
    {
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _innerRepository = innerRepository ?? throw new ArgumentNullException(nameof(innerRepository));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _cacheIsDisabled = !options.Value.Enabled;
        _slidingExpiration = options.Value.SlidingExpiration;
        _absoluteExpiration = options.Value.AbsoluteExpiration;
        _cacheHits = meter.CreateCounter<long>("salesorder_cache_hits");
        _cacheMisses = meter.CreateCounter<long>("salesorder_cache_misses");
    }

    public async Task<IEnumerable<SalesOrderHeader>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        if (_cacheIsDisabled)
            return await _innerRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);

        var salesOrderHeader = await _cacheService
            .GetOrAddAsync(
                cacheKey: CacheKeys.AllPrefix,
                factory: async () =>
                {
                    _cacheMisses.Add(1);
                    return await _innerRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
                },
                slidingExpiration: _slidingExpiration,
                absoluteExpiration: _absoluteExpiration,
                size: null,
                onHit: () => _cacheHits.Add(1),
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return salesOrderHeader ?? [];
    }

    public async Task<(IEnumerable<SalesOrderHeader>, PaginationMetadata)> GetSalesOrderHeadersAsync(
        string? salesOrderNumber,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (_cacheIsDisabled)
        {
            return await _innerRepository
                .GetSalesOrderHeadersAsync(salesOrderNumber, pageNumber, pageSize, cancellationToken)
                .ConfigureAwait(false);
        }

        var cacheKey = string.IsNullOrWhiteSpace(salesOrderNumber)
            ? CacheKeys.AllPrefix
            : CacheKeys.BySalesOrderNumber(salesOrderNumber);

        var paginationData = await _cacheService
            .GetOrAddAsync(
                cacheKey: cacheKey,
                factory: async () =>
                {
                    _cacheMisses.Add(1);
                    return await _innerRepository
                        .GetSalesOrderHeadersAsync(salesOrderNumber, pageNumber, pageSize, cancellationToken)
                        .ConfigureAwait(false);
                },
                slidingExpiration: _slidingExpiration,
                absoluteExpiration: _absoluteExpiration,
                size: null,
                onHit: () => _cacheHits.Add(1),
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return paginationData;
    }

    public async Task<SalesOrderHeader?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        if (_cacheIsDisabled)
            return await _innerRepository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);

        var cacheKey = CacheKeys.ById(id);

        var saleOrderHeader = await _cacheService
            .GetOrAddAsync(
                cacheKey: cacheKey,
                factory: async () =>
                {
                    _cacheMisses.Add(1);
                    return await _innerRepository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
                },
                slidingExpiration: _slidingExpiration,
                absoluteExpiration: _absoluteExpiration,
                size: null,
                onHit: () => _cacheHits.Add(1),
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return saleOrderHeader;
    }

    public SalesOrderHeader Add(SalesOrderHeader entityForCreation)
    {
        if (entityForCreation == null) throw new ArgumentNullException(nameof(entityForCreation));

        var created = _innerRepository.Add(entityForCreation);

        _ = InvalidateCacheAsync([CacheKeys.AllPrefix, CacheKeys.BySalesOrderNumber(entityForCreation.SalesOrderNumber)]);

        return created;
    }

    public SalesOrderHeader Update(SalesOrderHeader entityForUpdate)
    {
        if (entityForUpdate == null) throw new ArgumentNullException(nameof(entityForUpdate));

        var updated = _innerRepository.Update(entityForUpdate);

        IEnumerable<string> cacheKeys = [
            CacheKeys.AllPrefix, 
            CacheKeys.BySalesOrderNumber(entityForUpdate.SalesOrderNumber), 
            CacheKeys.ById(entityForUpdate.Id)];

        _ = InvalidateCacheAsync(cacheKeys);

        return updated;
    }

    public SalesOrderHeader Delete(SalesOrderHeader entityForDeletion)
    {
        if (entityForDeletion == null) throw new ArgumentNullException(nameof(entityForDeletion));

        var deleted = _innerRepository.Delete(entityForDeletion);

        // Fire-and-forget invalidation for affected keys.
        IEnumerable<string> cacheKeys = [
            CacheKeys.AllPrefix,
            CacheKeys.BySalesOrderNumber(entityForDeletion.SalesOrderNumber),
            CacheKeys.ById(entityForDeletion.Id)
        ];

        _ = InvalidateCacheAsync(cacheKeys);

        return deleted;
    }

    public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var result = await _innerRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        if (result && !_cacheIsDisabled)
        {
            // Ensure cache is invalidated after successful save.
            // Await here to guarantee consistency; change to fire-and-forget if you prefer not to block.
            await InvalidateCacheAsync([CacheKeys.AllPrefix], cancellationToken).ConfigureAwait(false);
        }

        return result;
    }

    private async Task InvalidateCacheAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default)
    {
        if (keys?.Count() == 0) return;
        cancellationToken.ThrowIfCancellationRequested();

        foreach (var key in keys)
        {
            if (string.IsNullOrWhiteSpace(key)) continue;

            try
            {
                await _cacheService.RemoveAsync(key, cancellationToken).ConfigureAwait(false);
                _cacheMisses.Add(1);
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
            }
        }
    }
}