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
            await message.DeleteAsync();

            if (message.Author is not SocketGuildUser guildUser) return;

            var (url, comment) = TryExtractSongUrl(message.Content);
            if (string.IsNullOrEmpty(url)) return;

            await message.Channel.TriggerTypingAsync();

            using var scope = services.CreateScope();
            var songShareService = scope.ServiceProvider.GetRequiredService<IDiscordSongShareService>();
            await songShareService.ShareToChannelAsync(guildUser, url, comment);
        };
    }

    // TODO - Move to util and return a more structured result
    private static (string url, string comment) TryExtractSongUrl(string messageContent)
    {
        var words = messageContent.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var url = words.FirstOrDefault(SongUtils.IsValidSongUrl);
        if (url is null) return (string.Empty, string.Empty);

        var comment = string.Join(' ', words.Where(w => w != url)).Trim();
        return (url, comment);
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