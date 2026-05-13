using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using Tijdperk.Service.Bot.Options;

namespace Tijdperk.Service.Bot.Services;

public class DiscordHostedService(
    ILogger<DiscordHostedService> logger,
    DiscordSocketClient client,
    IOptions<BotOptions> options
) : IHostedService, IDisposable
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await client.LoginAsync(TokenType.Bot, options.Value.Token);
        await client.StartAsync();

        if (!string.IsNullOrEmpty(options.Value.CustomStatus))
        {
            await client.SetCustomStatusAsync(options.Value.CustomStatus);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            await client.SetStatusAsync(UserStatus.Offline);
            await client.StopAsync();
            await client.LogoutAsync();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("Cancellation requested while stopping Discord client");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while stopping Discord client");
        }
    }

    public void Dispose()
    {
        client?.Dispose();
    }
}