using Discord;
using Discord.Interactions;
using Tijdperk.Logic.Models.Scenematic.GenerateCover;
using Tijdperk.Logic.Services;

namespace Tijdperk.Service.Bot.Interactions.Commands;

public class NewEmbedCommand(ScenematicService scenematicService) : InteractionModuleBase<SocketInteractionContext>
{
    private const string Cover =
        // "https://cdn.discordapp.com/attachments/1501197469453779094/1512794301212393603/zv19j8h.webp?ex=6a2562fb&is=6a24117b&hm=3d10fe288c739fee5316ae5bdf6128e825c4133e0d8e21e4d37060160020e138&";
        "https://cdn.pietr.space/tijdperk/cover/019f03c2-ccb5-7f37-9815-f2ebb8986aa9/cover-ec57fa34-1fed-4f50-989d-6e2c2cc954f3.webp";
    // "https://cdn.discordapp.com/attachments/1501197469453779094/1512794282161999882/nj8tm2t.webp?ex=6a2562f7&is=6a241177&hm=66d3890814a4ff8deded4c3369ad94bd049e975972ad524905fdc96cc0b35265&";

    private const string SpotifyUrl = "https://open.spotify.com/track/74m5d3fJygq03OXGXLdyXO";
    private const string YoutubeUrl = "https://pietr.space";
    private const string AppleMusicUrl = "https://pietr.space";
    private const string SoundcloudUrl = "https://pietr.space";
    private const string Author = "Self Aware";
    private const string Title = "Jiyagi";

    private readonly List<string> _coverGeneratingMessages =
    [
        "Generating a beautiful cover",
        "Creating some amazing artwork",
        "Rendering a stunning cover",
        "A cover is being generated",
        "Sit back and relax while we generate a cover"
    ];

    private readonly List<string> _waitingForVotesMessages =
    [
        "Waiting for more votes",
        "Procrastinating for more votes",
        "Waiting for the people to vote",
        "The moment of truth is coming, waiting for votes"
    ];

    [SlashCommand("generate", "Test generate functionality")]
    public async Task Generate(
        [Summary("title", "The title of the cover to generate")]
        string title,
        [Summary("artist", "The artist of the cover to generate")]
        string artist,
        [Summary("cover", "The cover image URL to use for generation")]
        string cover
    )
    {
        await Context.Interaction.DeferAsync(ephemeral: true);
        var builder = new ComponentBuilderV2();

        builder.WithSeparator(new SeparatorBuilder(false));
        builder.WithTextDisplay("**:rolling_eyes: No votes yet**");
        builder.WithTextDisplay("-# `0 🔥` | `0 👍` | `0 😐` | `0 👎` | `0 💩`");
        builder.WithSeparator(new SeparatorBuilder(false));
        builder.WithTextDisplay($"## **[{Author} - {Title}]({SpotifyUrl})**");

        builder.WithMediaGallery(new MediaGalleryBuilder()
            .AddItem(new MediaGalleryItemProperties("https://placehold.jp/3d4070/ffffff/1200x450.png?text=Cover"))
        );
        
        builder.WithMediaGallery(new MediaGalleryBuilder()
            .AddItem(new MediaGalleryItemProperties("https://placehold.jp/3d4070/ffffff/1200x200.png?text=Votes"))
        );

        builder.WithTextDisplay($"-# *Cover is being generated...*");

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

        var channel = Context.Channel as ITextChannel;
        if (channel is null)
        {
            await Context.Interaction.FollowupAsync(
                "This command can only be used in text channels.",
                ephemeral: true
            );
            return;
        }

        await Context.Interaction.FollowupAsync(
            "Success",
            ephemeral: true
        );

        var message = await channel.SendMessageAsync(
            components: builder.Build(),
            flags: MessageFlags.ComponentsV2 | MessageFlags.SuppressNotification
        );

        await scenematicService.GenerateCoverAsync(
            Context.Channel.Id,
            message.Id,
            new GenerateCoverArgs(
                title,
                artist,
                cover,
                []
            ));
    }

    [SlashCommand("test", "Test the new song message format")]
    public async Task SendSongMessage(string? comment = null)
    {
        var builder = new ComponentBuilderV2();

        builder.WithSeparator(new SeparatorBuilder(false));
        builder.WithTextDisplay("**:rolling_eyes: No votes yet**");
        builder.WithTextDisplay("-# `0 🔥` | `0 👍` | `0 😐` | `0 👎` | `0 💩`");
        builder.WithSeparator(new SeparatorBuilder(false));
        builder.WithTextDisplay($"## **[{Author} - {Title}]({SpotifyUrl})**");

        // builder.WithMediaGallery(new MediaGalleryBuilder()
        //     .AddItem(new MediaGalleryItemProperties(Cover))
        // );

        
        builder.WithMediaGallery(new MediaGalleryBuilder()
            // .AddItem(new MediaGalleryItemProperties("https://placehold.jp/3d4070/ffffff/1200x450.png?text=Cover"))
            .AddItem(new MediaGalleryItemProperties("https://cdn.pietr.space/tijdperk/cover/cover.webp"))
        );
        
        builder.WithMediaGallery(new MediaGalleryBuilder()
            // .AddItem(new MediaGalleryItemProperties("https://placehold.jp/3d4070/ffffff/1200x200.png?text=Votes"))
            .AddItem(new MediaGalleryItemProperties("https://cdn.pietr.space/tijdperk/cover/votes.webp"))
        );
        
        // builder.WithTextDisplay($"-# *Waiting for more votes...*");

        if (!string.IsNullOrWhiteSpace(comment))
            builder.WithTextDisplay($"```{comment}```");

        builder.WithContainer(new ActionRowBuilder()
            .WithButton(new ButtonBuilder("Preview", "testing_embed", ButtonStyle.Success))
            .WithButton(new ButtonBuilder("Vote", "testing_vote"))
            .WithButton(new ButtonBuilder("Stats", "testing_stats", ButtonStyle.Secondary))
        );

        builder.WithContainer(new ActionRowBuilder()
            .WithButton(new ButtonBuilder(
                "Spotify",
                style: ButtonStyle.Link,
                url: SpotifyUrl,
                emote: new Emote(1512865326604353737, "spotify"))
            )
            .WithButton(new ButtonBuilder(
                "YouTube",
                style: ButtonStyle.Link,
                url: YoutubeUrl,
                emote: new Emote(1512865375178723511, "youtube"))
            )
            .WithButton(new ButtonBuilder(
                "Apple Music",
                style: ButtonStyle.Link,
                url: AppleMusicUrl,
                emote: new Emote(1512866932259553400, "apple"))
            )
            .WithButton(new ButtonBuilder(
                "SoundCloud",
                style: ButtonStyle.Link,
                url: SoundcloudUrl,
                emote: new Emote(1521433698275164242, "soundcloud"))
            )
        );

        await Context.Interaction.RespondAsync(
            components: builder.Build(),
            flags: MessageFlags.ComponentsV2 | MessageFlags.SuppressNotification
        );
    }

    [ComponentInteraction("testing_vote")]
    public async Task Vote()
    {
        var modal = new ModalBuilder()
            .WithTitle($"{Author} - {Title}")
            .WithCustomId("testing_vote_modal");

        modal.AddRadioGroup("How do you feel about this song?", new RadioGroupBuilder("song_vote",
            [
                new RadioGroupOptionProperties("love", "Love", "This is the best song ever!"),
                new RadioGroupOptionProperties("like", "Like", "I really enjoy this song!"),
                new RadioGroupOptionProperties("ok", "Ok", "This song is decent, but not my favorite.", true),
                new RadioGroupOptionProperties("nope", "Nope", "I don't like this song at all."),
                new RadioGroupOptionProperties("trash", "Trash", "This song is terrible and I regret listening to it.")
            ],
            false
        ));

        modal.AddTextInput(
            "Comment (optional)",
            "comment",
            TextInputStyle.Paragraph,
            placeholder: "Share your thoughts about this song!",
            value: "I like this song, but it's ok.",
            required: false,
            maxLength: 200
        );

        await Context.Interaction.RespondWithModalAsync(modal.Build());
    }
}