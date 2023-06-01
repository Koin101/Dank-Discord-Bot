﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;

namespace Discord_Bot.Commands
{
    public class Music : BaseCommandModule
    {
        Queue<LavalinkTrack> musicQueue = new Queue<LavalinkTrack>();

        [Command]
        public async Task Join(CommandContext ctx)
        {
            var lava = ctx.Client.GetLavalink();
            var channel = ctx.Member.VoiceState.Channel;

            
            if (!lava.ConnectedNodes.Any())
            {
                await ctx.RespondAsync("A connection could not be established, please contact auke");
                return;
            }

            if (channel == null) { await ctx.RespondAsync("You are not connected to a voice channel retard!"); }

            var node = lava.ConnectedNodes.Values.First();

            await node.ConnectAsync(channel);
            await ctx.RespondAsync($"Joined {channel.Name}");
        }

        [Command]
        public async Task Leave(CommandContext ctx)
        {
            var lava = ctx.Client.GetLavalink();
            if (!lava.ConnectedNodes.Any())
            {
                await ctx.RespondAsync("A connection could not be established, please contact auke");
                return;
            }
            var channel = ctx.Member.VoiceState.Channel;
            var node = lava.ConnectedNodes.Values.First();


            var conn = node.GetGuildConnection(channel.Guild);

            if (conn == null)
            {
                await ctx.RespondAsync("The Bot is not connected.");
                return;
            }

            await conn.DisconnectAsync();
            await ctx.RespondAsync($"Left {channel.Name}!");
        }

        [Command]
        public async Task Play(CommandContext ctx, [RemainingText] string search)
        {
            var channel = ctx.Member.VoiceState.Channel;
            if (ctx.Member.VoiceState == null || channel == null)
            {
                await ctx.RespondAsync("You are not in a voice channel retard!");
                return;
            }

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                await node.ConnectAsync(channel);
                await ctx.RespondAsync($"Connected to {channel.Name}");
                conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);
            }

            var loadResult = await node.Rest.GetTracksAsync(search);

            if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
            {
                await ctx.RespondAsync($"Track search failed for {search}. Try better or blame Auke.");
                return;
            }

            var track = loadResult.Tracks.First();
            
            await conn.PlayAsync(track);
            
            await ctx.RespondAsync($"Now playing {track.Title}!");
        }

        

        [Command]
        public async Task Pause(CommandContext ctx)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync("You are not in a voice channel.");
                return;
            }

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                await ctx.RespondAsync("Lavalink is not connected.");
                return;
            }

            if (conn.CurrentState.CurrentTrack == null)
            {
                await ctx.RespondAsync("There are no tracks loaded.");
                return;
            }

            await conn.PauseAsync();

        }
    }
}
