using Microsoft.AspNetCore.JsonPatch;
using Sales.Application.Dtos;
using Sales.Domain.Pagination;

namespace Sales.Application.Interfaces;

public interface ISalesOrderService
{
    Task<IEnumerable<SalesOrderHeaderDto>> GetSalesOrderHeadersAsync(CancellationToken cancellationToken = default);
    
    Task<(IEnumerable<SalesOrderHeaderDto>, PaginationMetadata)> GetSalesOrderHeadersAsync(
        string? salesOrderNumber, int pageNumber, int pageSize,
        CancellationToken cancellationToken = default);

    Task<SalesOrderHeaderDto?> GetSalesOrderHeaderByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<SalesOrderHeaderDto?> AddSalesOrderHeaderAsync(SalesOrderHeaderForCreationDto salesOrderHeaderForCreationDto,
        CancellationToken cancellationToken = default);

    Task<SalesOrderHeaderDto?> UpdateSalesOrderHeaderAsync(int id, SalesOrderHeaderForUpdateDto salesOrderHeaderForUpdateDto, CancellationToken cancellationToken = default);

    Task<SalesOrderHeaderDto?> PatchDocumentAsync(int id, JsonPatchDocument<SalesOrderHeaderForUpdateDto> patchDocument,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteSalesOrderHeaderAsync(int id, CancellationToken cancellationToken = default);
}