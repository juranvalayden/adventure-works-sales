using Microsoft.Extensions.Logging;
using Sales.Application.Dtos;
using Sales.Application.Interfaces;

namespace Sales.Application.Services;

public class SalesProducerService : ISalesProducerService
{
    private readonly ILogger<SalesProducerService> _logger;

    public SalesProducerService(ILogger<SalesProducerService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> PublishAsync(SalesOrderHeaderDto saleOrderHeaderDto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Publishing SalesOrderHeader: {Id}", saleOrderHeaderDto.Id);
        await Task.Delay(1000, cancellationToken);
        return true;
    }
}