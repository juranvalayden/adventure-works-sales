using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sales.Application.Dtos;
using Sales.Application.Interfaces;

namespace Sales.Application.Services.Background;

public class SalesConsumerBackgroundService(ILogger<SalesConsumerBackgroundService> logger, IServiceScopeFactory serviceScopeFactory)
    : BackgroundService
{
    private const string _queue = "sales-orders";
    private readonly ILogger<SalesConsumerBackgroundService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));

    private IConsumer<SalesOrderHeaderDto>? _consumer;
    private readonly Lock _sync = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SalesConsumerBackgroundService is starting.");

        using var scope = _serviceScopeFactory.CreateScope();
        var provider = scope.ServiceProvider;
        _consumer = provider.GetRequiredService<IConsumer<SalesOrderHeaderDto>>();

        try
        {
            var hasStarted = await _consumer.StartConsumingAsync(_queue, stoppingToken).ConfigureAwait(false);

            if (!hasStarted) throw new InvalidOperationException("Consumer could not start correctly.");

            _logger.LogInformation("SalesConsumerBackgroundService started consuming queue {Queue}.", _queue);
            await Task.Delay(Timeout.Infinite, stoppingToken).ConfigureAwait(false);

            throw new InvalidOperationException("Consumer could not start correctly.");
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
            await StopAsync(CancellationToken.None).ConfigureAwait(false);
            _logger.LogInformation("SalesConsumerBackgroundService has stopped.");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("SalesConsumerBackgroundService is stopping.");

        lock (_sync)
        {
            // local copies to avoid race with ExecuteAsync assigning null later
        }

        lock (_sync)
        {
            _consumer = null;
        }

        if (_consumer != null)
        {
            try
            {
                await _consumer.StopConsumingAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Stop operation was canceled while stopping the consumer.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Exception while stopping/disposing the consumer. Continuing shutdown.");
            }
        }

        await base.StopAsync(cancellationToken).ConfigureAwait(false);
    }
}