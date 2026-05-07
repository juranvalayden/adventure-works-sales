using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Sales.Application.Configurations;
using Sales.Application.Interfaces;
using Sales.Infrastructure.Configurations.Persistence;
using Sales.Infrastructure.Repositories;
using System.Diagnostics.Metrics;

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

            // Uncomment if you want lazy loading
            // options.UseLazyLoadingProxies();
        });

        // Register repositories
        services.AddScoped<SalesOrderRepository>();
        
        // Decorated repository
        services.AddScoped<ISalesOrderRepository>(sp =>
        {
            var innerRepository = sp.GetRequiredService<SalesOrderRepository>();
            var cacheService = sp.GetRequiredService<ICacheService>();
            var options = sp.GetRequiredService<IOptions<SalesCachingOptions>>();
            var meter = sp.GetRequiredService<Meter>();

            return new SalesOrderCacheRepository(cacheService, innerRepository, meter, options);
        });
    }
}