using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using Tijdperk.Service.Bot.Options.Guild;

namespace Tijdperk.Service.Bot.Interactions.Commands;

public class TestRoleManagementCommand(IOptions<GuildOptions> options) : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("test-role-management", "Test the role management system")]
    public async Task TestRoleManagement(SocketGuildUser user)
    {
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

        await Context.Interaction.RespondAsync(
            components: builder.Build(),
            flags: MessageFlags.ComponentsV2
        );
    }

    [ComponentInteraction("testing_verify_*")]
    public async Task VerifyUser(string userId)
    {
        var verificationOptions = options.Value.UserVerification;

        var user = Context.Guild.GetUser(ulong.Parse(userId));
        if (user is null) return;

        var modal = new ModalBuilder()
            .WithTitle("Verify User")
            .WithCustomId($"verify_modal_{userId}");

        modal.AddRadioGroup("Select a group to assign to the user", new RadioGroupBuilder("verify_group",
            verificationOptions.Groups.Select(r => new RadioGroupOptionProperties(
                r.Id,
                r.Name,
                r.Description,
                r.IsDefault
            )).ToList(),
            true
        ));

        await Context.Interaction.RespondWithModalAsync(modal.Build());
    }

    [ComponentInteraction("testing_sync_*")]
    public async Task SyncRoles(string userId)
    {
        var user = Context.Guild.GetUser(ulong.Parse(userId));
        if (user is null) return;

        var previousRoleIds = InteractionHandler.UserRoles.TryGetValue(user.Id, out var role) ? role : [];

        var allRolesIds = InteractionHandler.SyncRoles;
        var allRoles = allRolesIds.Select(roleId => Context.Guild.GetRole(roleId)).Where(r => r != null).ToList();

        var modal = new ModalBuilder()
            .WithTitle("Sync Roles")
            .WithCustomId($"sync_modal_{userId}");

        modal.AddCheckBoxGroup("Select roles to assign to the user", new CheckboxGroupBuilder("sync_roles",
            allRoles.Select(r => new CheckboxGroupOptionProperties(
                r.Id.ToString(),
                r.Name,
                null,
                previousRoleIds.Contains(r.Id)
            )).ToList()
        ));

        await Context.Interaction.RespondWithModalAsync(modal.Build());
    }
}