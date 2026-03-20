using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using Sales.Application.Common.Helpers;
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
        // services.AddHostedService<SalesPublisherBackgroundService>();
        // services.AddHostedService<SalesConsumerBackgroundService>();
    }
}