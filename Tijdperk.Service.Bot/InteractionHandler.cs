using System.Reflection;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using Tijdperk.Service.Bot.Options;
using Tijdperk.Service.Bot.Services;
using Tijdperk.Service.Bot.Services.Interfaces;
using Tijdperk.Service.Bot.Utils;

namespace Tijdperk.Service.Bot;

public class InteractionHandler(
    DiscordSocketClient client,
    InteractionService interactions,
    IServiceProvider services,
    ILogger<InteractionHandler> logger,
    IOptions<GuildOptions> options,
    IMusicShareService musicShareService
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
            if (!SongUtils.IsValidSongUrl(message.Content)) return;

            await message.Channel.TriggerTypingAsync();
            await musicShareService.ShareAsync(guildUser, message.Content);
        };
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