using Tijdperk.Service.Bot.Options.Guild.UserVerification;

namespace Tijdperk.Service.Bot.Options.Guild;

public class GuildOptions
{
    public ulong Id { get; set; }
    public ulong MusicChannelId { get; set; }

    public UserVerificationOptions UserVerification { get; set; } = null!;
}