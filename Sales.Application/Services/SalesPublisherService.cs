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
    private readonly IRabbitService _rabbitService;
    private IChannel? _channel;

    public SalesPublisherService(ILogger<SalesPublisherService> logger, JsonSerializerOptionsWrapper jsonSerializerOptionsWrapper, IRabbitService rabbitService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _jsonSerializerOptionsWrapper = jsonSerializerOptionsWrapper ?? throw new ArgumentNullException(nameof(jsonSerializerOptionsWrapper));
        _rabbitService = rabbitService ?? throw new ArgumentNullException(nameof(rabbitService));
    }

    public async Task<bool> PublishAsync(SalesOrderHeaderDto saleOrderHeaderDto, CancellationToken cancellationToken = default)
    {
        //var factory = new ConnectionFactory
        //{
        //    HostName = "localhost",   // or "rabbitmq" if inside Docker network
        //    Port = 5672,
        //    UserName = "guest",
        //    Password = "guest"
        //};

        try
        {
            //await using var connection = await factory.CreateConnectionAsync(cancellationToken);
            //await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

            //await channel.QueueDeclareAsync(
            //    queue: "sales-orders",
            //    durable: true,
            //    exclusive: false,
            //    autoDelete: false,
            //    arguments: null,
            //    cancellationToken: cancellationToken);

            _channel ??= await _rabbitService.CreateChannelAsync(cancellationToken);

            var message = JsonSerializer.Serialize(saleOrderHeaderDto, _jsonSerializerOptionsWrapper.Options);
            var body = Encoding.UTF8.GetBytes(message);

            var props = new BasicProperties { Persistent = true };

            await _channel.BasicPublishAsync(
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