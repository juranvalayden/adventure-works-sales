using Sales.Application.Dtos;

namespace Sales.Application.Interfaces;

public interface ISalesPublisherService
{
    Task<bool> PublishAsync(SalesOrderHeaderDto saleOrderHeaderDto, CancellationToken cancellationToken = default);
}