using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Sales.Application.Interfaces;

namespace Sales.Application.Common.Helpers;

public class Publisher<T>(ILogger<Publisher<T>> logger, IConnectionFactory connectionFactory, SerializationWrapper serializationWrapper)
    : IPublisher<T> where T : class
{
    private IChannel? _channel;
    private IConnection? _connection;
    private readonly ILogger<Publisher<T>> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IConnectionFactory _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    private readonly SerializationWrapper _serializationWrapper = serializationWrapper ?? throw new ArgumentNullException(nameof(serializationWrapper));
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    private async Task<IChannel> CreateChannelAsync(string queue, CancellationToken cancellationToken = default)
    {
        if (_connection is not { IsOpen: true })
        {
            _connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        }

        if (_channel is { IsOpen: true }) return _channel;

        _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await _channel.QueueDeclareAsync(
            queue: queue,
            exclusive: false,
            durable: true,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        return _channel;
    }

    public async Task<bool> PublishAsync(T message, string queue, CancellationToken cancellationToken = default)
    {
        try
        {
            await _semaphore.WaitAsync(cancellationToken);

            _channel ??= await CreateChannelAsync(queue, cancellationToken);

            if (_channel is not { IsOpen: true })
            {
                _logger.LogWarning("Channel is closed, recreating...");
                _channel = await CreateChannelAsync(queue, cancellationToken);
            }

            var serializedMessage = JsonSerializer.Serialize(message, _serializationWrapper.Options);
            var body = Encoding.UTF8.GetBytes(serializedMessage);
            var basicProperties = new BasicProperties { Persistent = true };

            await _channel.BasicPublishAsync(
                body: body,
                mandatory: true,
                basicProperties: basicProperties,
                exchange: string.Empty,
                routingKey: queue,
                cancellationToken: cancellationToken);

            return true;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error occurred when publishing message");
            return false;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (_channel is not null)
            {
                await _channel.CloseAsync();
                await _channel.DisposeAsync();
                _channel = null;
            }

            if (_connection is not null)
            {
                await _connection.CloseAsync();
                await _connection.DisposeAsync();
                _connection = null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disposing RabbitMQ sender resources");
        }
        finally
        {
            _semaphore.Dispose();
        }
    }
}