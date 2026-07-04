namespace Tijdperk.Service.Bot.Options.Guild.UserVerification;

public class UserVerificationOptions
{
    public ulong LogChannelId { get; set; }
    public List<UserVerificationGroup> Groups { get; set; } = null!;
}