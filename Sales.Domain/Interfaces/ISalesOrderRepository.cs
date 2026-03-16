using Sales.Domain.Entities;

namespace Sales.Domain.Interfaces;

public interface ISalesOrderRepository
{
    Task<IEnumerable<SalesOrderHeader>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<SalesOrderHeader?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    SalesOrderHeader Add(SalesOrderHeader entityForCreation);
    SalesOrderHeader Update(SalesOrderHeader entity);
    SalesOrderHeader Delete(SalesOrderHeader entityForDeletion);

    Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default);
}