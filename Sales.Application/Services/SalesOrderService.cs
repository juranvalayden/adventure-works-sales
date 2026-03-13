using Microsoft.Extensions.Logging;
using Sales.Application.Interfaces;
using Sales.Domain.Entities;
using Sales.Domain.Interfaces;

namespace Sales.Application.Services;

internal class SalesOrderService : ISalesOrderService
{
    private readonly ILogger<SalesOrderService> _logger;
    private readonly ISalesOrderRepository _salesOrderRepository;

    public SalesOrderService(ILogger<SalesOrderService> logger, ISalesOrderRepository salesOrderRepository)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _salesOrderRepository = salesOrderRepository ?? throw new ArgumentNullException(nameof(salesOrderRepository));
    }

    public async Task<SalesOrderHeader?> GetSalesOrderHeaderAsync(CancellationToken cancellationToken)
    {
        var entity = await _salesOrderRepository.GetSalesOrderHeaderAsync(cancellationToken);
    }

    public Task<bool> AddAsync(SalesOrderHeader saleOrderHeaderDto, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
