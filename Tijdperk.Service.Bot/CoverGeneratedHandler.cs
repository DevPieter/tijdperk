using Discord;
using Discord.WebSocket;
using MediatR;
using Tijdperk.Logic.Services;

namespace Tijdperk.Service.Bot;

public class CoverGeneratedHandler(
    DiscordSocketClient client,
    ILogger<CoverGeneratedHandler> logger
) : INotificationHandler<ScenematicService.CoverGeneratedNotification>
{
    public async Task Handle(ScenematicService.CoverGeneratedNotification notification,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Received cover generated notification for JobId: {JobId}. MainCoverPath: {MainCoverPath}, DebounceCoverPath: {DebounceCoverPath}, ProcessingCoverPath: {ProcessingCoverPath}",
            notification.JobId,
            notification.MainCoverPath,
            notification.DebounceCoverPath,
            notification.ProcessingCoverPath
        );

        var channel = client.GetChannel(notification.ChannelId) as ITextChannel;
        if (channel is null)
        {
            logger.LogWarning(
                "Channel with ID {ChannelId} not found for JobId: {JobId}",
                notification.ChannelId,
                notification.JobId
            );
            return;
        }

        var builder = new ComponentBuilderV2();

        builder.WithSeparator(new SeparatorBuilder(false));
        builder.WithTextDisplay("**:rolling_eyes: No votes yet**");
        builder.WithTextDisplay("-# `0 🔥` | `0 👍` | `0 😐` | `0 👎` | `0 💩`");
        builder.WithSeparator(new SeparatorBuilder(false));
        builder.WithTextDisplay($"## **[{notification.Artist} - {notification.Title}](https://pietr.space)**");

        builder.WithMediaGallery(new MediaGalleryBuilder()
            .AddItem(new MediaGalleryItemProperties($"https://cdn.pietr.space/{notification.MainCoverPath}"))
        );

        builder.WithContainer(new ActionRowBuilder()
            .WithButton(new ButtonBuilder("Preview", "testing_embed", ButtonStyle.Success))
            .WithButton(new ButtonBuilder("Vote", "testing_vote"))
            .WithButton(new ButtonBuilder("Stats", "testing_stats", ButtonStyle.Secondary))
        );

        builder.WithContainer(new ActionRowBuilder()
            .WithButton(new ButtonBuilder(
                "Spotify",
                style: ButtonStyle.Link,
                url: "https://pietr.space",
                emote: new Emote(1512865326604353737, "spotify"))
            )
            .WithButton(new ButtonBuilder(
                "YouTube",
                style: ButtonStyle.Link,
                url: "https://pietr.space",
                emote: new Emote(1512865375178723511, "youtube"))
            )
            .WithButton(new ButtonBuilder(
                "Apple Music",
                style: ButtonStyle.Link,
                url: "https://pietr.space",
                emote: new Emote(1512866932259553400, "apple"))
            )
            .WithButton(new ButtonBuilder(
                "SoundCloud",
                style: ButtonStyle.Link,
                url: "https://pietr.space",
                emote: new Emote(1521433698275164242, "soundcloud"))
            )
        );

        var message = await channel.GetMessageAsync(notification.MessageId) as IUserMessage;
        if (message is null)
        {
            await channel.SendMessageAsync(
                components: builder.Build(),
                flags: MessageFlags.ComponentsV2 | MessageFlags.SuppressNotification
            );
            return;
        }

        await message.ModifyAsync(msg => msg.Components = builder.Build());
    }
}