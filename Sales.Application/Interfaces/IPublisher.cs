namespace Sales.Application.Interfaces;

public interface IPublisher<in T> : IAsyncDisposable
{
    Task<bool> PublishAsync(T message, string queue, CancellationToken cancellationToken = default);
}