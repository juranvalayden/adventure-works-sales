using Sales.Domain.Entities;
using Sales.Domain.Pagination;

namespace Sales.Domain.Interfaces;

public interface ISalesOrderRepository
{
    Task<IEnumerable<SalesOrderHeader>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<(IEnumerable<SalesOrderHeader>, PaginationMetadata)> GetSalesOrderHeadersAsync(
        string? salesOrderNumber, int pageNumber, int pageSize,
        CancellationToken cancellationToken = default);

    Task<SalesOrderHeader?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    SalesOrderHeader Add(SalesOrderHeader entityForCreation);
    SalesOrderHeader Update(SalesOrderHeader entity);
    SalesOrderHeader Delete(SalesOrderHeader entityForDeletion);

    Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default);
}