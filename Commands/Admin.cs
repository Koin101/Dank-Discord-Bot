using System;
using System.Threading.Tasks;
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
        [Option("channelID", "ID of the voice channel to move the user to")] string channelID)
    {

        DiscordMember member = ctx.Guild.GetMemberAsync(user.Id).Result;
        var vc = member.VoiceState?.Channel;
        

    }
    
    
    
}