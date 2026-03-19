using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Sales.Application.Interfaces;
using System.Text;
using System.Text.Json;

namespace Sales.Application.Common.Helpers;

public class Consumer<T>(ILogger<Consumer<T>> logger, IConnectionFactory connectionFactory, SerializationWrapper serializationWrapper)
    : IConsumer<T> where T : class
{
    private readonly ILogger<Consumer<T>> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IConnectionFactory _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    private readonly SerializationWrapper _serializationWrapper = serializationWrapper ?? throw new ArgumentNullException(nameof(serializationWrapper));

    private IConnection? _connection;
    private IChannel? _channel;
    private string? _consumerTag;
    private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
    private bool _started;
    private bool _disposed;

    public async Task<bool> StartConsumingAsync(string queue, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(queue)) throw new ArgumentNullException(nameof(queue));
        ObjectDisposedException.ThrowIf(_disposed, nameof(Consumer<>));

        await _semaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            if (_started) return true;

            if (_connection is not { IsOpen: true })
            {
                _connection = await _connectionFactory.CreateConnectionAsync(cancellationToken: cancellationToken);
            }

            if (_channel is not { IsOpen: true })
            {
                _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);
            }

            await _channel.QueueDeclareAsync(
                queue: queue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken);

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (sender, eventArgs) =>
            {
                try
                {
                    var message = Encoding.UTF8.GetString(eventArgs.Body.Span);

                    try
                    {
                        var dto = JsonSerializer.Deserialize<T>(message, _serializationWrapper.Options);

                        if (dto == null)
                        {
                            _logger.LogError("Deserialized message was null for type {TypeName}. DeliveryTag: {Tag}", typeof(T).Name, eventArgs.DeliveryTag);

                            await _channel.BasicNackAsync(
                                eventArgs.DeliveryTag,
                                multiple: false,
                                requeue: false,
                                cancellationToken: cancellationToken);

                            return;
                        }

                        await ((AsyncEventingBasicConsumer)sender).Channel.BasicAckAsync(
                            eventArgs.DeliveryTag,
                            multiple: false,
                            cancellationToken);

                        await Task.Delay(500, cancellationToken);
                    }
                    catch (JsonException jsonException)
                    {
                        _logger.LogError(jsonException, "Failed to deserialize message for type {TypeName}. Message: {Message}", typeof(T).Name, message);
                        await _channel.BasicNackAsync(
                            eventArgs.DeliveryTag,
                            multiple: false,
                            requeue: false,
                            cancellationToken: cancellationToken);

                        return;
                    }

                    _logger.LogInformation("Started consuming from queue '{Queue}'", queue);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled exception while processing message DeliveryTag {DeliveryTag}",
                        eventArgs.DeliveryTag);
                }
            };

            _consumerTag = await _channel.BasicConsumeAsync(queue: queue, autoAck: false, consumer: consumer, cancellationToken: cancellationToken);
            _started = true;

            _logger.LogInformation("Started consuming queue {Queue} with consumer tag {Tag}", queue, _consumerTag);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start consumer for queue {Queue}", queue);
            return false;
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    public async Task StopConsumingAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed) return;

        await _semaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            if (!_started) return;

            try
            {
                if (_channel != null && !string.IsNullOrEmpty(_consumerTag))
                {
                    await _channel.BasicCancelAsync(_consumerTag, cancellationToken: cancellationToken);
                    _logger.LogInformation("Requested BasicCancel for consumer tag {Tag}", _consumerTag);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error while cancelling consumer");
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                /* ignore */
            }

            _started = false;
            _consumerTag = null;
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

        try
        {
            await StopConsumingAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Exception while stopping consumer during dispose");
        }

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