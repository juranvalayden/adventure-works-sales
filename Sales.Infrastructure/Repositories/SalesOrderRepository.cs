using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sales.Domain.Entities;
using Sales.Domain.Interfaces;
using Sales.Infrastructure.Configurations.Persistence;

namespace Sales.Infrastructure.Repositories;

public class SalesOrderRepository : ISalesOrderRepository
{
    private readonly ILogger<SalesOrderRepository> _logger;
    private readonly SalesDbContext _salesDbContext;

    public SalesOrderRepository(ILogger<SalesOrderRepository> logger, SalesDbContext salesDbContext)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _salesDbContext = salesDbContext ?? throw new ArgumentNullException(nameof(salesDbContext));
    }

    public async Task<SalesOrderHeader?> GetSalesOrderHeaderAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var takenIds = _salesDbContext.SalesOrderHeaderTaken
                .Select(t => t.SalesOrderId);

            var salesOrderHeader = await _salesDbContext.SalesOrderHeaders
                .Where(soh => !takenIds.Contains(soh.Id))
                .FirstOrDefaultAsync(cancellationToken);

            if (salesOrderHeader == null) return null;

            _salesDbContext.SalesOrderHeaderTaken.Add(new SalesOrderHeaderTaken
            {
                SalesOrderId = salesOrderHeader.Id,
                DateTaken = DateTimeOffset.UtcNow
            });

            var hasSaved = await _salesDbContext.SaveChangesAsync(cancellationToken) > 0;

            if (!hasSaved)
            {
                _logger.LogWarning("Failed to save SalesOrderHeaderTaken for SalesOrderId {SalesOrderId}.", salesOrderHeader.Id);
            }

            return salesOrderHeader;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error occurred fetching the SalesOrderHeader.");
            return null;
        }
    }

    public void Add(SalesOrderHeader saleOrderHeaderDto)
    {
        try
        {
            _salesDbContext.SalesOrderHeaders.Add(saleOrderHeaderDto);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error occurred adding the SalesOrderHeader.");
        }
    }

    public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _salesDbContext.SaveChangesAsync(cancellationToken) > 0;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error occurred saving the SalesOrderHeader.");
            return false;
        }
    }
}
