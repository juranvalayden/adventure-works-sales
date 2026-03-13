using Microsoft.Extensions.Logging;
using Sales.Application.Interfaces;
using Sales.Domain.Entities;

namespace Sales.Application.Services;

internal class SalesProducerService : ISalesProducerService
{
    private readonly ILogger<SalesProducerService> _logger;

    public SalesProducerService(ILogger<SalesProducerService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> PublishAsync(SalesOrderHeader? saleOrderHeaderDto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Publishing SalesOrderHeader: {Id}", saleOrderHeaderDto.Id);
        await Task.Delay(1000, cancellationToken);
        return true;
    }
}