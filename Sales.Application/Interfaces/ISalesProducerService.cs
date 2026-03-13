using Sales.Application.Dtos;

namespace Sales.Application.Interfaces;

public interface ISalesProducerService
{
    Task<bool> PublishAsync(SalesOrderHeaderDto saleOrderHeaderDto, CancellationToken cancellationToken = default);
}