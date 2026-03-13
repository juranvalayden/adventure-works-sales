namespace Sales.Application.Interfaces;

public interface ISalesConsumerService
{
    Task<bool> ConsumerAsync(CancellationToken cancellationToken = default);
}