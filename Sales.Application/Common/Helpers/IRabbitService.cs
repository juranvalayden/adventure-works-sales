using RabbitMQ.Client;

namespace Sales.Application.Common.Helpers;

public interface IRabbitService
{
    Task<IChannel> CreateChannelAsync(CancellationToken cancellationToken = default);
}