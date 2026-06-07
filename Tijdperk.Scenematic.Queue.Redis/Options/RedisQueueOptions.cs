namespace Tijdperk.Scenematic.Queue.Redis.Options;

public class RedisQueueOptions
{
    public string Host { get; init; } = "localhost";
    public int Port { get; init; } = 6379;
    public string? Password { get; init; }
}