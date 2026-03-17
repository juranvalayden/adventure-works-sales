namespace Sales.Application.Configurations;

public sealed class RabbitMqOptions
{
    public const string RabbitMq = "RabbitMq";
    public string? HostName { get; set; } = null!;
    public int? Port { get; set; }
    public string? Username { get; set; } = null!;
    public string? Password { get; set; } = null!;
    public string? VirtualHost { get; set; } = "/";
    public int? RequestedHeartbeat { get; set; }
    public bool? SslEnabled { get; set; } = false;

    public RabbitMqPublisherOptions Publisher { get; set; } = new();
    public RabbitMqConsumerOptions Consumer { get; set; } = new();
}