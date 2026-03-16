using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sales.Application.Interfaces;
using Sales.Domain.Interfaces;

namespace Sales.Application.Services.Background;

public class SalesConsumerBackgroundService : BackgroundService
{
    private readonly ILogger<SalesConsumerBackgroundService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public SalesConsumerBackgroundService(
        ILogger<SalesConsumerBackgroundService> logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("SalesConsumerBackgroundService is starting...");

        using var scope = _serviceScopeFactory.CreateScope();
        var provider = scope.ServiceProvider;
        var salesConsumerService = provider.GetRequiredService<ISalesConsumerService>();

        try
        {
            // Start consuming once, let RabbitMQ drive the callbacks
            await salesConsumerService.StartConsumingAsync(cancellationToken);

            // Keep running until cancellation
            await Task.Delay(Timeout.Infinite, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SalesConsumerBackgroundService encountered an error.");
        }
        finally
        {
            // Cleanup on shutdown
            await salesConsumerService.StopConsumingAsync(cancellationToken);
            await salesConsumerService.DisposeAsync();

            _logger.LogInformation("SalesConsumerBackgroundService is stopping.");
        }
    }
}