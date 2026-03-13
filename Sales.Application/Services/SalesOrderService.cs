using Microsoft.Extensions.Logging;
using Sales.Application.Dtos;
using Sales.Application.Interfaces;
using Sales.Application.Mappers;
using Sales.Domain.Interfaces;

namespace Sales.Application.Services;

public class SalesOrderService : ISalesOrderService
{
    private readonly ILogger<SalesOrderService> _logger;
    private readonly ISalesOrderRepository _salesOrderRepository;

    public SalesOrderService(ILogger<SalesOrderService> logger, ISalesOrderRepository salesOrderRepository)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _salesOrderRepository = salesOrderRepository ?? throw new ArgumentNullException(nameof(salesOrderRepository));
    }

    public async Task<SalesOrderHeaderDto?> GetSalesOrderHeaderAsync(CancellationToken cancellationToken = default)
    {
        var entity = await _salesOrderRepository.GetSalesOrderHeaderAsync(cancellationToken);

        return entity == null 
            ? null 
            : Mapper.MapToDto(entity);
    }

    public async Task<bool> AddAsync(SalesOrderHeaderDto saleOrderHeaderDto, CancellationToken cancellationToken = default)
    {
        var entity = Mapper.MapToEntity(saleOrderHeaderDto);
        _salesOrderRepository.Add(entity);

        var hasSaved = await _salesOrderRepository.SaveChangesAsync(cancellationToken);

        return hasSaved;
    }
}
