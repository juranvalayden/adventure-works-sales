namespace Sales.Application.Interfaces;

public interface ISalesConsumerService : IAsyncDisposable
{
    Task<bool> StartConsumingAsync(CancellationToken cancellationToken = default);
    Task StopConsumingAsync(CancellationToken cancellationToken);
}