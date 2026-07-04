namespace Tijdperk.Service.Bot.Options.Guild.UserVerification;

public class UserVerificationGroup
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public bool IsDefault { get; set; } = false;

    public List<ulong> Roles { get; set; } = null!;
}