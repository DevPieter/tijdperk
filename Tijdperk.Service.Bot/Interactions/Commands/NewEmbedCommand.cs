using Discord;
using Discord.Interactions;

namespace Tijdperk.Service.Bot.Interactions.Commands;

public class NewEmbedCommand : InteractionModuleBase<SocketInteractionContext>
{
    public const string Cover =
        "https://cdn.discordapp.com/attachments/1501197469453779094/1512794301212393603/zv19j8h.webp?ex=6a2562fb&is=6a24117b&hm=3d10fe288c739fee5316ae5bdf6128e825c4133e0d8e21e4d37060160020e138&";
    // "https://cdn.discordapp.com/attachments/1501197469453779094/1512794282161999882/nj8tm2t.webp?ex=6a2562f7&is=6a241177&hm=66d3890814a4ff8deded4c3369ad94bd049e975972ad524905fdc96cc0b35265&";

    public const string SpotifyUrl = "https://open.spotify.com/track/74m5d3fJygq03OXGXLdyXO";
    public const string YoutubeUrl = "https://pietr.space";
    public const string AppleMusicUrl = "https://pietr.space";
    public const string Author = "Crypton";
    public const string Title = "GOOD VIBES";

    [SlashCommand("test", "Test the new song message format")]
    public async Task SendSongMessage(string? comment = null)
    {
        var builder = new ComponentBuilderV2();

        builder.WithSeparator(new SeparatorBuilder(false));
        builder.WithTextDisplay("**:thumbsup: Liked**");
        builder.WithTextDisplay("-# `10 🔥` | `3 👍` | `1 😐` | `0 👎` | `1 💩`");
        builder.WithSeparator(new SeparatorBuilder(false));
        builder.WithTextDisplay($"## **[{Author} - {Title}]({SpotifyUrl})**");

        builder.WithMediaGallery(new MediaGalleryBuilder()
            .AddItem(new MediaGalleryItemProperties(Cover))
        );

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