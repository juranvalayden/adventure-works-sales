using Sales.Domain.Interfaces;

namespace Sales.Application.Common.Behaviours;

public class UnitOfWorkBehaviour : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}