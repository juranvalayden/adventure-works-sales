using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sales.Application.Common.Helpers;
using Sales.Application.Interfaces;
using Sales.Application.Services;

namespace Sales.Application;

public static class DependencyInjection
{
    public static void AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<JsonSerializerOptionsWrapper>();

        services.AddHostedService<SalesBackgroundService>();
        
        services.AddScoped<ISalesOrderService, SalesOrderService>();
        services.AddScoped<ISalesProducerService, SalesProducerService>();
        services.AddScoped<ISalesConsumerService, SalesConsumerService>();

        var localhost = configuration["RabbitMQ:Host"] ?? "localhost";
        var username = configuration["RabbitMQ:Username"] ?? "guest";
        var password = configuration["RabbitMQ:Password"] ?? "guest";
    }
}