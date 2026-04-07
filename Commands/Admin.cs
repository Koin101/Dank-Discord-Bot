using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord_Bot.Scripts;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Net.Models;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

namespace Discord_Bot.Commands;

public class Admin : ApplicationCommandModule
{

    [SlashCommand("timeout", "Timeout a user for a specified duration"), SlashRequireOwner]
    public async Task TimeOutUser(InteractionContext ctx,
        [Option("user", "User to timeout")] DiscordUser user,
        [Option("duration", "Duration of the timeout in minutes")] string duration,
        [Option("reason", "Reason for the timeout")]
        string reason = "Too lazy to type a reason, probably misbehaved")
    {

        DiscordMember member = ctx.Guild.GetMemberAsync(user.Id).Result;
        DateTimeOffset offset = new DateTimeOffset(DateTime.Now).AddMinutes(Convert.ToInt32(duration));
        await member.TimeoutAsync(offset, reason);
        await ctx.CreateResponseAsync("User "
                                      + user.Username + " has been timed out for "
                                      + duration + " minutes. Reason: " + reason,  true);

    }


    [SlashCommand("disconnectVC", "Disconnect a user from voice channel"), SlashRequireOwner]
    public async Task DisconnectFromVoiceChannel(InteractionContext ctx,
        [Option("user", "User to disconnect from voice channel")] DiscordUser user)
    {

        DiscordMember member = ctx.Guild.GetMemberAsync(user.Id).Result;
        var vc = member.VoiceState?.Channel;
        if (vc is null)
        {
            await ctx.CreateResponseAsync($"User {user.Username} is not in a voice channel.",
                true);
            return;
        }
        await member.ModifyAsync(new Action<MemberEditModel>(m => m.VoiceChannel = null));
        await ctx.CreateResponseAsync("User "
                                      + user.Username + " has been disconnected from voice channel "
                                      + vc.Name + ".",  true);

    }

    [SlashCommand("moveVC", "Move a user to another voice channel"), SlashRequireOwner]
    public async Task MoveUserToVoiceChannel(InteractionContext ctx,
        [Option("user", "User to move to another voice channel")] DiscordUser user,
        [Option("channel", "Voice channel to move the user to")] DiscordChannel channel)
    {
        DiscordMember member = await ctx.Guild.GetMemberAsync(user.Id);
        if (member.VoiceState?.Channel is null)
        {
            await ctx.CreateResponseAsync($"User {user.Username} is not in a voice channel.", true);
            return;
        }
        await member.ModifyAsync(m => m.VoiceChannel = channel);
        await ctx.CreateResponseAsync($"Moved {user.Username} to {channel.Name}.", true);
    }

    [SlashCommand("kick", "Kick a user from the server"), SlashRequireUserPermissions(Permissions.KickMembers)]
    public async Task KickUser(InteractionContext ctx,
        [Option("user", "User to kick")] DiscordUser user,
        [Option("reason", "Reason for the kick")] string reason = "No reason provided")
    {
        DiscordMember member = await ctx.Guild.GetMemberAsync(user.Id);
        await member.RemoveAsync(reason);
        await ctx.CreateResponseAsync($"User {user.Username} has been kicked. Reason: {reason}", true);
    }

    [SlashCommand("ban", "Ban a user from the server"), SlashRequireUserPermissions(Permissions.BanMembers)]
    public async Task BanUser(InteractionContext ctx,
        [Option("user", "User to ban")] DiscordUser user,
        [Option("reason", "Reason for the ban")] string reason = "No reason provided",
        [Option("delete_days", "Days of messages to delete (0-7)")] long deleteDays = 0)
    {
        await ctx.Guild.BanMemberAsync(user.Id, (int)deleteDays, reason);
        await ctx.CreateResponseAsync($"User {user.Username} has been banned. Reason: {reason}", true);
    }

    [SlashCommand("unban", "Unban a user from the server"), SlashRequireUserPermissions(Permissions.BanMembers)]
    public async Task UnbanUser(InteractionContext ctx,
        [Option("userid", "ID of the user to unban")] string userId)
    {
        if (!ulong.TryParse(userId, out ulong id))
        {
            await ctx.CreateResponseAsync("Invalid user ID.", true);
            return;
        }
        await ctx.Guild.UnbanMemberAsync(id);
        await ctx.CreateResponseAsync($"User with ID {userId} has been unbanned.", true);
    }

    [SlashCommand("purge", "Delete a number of messages from this channel"), SlashRequireUserPermissions(Permissions.ManageMessages)]
    public async Task PurgeMessages(InteractionContext ctx,
        [Option("amount", "Number of messages to delete (1-100)")] long amount)
    {
        if (amount < 1 || amount > 100)
        {
            await ctx.CreateResponseAsync("Amount must be between 1 and 100.", true);
            return;
        }
        var messages = await ctx.Channel.GetMessagesAsync((int)amount);
        await ctx.Channel.DeleteMessagesAsync(messages);
        await ctx.CreateResponseAsync($"Deleted {messages.Count} messages.", true);
    }

    [SlashCommand("warn", "Issue a warning to a user"), SlashRequireUserPermissions(Permissions.KickMembers)]
    public async Task WarnUser(InteractionContext ctx,
        [Option("user", "User to warn")] DiscordUser user,
        [Option("reason", "Reason for the warning")] string reason)
    {
        WarningsStore.AddWarning(user.Id, reason);
        var count = WarningsStore.GetWarnings(user.Id).Count;
        await ctx.CreateResponseAsync($"**{user.Username}** has been warned. Reason: {reason}\nTotal warnings: {count}", true);
    }

    [SlashCommand("warnings", "View warnings for a user"), SlashRequireUserPermissions(Permissions.KickMembers)]
    public async Task ViewWarnings(InteractionContext ctx,
        [Option("user", "User to check warnings for")] DiscordUser user)
    {
        var warnings = WarningsStore.GetWarnings(user.Id);
        if (warnings.Count == 0)
        {
            await ctx.CreateResponseAsync($"{user.Username} has no warnings.", true);
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine($"**Warnings for {user.Username}** ({warnings.Count} total)");
        for (int i = 0; i < warnings.Count; i++)
            sb.AppendLine($"`{i + 1}.` {warnings[i].Reason} — <t:{new DateTimeOffset(warnings[i].IssuedAt).ToUnixTimeSeconds()}:d>");

        await ctx.CreateResponseAsync(sb.ToString(), true);
    }

    [SlashCommand("role", "Add or remove a role from a user"), SlashRequireUserPermissions(Permissions.ManageRoles)]
    public async Task ManageRole(InteractionContext ctx,
        [Option("action", "add or remove")] string action,
        [Option("user", "Target user")] DiscordUser user,
        [Option("role", "Role to assign or remove")] DiscordRole role)
    {
        DiscordMember member = await ctx.Guild.GetMemberAsync(user.Id);
        switch (action.ToLower())
        {
            case "add":
                await member.GrantRoleAsync(role);
                await ctx.CreateResponseAsync($"Added role **{role.Name}** to {user.Username}.", true);
                break;
            case "remove":
                await member.RevokeRoleAsync(role);
                await ctx.CreateResponseAsync($"Removed role **{role.Name}** from {user.Username}.", true);
                break;
            default:
                await ctx.CreateResponseAsync("Action must be `add` or `remove`.", true);
                break;
        }
    }

    [SlashCommand("nick", "Change a user's server nickname"), SlashRequireUserPermissions(Permissions.ManageNicknames)]
    public async Task ChangeNickname(InteractionContext ctx,
        [Option("user", "User to rename")] DiscordUser user,
        [Option("nickname", "New nickname (leave empty to reset)")] string nickname = "")
    {
        DiscordMember member = await ctx.Guild.GetMemberAsync(user.Id);
        var newNick = string.IsNullOrWhiteSpace(nickname) ? null : nickname;
        await member.ModifyAsync(m => m.Nickname = newNick);
        var response = newNick is null
            ? $"Reset nickname for {user.Username}."
            : $"Changed nickname of {user.Username} to **{newNick}**.";
        await ctx.CreateResponseAsync(response, true);
    }
}