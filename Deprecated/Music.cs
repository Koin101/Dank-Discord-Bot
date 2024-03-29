﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using KGySoft.CoreLibraries;
using SixLabors.ImageSharp.Processing.Processors.Transforms;

namespace Discord_Bot.Commands
{
    
    public class Music : BaseCommandModule
    {
        static Queue<LavalinkTrack> musicQueue = new Queue<LavalinkTrack>();

        string[] NumberEmojis = new string[] { ":one:", ":two:", ":three:", ":four:", ":five:" };
        static List<LavalinkTrack> tracksLoadResult = new List<LavalinkTrack>();
        bool isPlaying = false;
        static LavalinkTrack currentTrack = null;

        [Command]
        public async Task Join(CommandContext ctx)
        {
            var lava = ctx.Client.GetLavalink();
            
            var voiceState = ctx.Member.VoiceState;

            if(voiceState != null) 
            {
                var channel = voiceState.Channel;

                if (!lava.ConnectedNodes.Any())
                {
                    await ctx.RespondAsync("A connection could not be established, please contact auke");
                    return;
                }

                var node = lava.ConnectedNodes.Values.First();

                await node.ConnectAsync(channel);
                await ctx.RespondAsync($"Joined {channel.Name}");
            }
            else await ctx.RespondAsync("You are not connected to a voice channel retard!");

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
            var voiceState = ctx.Member.VoiceState;
            if (ctx.Member.VoiceState == null)
            {
                await ctx.RespondAsync("You are not in a voice channel retard!");
                return;
            }

            var channel = voiceState.Channel;
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);
            
            if (conn == null)
            {
                await node.ConnectAsync(channel);
                //await ctx.RespondAsync($"Connected to {channel.Name}");
                conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);
            }

            var loadResult = await node.Rest.GetTracksAsync(search);

            if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
            {
                await ctx.RespondAsync($"Track search failed for {search}. Try better or blame Auke.");
                return;
            }
            tracksLoadResult = loadResult.Tracks.ToList();

            var embedBuilder = new DiscordEmbedBuilder();
            var messageBuilder = new DiscordMessageBuilder();


            embedBuilder.Title = "Search results";
            embedBuilder.Description = "This is the top 5 of songs found. Please respond with the number of the song you want to play";
            DiscordComponent[] buttonList = new DiscordComponent[5];
            for (int i = 0; i < tracksLoadResult.Count; i++)
            {
                embedBuilder.AddField((i + 1) + ". " + tracksLoadResult[i].Title, tracksLoadResult[i].Author);

                DiscordButtonComponent trackButton = new DiscordButtonComponent(
                    ButtonStyle.Primary,
                    "TrackButton_" + (i+1),
                    null,
                    false,
                    new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, NumberEmojis[i])));
                buttonList[i] = trackButton;
                if (i == 4) break;

            };
            messageBuilder.AddComponents(buttonList);

            DiscordButtonComponent cancelButton = new DiscordButtonComponent(
            ButtonStyle.Danger,
            "CancelButton_0",
            null,
            false,
            new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":heavy_multiplication_x:")));


            messageBuilder.AddComponents(cancelButton);
            messageBuilder.AddEmbed(embedBuilder.Build());
            await ctx.RespondAsync(messageBuilder);          

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

            if (!isPlaying)
            {
                await ctx.RespondAsync("There are no tracks loaded.");
                return;
            }

            await conn.PauseAsync();

        }

        [Command, Description("Skips the currently playing song")]
        public async Task Skip(CommandContext ctx)
        {

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.ConnectedGuilds.Values.First();

            var wasPlaying = currentTrack;

            TimeSpan songLength = currentTrack.Length;


            if (musicQueue.Count > 0)
            {

                TimeSpan oneSeconds = new TimeSpan(0, 0, 1);
                await conn.SeekAsync(songLength - oneSeconds);


                await ctx.RespondAsync($"Skipped {wasPlaying.Title} and started playing {musicQueue.First().Title}");

            }
            else
            {
                await conn.SeekAsync(songLength);
                await ctx.RespondAsync($"Skipped {wasPlaying.Title}.");
                isPlaying = false;
                currentTrack = null;
            }

        }

        [Command, Aliases("np")]
        public async Task nowPlaying(CommandContext ctx)
        {
            //var lava = ctx.Client.GetLavalink();
            //var node = lava.ConnectedNodes.Values.First();
            //var conn = node.ConnectedGuilds.Values.First();
            //var currentTrack = conn.CurrentState.CurrentTrack;
            if(currentTrack != null) 
            {
                await ctx.RespondAsync($"The current track is {currentTrack.Title} by {currentTrack.Author}");

            }
            else
            {
                await ctx.RespondAsync("No track playing");
                return;
            }
        }

        [Command]
        public async Task reset(CommandContext ctx)
        {
            await ctx.RespondAsync("Clearing Queue and stopping music player!");
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.ConnectedGuilds.Values.First();

            musicQueue.Clear();
            await Skip(ctx);
            await conn.DisconnectAsync();
            currentTrack = null;
            tracksLoadResult.Clear();
            isPlaying = false;
            await ctx.RespondAsync("Reset done");

        }

        public async void playbackFinished(LavalinkNodeConnection node)
        {
            if (node.ConnectedGuilds.Values.IsNullOrEmpty()) return;
            var conn = node.ConnectedGuilds.Values.First();

            if (musicQueue.TryDequeue(out var nextTrack))
            {
                await conn.PlayAsync(nextTrack);
                currentTrack = nextTrack;
                isPlaying = true;
            }
            else
            {
                isPlaying = false;
                currentTrack = null;
                return;
            };
        }

        public async Task<(LavalinkTrack, int?)> AddtoQueue(LavalinkGuildConnection conn, int trackNr)
        {
            var track = tracksLoadResult[trackNr];

            if (!isPlaying)
            {
                await conn.PlayAsync(track);
                currentTrack = track;
                isPlaying = true;
                tracksLoadResult.Clear();
                return (track, null);
            }
            else
            {
                musicQueue.Enqueue(track);

                return (track, musicQueue.Count);
            }
        }
        
        
    }
}
