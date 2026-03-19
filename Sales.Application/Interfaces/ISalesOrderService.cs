using Sales.Application.Dtos;

namespace Sales.Application.Interfaces;

public interface ISalesOrderService
{
    Task<IEnumerable<SalesOrderHeaderDto>> GetSalesOrderHeadersAsync(CancellationToken cancellationToken = default);
    Task<SalesOrderHeaderDto?> GetSalesOrderHeaderByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<SalesOrderHeaderDto?> AddSalesOrderHeaderAsync(SalesOrderHeaderForCreationDto salesOrderHeaderForCreationDto,
        CancellationToken cancellationToken = default);

    Task<SalesOrderHeaderDto?> UpdateSalesOrderHeaderAsync(int id, SalesOrderHeaderForUpdateDto salesOrderHeaderForUpdateDto, CancellationToken cancellationToken = default);
    
    Task<bool> DeleteSalesOrderHeaderAsync(int id, CancellationToken cancellationToken = default);
}