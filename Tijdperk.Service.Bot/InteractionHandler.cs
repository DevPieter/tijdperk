using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using Tijdperk.Service.Bot.Options;
using Tijdperk.Service.Bot.Options.Guild;
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
    public static List<ulong> SyncRoles =
    [
        1522896982329262240, // Meow
        1522897050881097809, // Woof
        1522897097282682941 // Fuzzy
    ];

    public static List<ulong> VerifiedRoles =
    [
        1522895336807333898 // Member
    ];

    public static Dictionary<ulong, List<ulong>> UserRoles = new();

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

        // client.UserUpdated += async (oldUser, newUser) =>
        // {
        //     // Check if roles have changed
        //     
        // };

        client.GuildMemberUpdated += async (oldUserCache, newUser) =>
        {
            var oldUser = await oldUserCache.GetOrDownloadAsync();
            if (oldUser == null) return;

            var oldRoles = oldUser.Roles.Select(r => r.Id).ToHashSet();
            var newRoles = newUser.Roles.Select(r => r.Id).ToHashSet();

            var addedRoles = newRoles.Except(oldRoles).Where(r => SyncRoles.Contains(r)).ToList();
            var removedRoles = oldRoles.Except(newRoles).Where(r => SyncRoles.Contains(r)).ToList();

            if (addedRoles.Any() || removedRoles.Any())
            {
                UserRoles[newUser.Id] = newUser.Roles.Select(r => r.Id).ToList();

                logger.LogInformation(
                    "Updated roles for user {UserId}: Added: {Added}, Removed: {Removed}",
                    newUser.Id,
                    string.Join(", ", addedRoles),
                    string.Join(", ", removedRoles)
                );
            }
        };

        client.UserJoined += async user =>
        {
            if (user.Guild.Id != options.Value.Id) return;
            var verificationOptions = options.Value.UserVerification;

            var verificationChannel = user.Guild.GetTextChannel(verificationOptions.LogChannelId);
            if (verificationChannel is null) return;

            var builder = new ComponentBuilderV2();

            builder.WithTextDisplay($"## **{user.Mention} just joined the server!**");

            builder.WithContainer(new ActionRowBuilder()
                .WithButton(new ButtonBuilder("Verify", $"testing_verify_{user.Id}", ButtonStyle.Success))
                .WithButton(new ButtonBuilder("Sync Roles", $"testing_sync_{user.Id}", ButtonStyle.Primary))
                .WithButton(new ButtonBuilder("Kick", $"testing_kick_{user.Id}", ButtonStyle.Danger))
            );

            await verificationChannel.SendMessageAsync(
                components: builder.Build(),
                flags: MessageFlags.ComponentsV2
            );
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