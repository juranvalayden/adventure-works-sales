using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Sales.Application.Services.Background;

public class SalesCacheBackgroundService : BackgroundService
{
    private readonly ILogger<SalesCacheBackgroundService> _logger;

    public SalesCacheBackgroundService(ILogger<SalesCacheBackgroundService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting {ServiceName}...", nameof(SalesCacheBackgroundService));
        await Task.Delay(500, cancellationToken);
    }
}
