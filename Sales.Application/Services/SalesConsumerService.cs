using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Sales.Application.Common.Helpers;
using Sales.Application.Dtos;
using Sales.Application.Interfaces;

namespace Sales.Application.Services;

public class SalesConsumerService : ISalesConsumerService
{
    private readonly ILogger<SalesConsumerService> _logger;
    private readonly JsonSerializerOptionsWrapper _jsonSerializerOptionsWrapper;

    public SalesConsumerService(ILogger<SalesConsumerService> logger, JsonSerializerOptionsWrapper jsonSerializerOptionsWrapper)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _jsonSerializerOptionsWrapper = jsonSerializerOptionsWrapper ?? throw new ArgumentNullException(nameof(jsonSerializerOptionsWrapper));
    }

    private IConnection? _connection;
    private IChannel? _channel;

    public async Task<bool> ConsumerAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                Port = 5672,
                UserName = "guest",
                Password = "guest"
            };

            // Create connection and channel once, keep them alive
            _connection ??= await factory.CreateConnectionAsync(cancellationToken);
            _channel ??= await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

            await _channel.QueueDeclareAsync(
                queue: "messages",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken);

            _logger.LogInformation("Waiting to consume...");

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (_, eventArgs) =>
            {
                await HandleMessageAsync(eventArgs.Body, eventArgs.DeliveryTag, cancellationToken);
            };

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

        // acknowledge the message
        await _channel.BasicAckAsync(deliveryTag, multiple: false, cancellationToken: cancellationToken);

        // optional: simulate work
        await Task.Delay(2000, cancellationToken);
    }
}