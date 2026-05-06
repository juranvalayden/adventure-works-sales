using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sales.Application.Interfaces;
using Sales.Domain.Entities;
using Sales.Domain.Pagination;
using Sales.Infrastructure.Configurations.Persistence;

namespace Sales.Infrastructure.Repositories;

internal class SalesOrderRepository(ILogger<SalesOrderRepository> logger, SalesDbContext salesDbContext) : ISalesOrderRepository
{
    private readonly ILogger<SalesOrderRepository> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly SalesDbContext _salesDbContext = salesDbContext ?? throw new ArgumentNullException(nameof(salesDbContext));

    public async Task<IEnumerable<SalesOrderHeader>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _salesDbContext
                .SalesOrderHeaders
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred getting all sales order headers from the db.");
            throw;
        }
    }

    public async Task<(IEnumerable<SalesOrderHeader>, PaginationMetadata)> GetSalesOrderHeadersAsync(string? salesOrderNumber, int pageNumber, int pageSize,
        CancellationToken cancellationToken = default)
    {
        IQueryable<SalesOrderHeader> collection = _salesDbContext.SalesOrderHeaders;

        if (!string.IsNullOrWhiteSpace(salesOrderNumber))
        {
            salesOrderNumber = salesOrderNumber.Trim();
            collection = collection.Where(s => s.SalesOrderNumber.Contains(salesOrderNumber));
        }

        var totalItemCount = await collection.CountAsync(cancellationToken: cancellationToken);

        var paginationMetadata = new PaginationMetadata(totalItemCount, pageSize, pageNumber);

        var collectionToReturn = await collection
            .OrderBy(c => c.SalesOrderNumber)
            .Skip(pageSize * (pageNumber - 1))
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new ValueTuple<IEnumerable<SalesOrderHeader>, PaginationMetadata>(collectionToReturn, paginationMetadata);
    }

    public async Task<SalesOrderHeader?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _salesDbContext
                .SalesOrderHeaders
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred getting sales order header by {Id} from the db.", id);
            throw;
        }
    }

    public SalesOrderHeader Add(SalesOrderHeader entity)
    {
        return _salesDbContext
            .SalesOrderHeaders
            .Add(entity)
            .Entity;
    }

    public SalesOrderHeader Update(SalesOrderHeader entity)
    {
        return _salesDbContext
            .SalesOrderHeaders
            .Update(entity)
            .Entity;
    }

    public SalesOrderHeader Delete(SalesOrderHeader entityForDeletion)
    {
        return _salesDbContext
            .SalesOrderHeaders
            .Remove(entityForDeletion)
            .Entity;
    }

    public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _salesDbContext.SaveChangesAsync(cancellationToken) > 0;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred saving sales order header to the db.");
            throw;
        }
    }
}
