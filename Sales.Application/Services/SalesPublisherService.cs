using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Sales.Application.Common.Helpers;
using Sales.Application.Dtos;
using Sales.Application.Interfaces;

namespace Sales.Application.Services;

public class SalesPublisherService : ISalesPublisherService
{
    private readonly ILogger<SalesPublisherService> _logger;
    private readonly JsonSerializerOptionsWrapper _jsonSerializerOptionsWrapper;
    public SalesPublisherService(ILogger<SalesPublisherService> logger, JsonSerializerOptionsWrapper jsonSerializerOptionsWrapper)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _jsonSerializerOptionsWrapper = jsonSerializerOptionsWrapper ?? throw new ArgumentNullException(nameof(jsonSerializerOptionsWrapper));
    }

    public async Task<bool> ConsumerAsync(CancellationToken cancellationToken = default)
    {
        var factory = new ConnectionFactory { HostName = "localhost" };

        await using var connection = await factory.CreateConnectionAsync(cancellationToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: "messages",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        _logger.LogInformation("Waiting to consume...");

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (sender, eventArgs) =>
        {
            var body = eventArgs.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            var salesOrderHeaderDto =
                JsonSerializer.Deserialize<SalesOrderHeaderDto>(message, _jsonSerializerOptionsWrapper.Options);

            if (salesOrderHeaderDto != null)
            {
                _logger.LogInformation("Received SalesOrderHeaderDto with {Id}", salesOrderHeaderDto.Id);
            }

            await ((AsyncEventingBasicConsumer)sender).Channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false,
                cancellationToken: cancellationToken);
        };

        _ = await channel.BasicConsumeAsync("messages", autoAck: false, consumer, cancellationToken);

        return true;
    }

    public async Task<bool> PublishAsync(SalesOrderHeaderDto saleOrderHeaderDto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Publishing SalesOrderHeader: {Id}", saleOrderHeaderDto.Id);

        var factory = new ConnectionFactory
        {
            HostName = "localhost",   // or "rabbitmq" if inside Docker network
            Port = 5672,
            UserName = "guest",
            Password = "guest"
        };

        try
        {
            await using var connection = await factory.CreateConnectionAsync(cancellationToken);
            await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

            await channel.QueueDeclareAsync(
                queue: "sales-orders",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken);

            var message = JsonSerializer.Serialize(saleOrderHeaderDto, _jsonSerializerOptionsWrapper.Options);
            var body = Encoding.UTF8.GetBytes(message);

            var props = new BasicProperties { Persistent = true };

            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: "sales-orders",
                mandatory: true,
                basicProperties: props,
                body: body,
                cancellationToken: cancellationToken);

            _logger.LogInformation("Successfully published SalesOrderHeader {Id}", saleOrderHeaderDto.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish SalesOrderHeader {Id}", saleOrderHeaderDto.Id);
            throw; // or implement retry/backoff here
        }
    }
}