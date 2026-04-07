using System;
using System.Threading.Tasks;
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
}