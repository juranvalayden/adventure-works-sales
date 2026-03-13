using Sales.Domain.Entities;

namespace Sales.Domain.Interfaces;

public interface ISalesOrderRepository
{
    Task<SalesOrderHeader?> GetSalesOrderHeaderAsync(CancellationToken cancellationToken = default);
    void Add(SalesOrderHeader saleOrderHeaderDto);
}