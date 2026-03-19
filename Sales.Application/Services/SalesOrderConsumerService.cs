using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Sales.Application.Common.Helpers;
using Sales.Application.Dtos;

namespace Sales.Application.Services;

public class SalesOrderConsumerService(ILogger<Consumer<SalesOrderHeaderDto>> logger, IConnectionFactory connectionFactory, SerializationWrapper serializationWrapper)
    : Consumer<SalesOrderHeaderDto>(logger, connectionFactory, serializationWrapper);