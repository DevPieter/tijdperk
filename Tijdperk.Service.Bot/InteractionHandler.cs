using System.Reflection;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using Tijdperk.Service.Bot.Options;
using Tijdperk.Service.Bot.Services.Interfaces;
using Tijdperk.Service.Bot.Utils;

namespace Tijdperk.Service.Bot;

public class InteractionHandler(
    DiscordSocketClient client,
    InteractionService interactions,
    IServiceProvider services,
    ILogger<InteractionHandler> logger,
    IOptions<GuildOptions> options
)
{
    public async Task InitializeAsync()
    {
        await interactions.AddModulesAsync(Assembly.GetEntryAssembly(), services);

        client.InteractionCreated += HandleInteractionAsync;
        client.Ready += ClientOnReady;

        client.MessageReceived += async message =>
        {
            if (message.Channel.Id != options.Value.MusicChannelId) return;
            if (message.Author.IsBot) return;

            if (message.Author is not SocketGuildUser guildUser) return;

            var songUrls = TryExtractSongUrls(message.Content).ToList();
            if (songUrls.Count == 0) return;

            using var scope = services.CreateScope();
            var songShareService = scope.ServiceProvider.GetRequiredService<IDiscordSongShareService>();

            foreach (var (url, comment) in songUrls)
            {
                await message.Channel.TriggerTypingAsync();
                await songShareService.ShareToChannelAsync(guildUser, url, comment);
            }

            await message.DeleteAsync();
        };
    }

    // TODO - Move to util and return a more structured result
    private static IEnumerable<(string url, string comment)> TryExtractSongUrls(string messageContent)
    {
        var words = messageContent.Split([' ', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries);

        var urls = words.Where(SongUtils.IsValidSongUrl).ToHashSet();
        if (urls.Count == 0) yield break;

        var comment = string.Join(' ', words.Where(w => !urls.Contains(w))).Trim();
        foreach (var url in urls) yield return (url, comment);
    }

    private async Task HandleInteractionAsync(SocketInteraction interaction)
    {
        try
        {
            var context = new SocketInteractionContext(client, interaction);
            await interactions.ExecuteCommandAsync(context, services);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while handling an interaction.");
        }
    }

    private async Task ClientOnReady()
    {
        await interactions.RegisterCommandsToGuildAsync(options.Value.Id);
        logger.LogDebug("Registered commands to the development guild.");
    }
}