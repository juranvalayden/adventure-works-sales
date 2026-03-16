using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Sales.Application.Common.Helpers;
using Sales.Application.Configurations;
using Sales.Application.Dtos;
using Sales.Application.Interfaces;

namespace Sales.Application.Services;

public class SalesConsumerService : ISalesConsumerService
{
    private readonly ILogger<SalesConsumerService> _logger;
    private readonly MyRabbitOptions _myRabbitOptions;
    private readonly JsonSerializerOptionsWrapper _jsonSerializerOptionsWrapper;

    private IConnection? _connection;
    private IChannel? _channel;

    public SalesConsumerService(ILogger<SalesConsumerService> logger, IOptions<MyRabbitOptions> options, JsonSerializerOptionsWrapper jsonSerializerOptionsWrapper)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _jsonSerializerOptionsWrapper = jsonSerializerOptionsWrapper ?? throw new ArgumentNullException(nameof(jsonSerializerOptionsWrapper));
        _myRabbitOptions = options.Value;
    }

    public async Task<bool> StartConsumingAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = "localhost", // use "localhost" if API runs outside Docker
                Port = 5672,
                UserName = "guest",
                Password = "guest"
            };

            _connection ??= await factory.CreateConnectionAsync(cancellationToken);
            _channel ??= await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

            await _channel.QueueDeclareAsync(
                queue: "sales-orders",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken);

            _logger.LogInformation("Waiting to consume messages from 'sales-orders'...");

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (_, eventArgs) =>
            {
                await HandleMessageAsync(eventArgs.Body, eventArgs.DeliveryTag, cancellationToken);
            };

            await _channel.BasicConsumeAsync(
                queue: "sales-orders",
                autoAck: false, // manual ack
                consumer: consumer,
                cancellationToken: cancellationToken);

            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred while consuming messages.");
            throw;
        }
    }

    private async Task HandleMessageAsync(ReadOnlyMemory<byte> body, ulong deliveryTag, CancellationToken cancellationToken)
    {
        var message = Encoding.UTF8.GetString(body.Span);

        var salesOrderHeaderDto =
            JsonSerializer.Deserialize<SalesOrderHeaderDto>(message, _jsonSerializerOptionsWrapper.Options);

        if (salesOrderHeaderDto != null)
        {
            _logger.LogInformation("Received SalesOrderHeaderDto with {Id}", salesOrderHeaderDto.Id);
        }

        if (_channel != null)
        {
            await _channel.BasicAckAsync(deliveryTag, multiple: false, cancellationToken: cancellationToken);
        }

        // optional: simulate work
        await Task.Delay(2000, cancellationToken);
    }

    public async Task StopConsumingAsync(CancellationToken cancellationToken)
    {
        if (_channel != null)
        {
            await _channel.CloseAsync(cancellationToken);
            _channel = null;
        }

        if (_connection != null)
        {
            await _connection.CloseAsync(cancellationToken);
            _connection = null;
        }

        _logger.LogInformation("Stopped consuming from 'sales-orders'.");
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel != null)
        {
            await _channel.DisposeAsync();
            _channel = null;
        }

        if (_connection != null)
        {
            await _connection.DisposeAsync();
            _connection = null;
        }

        _logger.LogInformation("SalesConsumerService disposed successfully.");
    }
}