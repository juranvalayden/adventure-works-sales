using Sales.Domain.Entities;

namespace Sales.Application.Interfaces;

public interface ISalesProducerService
{
    Task<bool> PublishAsync(SalesOrderHeader? saleOrderHeaderDto, CancellationToken cancellationToken = default);
}