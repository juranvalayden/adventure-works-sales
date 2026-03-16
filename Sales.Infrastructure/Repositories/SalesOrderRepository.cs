using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sales.Domain.Entities;
using Sales.Domain.Interfaces;
using Sales.Infrastructure.Configurations.Persistence;

namespace Sales.Infrastructure.Repositories;

public class SalesOrderRepository(ILogger<SalesOrderRepository> logger, SalesDbContext salesDbContext) : ISalesOrderRepository
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
