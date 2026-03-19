namespace Sales.Application.Interfaces;

public interface IConsumer<T> : IAsyncDisposable
{
    Task<bool> StartConsumingAsync(string queue, CancellationToken cancellationToken = default);
    Task StopConsumingAsync(CancellationToken cancellationToken = default);
}