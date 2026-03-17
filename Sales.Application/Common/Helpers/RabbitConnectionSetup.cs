using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Sales.Application.Configurations;

namespace Sales.Application.Common.Helpers;

public class RabbitConnectionSetup : IRabbitService
{
    private readonly RabbitMqOptions _rabbitMqOptions;

    private IConnection? _connection;
    private IChannel? _channel;

    public RabbitConnectionSetup(IOptions<RabbitMqOptions> options)
    {
        _rabbitMqOptions = options.Value;
    }

    public async Task<IChannel> CreateChannelAsync(CancellationToken cancellationToken)
    {
        var factory = CreateConnectionFactory();
        _connection ??= await factory.CreateConnectionAsync(cancellationToken);
        _channel ??= await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await _channel.QueueDeclareAsync(
            queue: _rabbitMqOptions.Consumer.Queue ?? "sales-orders",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        return _channel;
    }

    private ConnectionFactory CreateConnectionFactory()
    {
        return new ConnectionFactory
        {
            HostName = _rabbitMqOptions.HostName ?? "localhost",
            Port = _rabbitMqOptions.Port ?? 5672,
            UserName = _rabbitMqOptions.Username ?? "guest",
            Password = _rabbitMqOptions.Password ?? "guest",
        };
    }
}