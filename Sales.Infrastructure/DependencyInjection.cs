using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
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
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<SalesDbContext>((_, options) =>
        {
            options.UseSqlServer(connectionString, b =>
            {
                b.MigrationsAssembly(typeof(SalesDbContext).Assembly.FullName);
                b.MigrationsHistoryTable(HistoryRepository.DefaultTableName, "SalesLT");
            });
            // options.UseLazyLoadingProxies();
        });

        services.AddScoped<ISalesOrderRepository, SalesOrderRepository>();
    }
}
