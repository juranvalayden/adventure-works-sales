using Sales.Domain.Entities;

namespace Sales.Application.Interfaces;

public interface ISalesOrderService
{
    Task<SalesOrderHeader?> GetSalesOrderHeaderAsync(CancellationToken cancellationToken = default);
    Task<bool> AddAsync(SalesOrderHeader saleOrderHeaderDto, CancellationToken cancellationToken = default);
}