namespace Tijdperk.Service.Bot.Options;

public class BotOptions
{
    public string Token { get; init; } = null!;
    public string? CustomStatus { get; init; }
}