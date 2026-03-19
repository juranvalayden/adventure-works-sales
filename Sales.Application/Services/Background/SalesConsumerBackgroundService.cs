using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sales.Application.Dtos;
using Sales.Application.Interfaces;

namespace Sales.Application.Services.Background;

public sealed class SalesConsumerBackgroundService(
    ILogger<SalesConsumerBackgroundService> logger,
    IServiceScopeFactory serviceScopeFactory)
    : BackgroundService
{
    private const string _queue = "sales-orders";
    private readonly ILogger<SalesConsumerBackgroundService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));

    // These are assigned in ExecuteAsync and used in StopAsync/cleanup.
    private IServiceScope? _scope;
    private IConsumer<SalesOrderHeaderDto>? _receiver;
    private readonly object _sync = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SalesConsumerBackgroundService is starting.");

        _scope = _serviceScopeFactory.CreateScope();
        _receiver = _scope.ServiceProvider.GetRequiredService<IConsumer<SalesOrderHeaderDto>>();

        try
        {
            await _receiver.StartConsumingAsync(_queue, stoppingToken).ConfigureAwait(false);

            _logger.LogInformation("SalesConsumerBackgroundService started consuming queue {Queue}.", _queue);

            await Task.Delay(Timeout.Infinite, stoppingToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Cancellation requested for SalesConsumerBackgroundService.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SalesConsumerBackgroundService encountered an error while running.");
            throw;
        }
        finally
        {
            await StopAndDisposeReceiverAsync(CancellationToken.None).ConfigureAwait(false);
            _logger.LogInformation("SalesConsumerBackgroundService has stopped.");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("SalesConsumerBackgroundService is stopping.");

        await StopAndDisposeReceiverAsync(cancellationToken).ConfigureAwait(false);

        await base.StopAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Stop the receiver if it was started and dispose the scope. Safe to call multiple times.
    /// </summary>
    private async Task StopAndDisposeReceiverAsync(CancellationToken cancellationToken)
    {
        // Ensure only one thread performs stop/dispose.
        lock (_sync)
        {
            // local copies to avoid race with ExecuteAsync assigning null later
        }

        IConsumer<SalesOrderHeaderDto>? receiver;
        IServiceScope? scope;

        lock (_sync)
        {
            receiver = _receiver;
            scope = _scope;

            _receiver = null;
            _scope = null;
        }

        if (receiver != null)
        {
            try
            {
                // If the consumer exposes a StopConsumingAsync method, call it.
                // This is defensive: many consumer implementations provide a stop method.
                var stopMethod = receiver.GetType().GetMethod("StopConsumingAsync", new[] { typeof(CancellationToken) });
                if (stopMethod != null)
                {
                    var result = stopMethod.Invoke(receiver, new object[] { cancellationToken });
                    if (result is Task stopTask)
                    {
                        await stopTask.ConfigureAwait(false);
                    }
                }
                else
                {
                    // If no explicit stop method, try to dispose the consumer if it implements IAsyncDisposable/IDisposable.
                    if (receiver is IAsyncDisposable asyncDisposable)
                    {
                        await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                    }
                    else if (receiver is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Cancellation while stopping; swallow as it's expected during shutdown.
                _logger.LogInformation("Stop operation was canceled while stopping the consumer.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Exception while stopping/disposing the consumer. Continuing shutdown.");
            }
        }

        if (scope != null)
        {
            try
            {
                // Dispose the scope (will dispose any scoped services not already disposed).
                scope.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Exception while disposing the service scope.");
            }
        }
    }
}