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
            var salesOrderHeader = await _salesDbContext
                .SalesOrderHeaders
                .Take(1)
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

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
        _salesDbContext.SalesOrderHeaders.Add(saleOrderHeaderDto);
    }
}
