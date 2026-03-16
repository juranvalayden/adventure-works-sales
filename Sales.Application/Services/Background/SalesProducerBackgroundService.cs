using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sales.Application.Dtos;
using Sales.Application.Interfaces;

namespace Sales.Application.Services.Background;

public class SalesProducerBackgroundService : BackgroundService
{
    private readonly ILogger<SalesProducerBackgroundService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public SalesProducerBackgroundService(
        ILogger<SalesProducerBackgroundService> logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("SalesProducerBackgroundService is starting...");

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var provider = scope.ServiceProvider;

                var salesOrderService = provider.GetRequiredService<ISalesOrderService>();
                var salesProducerService = provider.GetRequiredService<ISalesPublisherService>();

                var salesOrderHeaderForCreationDto = CreateHeaderForCreationDto();

                var saleOrderHeaderDto = await salesOrderService.AddSalesOrderHeaderAsync(
                    salesOrderHeaderForCreationDto, cancellationToken);

                if (saleOrderHeaderDto == null)
                {
                    _logger.LogWarning("No SaleOrderHeader created this cycle.");
                }
                else
                {
                    var hasPublished = await salesProducerService.PublishAsync(saleOrderHeaderDto, cancellationToken);

                    if (hasPublished)
                    {
                        _logger.LogInformation("Published SalesOrderHeader {Id}", saleOrderHeaderDto.Id);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to publish SalesOrderHeader {Id}, will retry...", saleOrderHeaderDto.Id);
                    }
                }
            }
            catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Error occurred in SalesProducerBackgroundService loop.");
            }

            // Delay between iterations
            await Task.Delay(500, cancellationToken);
        }

        _logger.LogInformation("SalesProducerBackgroundService is stopping.");
    }

    private SalesOrderHeaderForCreationDto CreateHeaderForCreationDto()
    {
        const int subTotalMin = 500;
        const int subTotalMax = 10000;
        const int freightMin = 20;
        const int freightMax = 200;

        var subTotal = Random.Shared.NextDouble() * (subTotalMax - subTotalMin) + subTotalMin;
        var freight = Random.Shared.NextDouble() * (freightMax - freightMin) + freightMin;
        var taxAmount = (subTotal + freight) * 0.15;

        return new SalesOrderHeaderForCreationDto
        {
            RevisionNumber = 0,
            Status = 5,
            CustomerId = 29847,
            ShipMethod = "CARGO TRANSPORT 5",
            SubTotal = (decimal)Math.Round(subTotal, 2),
            TaxAmount = (decimal)Math.Round(taxAmount, 2),
            Freight = (decimal)Math.Round(freight, 2),
            ShipDate = null,
            PurchaseOrderNumber = null,
            AccountNumber = null,
            ShipToAddressId = null,
            BillToAddressId = null,
            CreditCardApprovalCode = null,
            Comment = null
        };
    }
}