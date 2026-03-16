using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Sales.Application.Configurations;

namespace Sales.Application.Common.Helpers;

public class RabbitConnectionSetup
{
    private readonly IOptions<MyRabbitOptions> _options;
    private ConnectionFactory _connectionFactory;
    private IConnection? _connection = null;
    private IChannel? _channel = null;
    
    public IConnection Connection { get; private set; }
    public IChannel Channel { get; private set; }

    private readonly MyRabbitOptions _rabbitOptions;

    public RabbitConnectionSetup(IOptions<MyRabbitOptions> options, ConnectionFactory connectionFactory)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));

        _rabbitOptions = options.Value;
    }

    public async Task<IChannel> CreateChannel(CancellationToken cancellationToken)
    {
        _connectionFactory ??= CreateFactory();

        _connection ??= await _connectionFactory.CreateConnectionAsync(cancellationToken);
        _channel ??= await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await _channel.QueueDeclareAsync(
            queue: _rabbitOptions.Consumer.Queue ?? "sales-orders",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        return _channel;
    }

    private ConnectionFactory CreateFactory()
    {
        return new ConnectionFactory
        {
            Port = _rabbitOptions.Port ?? 5672,
            UserName = _rabbitOptions.Username ?? "guest",
            Password = _rabbitOptions.Password ?? "guest",
            HostName = _rabbitOptions.HostName ?? "localhost"
        };
    }
}
