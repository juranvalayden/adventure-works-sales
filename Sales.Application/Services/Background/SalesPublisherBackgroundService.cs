using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sales.Application.Dtos;
using Sales.Application.Factories;
using Sales.Application.Interfaces;

namespace Sales.Application.Services.Background;

public sealed class SalesPublisherBackgroundService(ILogger<SalesPublisherBackgroundService> logger, IServiceScopeFactory serviceScopeFactory) : BackgroundService
{
    private const string _queue = "sales-orders";
    private readonly ILogger<SalesPublisherBackgroundService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("SalesPublisherBackgroundService starting.");

        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(500));

        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false))
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var provider = scope.ServiceProvider;

                var salesOrderService = provider.GetRequiredService<ISalesOrderService>();
                var publisher = provider.GetRequiredService<IPublisher<SalesOrderHeaderDto>>();

                try
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var creationDto = SalesOrderFactory.Create();

                    var salesOrderHeaderDto = await salesOrderService.AddSalesOrderHeaderAsync(creationDto, cancellationToken);

                    if (salesOrderHeaderDto == null)
                    {
                        _logger.LogWarning("No SalesOrderHeader created this cycle.");
                        continue;
                    }

                    const int maxAttempts = 3;
                    var attempt = 0;
                    var published = false;

                    while (attempt < maxAttempts && !published)
                    {
                        attempt++;
                        cancellationToken.ThrowIfCancellationRequested();

                        try
                        {
                            published = await publisher.PublishAsync(salesOrderHeaderDto, _queue, cancellationToken);

                            if (!published)
                            {
                                _logger.LogWarning("Publish attempt {Attempt} failed for SalesOrderHeader {Id}.", attempt, salesOrderHeaderDto.Id);
                                await Task.Delay(TimeSpan.FromMilliseconds(200 * attempt), cancellationToken);
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            throw;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Publish attempt {Attempt} threw for SalesOrderHeader {Id}.", attempt, salesOrderHeaderDto.Id);
                            await Task.Delay(TimeSpan.FromMilliseconds(200 * attempt), cancellationToken);
                        }
                    }

                    if (!published)
                    {
                        _logger.LogError("Failed to publish SalesOrderHeader {Id} after {Attempts} attempts.", salesOrderHeaderDto.Id, maxAttempts);
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Cancellation requested; stopping publisher loop.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled exception in SalesPublisherBackgroundService iteration.");
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Cancellation requested; stopping publisher loop.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal exception in SalesPublisherBackgroundService.");
            throw; 
        }
        finally
        {
            _logger.LogInformation("SalesPublisherBackgroundService has stopped executing.");
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("SalesPublisherBackgroundService is stopping.");
        return base.StopAsync(cancellationToken);
    }
}