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
    private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
    private bool _disposed;

    public async Task<bool> PublishAsync(T message, string queue, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(queue)) throw new ArgumentNullException(nameof(queue));
        ObjectDisposedException.ThrowIf(_disposed, nameof(Consumer<>));

        await _semaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            if (_connection is not { IsOpen: true })
            {
                _connection = await _connectionFactory.CreateConnectionAsync(cancellationToken: cancellationToken);
            }

            if (_channel is not { IsOpen: true })
            {
                _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);
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
            _semaphoreSlim.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;

        var cancellationToken = CancellationToken.None;

        await CloseChannelAsync(cancellationToken);
        await CloseConnectionAsync(cancellationToken);

        _semaphoreSlim.Dispose();
        _disposed = _channel is null && _connection is null;
    }

    private async ValueTask CloseConnectionAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (_connection != null)
            {
                try
                {
                    await _connection.CloseAsync(cancellationToken);
                }
                catch
                {
                    /* swallow */
                }

                _connection.Dispose();
                _connection = null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Exception while disposing connection");
        }
    }

    private async ValueTask CloseChannelAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (_channel != null)
            {
                try
                {
                    await _channel.CloseAsync(cancellationToken);
                }
                catch
                {
                    /* swallow */
                }

                _channel.Dispose();
                _channel = null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Exception while disposing channel");
        }
    }
}