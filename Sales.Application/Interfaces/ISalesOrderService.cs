using Sales.Application.Dtos;

namespace Sales.Application.Interfaces;

public interface ISalesOrderService
{
    // get all sales order headers
    Task<IEnumerable<SalesOrderHeaderDto>> GetSalesOrderHeadersAsync(CancellationToken cancellationToken = default);

    // get sales order header by id
    Task<SalesOrderHeaderDto?> GetSalesOrderHeaderByIdAsync(int id, CancellationToken cancellationToken = default);

    // add
    Task<SalesOrderHeaderDto?> AddSalesOrderHeaderAsync(SalesOrderHeaderForCreationDto salesOrderHeaderForCreationDto,
        CancellationToken cancellationToken = default);

    // update
    Task<SalesOrderHeaderDto?> UpdateSalesOrderHeaderAsync(int id, SalesOrderHeaderForUpdateDto salesOrderHeaderForUpdateDto, CancellationToken cancellationToken = default);
    
    // delete
    Task<bool> DeleteSalesOrderHeaderAsync(int id, CancellationToken cancellationToken = default);
}