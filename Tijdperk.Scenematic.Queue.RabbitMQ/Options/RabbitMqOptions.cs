namespace Tijdperk.Scenematic.Queue.RabbitMQ.Options;

public class RabbitMqOptions
{
    public string Host { get; init; } = "localhost";
    public ushort Port { get; init; } = 5672;
    public string VirtualHost { get; init; } = null!;
    public string Username { get; init; } = null!;
    public string Password { get; init; } = null!;
}