using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sales.Domain.Interfaces;
using Sales.Infrastructure.Configurations.Persistence;
using Sales.Infrastructure.Repositories;

namespace Sales.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<SalesDbContext>(o => o.UseSqlServer(configuration["ConnectionString:AdventureWorksConnectionString"]));

        services.AddScoped<ISalesOrderRepository, SalesOrderRepository>();
    }
}
