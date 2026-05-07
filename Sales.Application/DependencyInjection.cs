using System.Diagnostics.Metrics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using Sales.Application.Common;
using Sales.Application.Configurations;
using Sales.Application.Dtos;
using Sales.Application.Interfaces;
using Sales.Application.Services;
using Sales.Application.Services.Background;

namespace Sales.Application;

public static class DependencyInjection
{
    public static void AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<SerializationWrapper>();

        // services.Configure<CacheSettingsOptions>(configuration.GetSection(nameof(CacheSettingsOptions)));

        services.AddMemoryCache();

        services.AddSingleton<ICacheService, MemoryCacheService>();
        services.AddSingleton(new Meter("salesorders.api", "1.0.0"));

        var hostName = configuration["RabbitMQ:HostName"];
        if (string.IsNullOrWhiteSpace(hostName)) hostName = "localhost";

        var username = configuration["RabbitMQ:Username"];
        if (string.IsNullOrWhiteSpace(username)) username = "guest";

        var password = configuration["RabbitMQ:Password"];
        if (string.IsNullOrWhiteSpace(password)) password = "guest";

        var isValidPort = int.TryParse(configuration["RabbitMQ:Port"], out var port);

        if (isValidPort)
        {
            services.AddSingleton<IConnectionFactory>(_ => new ConnectionFactory
            {
                HostName = hostName,
                UserName = username,
                Password = password,
                Port = port
            });
        }

        // Application services
        services.AddScoped<ISalesOrderService, SalesOrderService>();

        // Publisher/Consumer services should be singletons if they manage RabbitMQ connections
        services.AddSingleton<IPublisher<SalesOrderHeaderDto>, SaleOrderPublisherService>();
        services.AddSingleton<IConsumer<SalesOrderHeaderDto>, SalesOrderConsumerService>();

        // Background services
        services.AddHostedService<SalesCacheBackgroundService>();
        // services.AddHostedService<SalesPublisherBackgroundService>();
        // services.AddHostedService<SalesConsumerBackgroundService>();
    }
}