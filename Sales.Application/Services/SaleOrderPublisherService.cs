using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Sales.Application.Common;
using Sales.Application.Common.Rabbit;
using Sales.Application.Dtos;

namespace Sales.Application.Services;

public class SaleOrderPublisherService(ILogger<Publisher<SalesOrderHeaderDto>> logger, IConnectionFactory connectionFactory, SerializationWrapper serializationWrapper)
    : Publisher<SalesOrderHeaderDto>(logger, connectionFactory, serializationWrapper);