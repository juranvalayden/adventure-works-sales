using Sales.Application.Dtos;

namespace Sales.Application.Interfaces;

public interface ISalesOrderService
{
    Task<SalesOrderHeaderDto?> GetSalesOrderHeaderAsync(CancellationToken cancellationToken = default);
    Task<bool> AddAsync(SalesOrderHeaderDto saleOrderHeaderDto, CancellationToken cancellationToken = default);
}