using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Sales.Application.Common.Helpers;
using Sales.Application.Configurations;
using Sales.Application.Interfaces;
using Sales.Application.Services;
using Sales.Application.Services.Background;

namespace Sales.Application;

public static class DependencyInjection
{
    public static void AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<JsonSerializerOptionsWrapper>();

        // Background services
        services.AddHostedService<SalesProducerBackgroundService>();
        services.AddHostedService<SalesConsumerBackgroundService>();

        // Application services
        services.AddScoped<ISalesOrderService, SalesOrderService>();

        // Publisher/Consumer services should be singletons if they manage RabbitMQ connections
        services.AddSingleton<ISalesPublisherService, SalesPublisherService>();
        services.AddSingleton<ISalesConsumerService, SalesConsumerService>();

        services.AddSingleton(sp =>
        {

            return new ConnectionFactory
            {
                HostName = options.Host,
                Port = options.Port,
                UserName = options.Username,
                Password = options.Password,
                VirtualHost = options.VirtualHost,
                RequestedHeartbeat = TimeSpan.FromSeconds(options.RequestedHeartbeat),
                Ssl = new SslOption { Enabled = options.SslEnabled }
            };
        });
    }
}