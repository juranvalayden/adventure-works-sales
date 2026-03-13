using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sales.Application.Interfaces;

namespace Sales.Application.Services;

public class SalesBackgroundService : BackgroundService
{
    private readonly ILogger<SalesBackgroundService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public SalesBackgroundService(ILogger<SalesBackgroundService> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {   
        const string serviceName = nameof(SalesBackgroundService);

        _logger.LogInformation("SalesBackgroundService is starting...");
        using var scope = _serviceScopeFactory.CreateScope();
        var provider = scope.ServiceProvider;

        while (!cancellationToken.IsCancellationRequested)
        {
            var salesOrderService = provider.GetRequiredService<ISalesOrderService>();

            var saleOrderHeaderDto = await salesOrderService.GetSalesOrderHeaderAsync(cancellationToken);

            if (saleOrderHeaderDto == null)
            {
                _logger.LogWarning($"No SaleOrderHeader found.");
                continue;
            }

            var salesProducerService = provider.GetRequiredService<ISalesProducerService>();

            var hasPublished = await salesProducerService.PublishAsync(saleOrderHeaderDto, cancellationToken);

            if (!hasPublished)
            {
                _logger.LogWarning("SalesOrderHeader with {Id} has not been successfully published, will attempt to retry...", saleOrderHeaderDto.Id);
                var hasAdded = await salesOrderService.AddAsync(saleOrderHeaderDto, cancellationToken);

                if (!hasAdded)
                {
                    _logger.LogWarning("SalesOrderHeader with {Id} has not been successfully saved to the outbox table.", saleOrderHeaderDto.Id);
                }
            }

            await Task.Delay(500, cancellationToken);
        }

        _logger.LogInformation("SalesBackgroundService is stopping.");
    }
}
