using System;
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

namespace Discord_Bot.Commands
{
    
    public class Music : BaseCommandModule
    {
        Queue<LavalinkTrack> musicQueue = new Queue<LavalinkTrack>();

        string[] NumberEmojis = new string[] { ":one:", ":two:", ":three:", ":four:", ":five:" };
        List<LavalinkTrack> tracksLoadResult = new List<LavalinkTrack>();

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

            if (conn.CurrentState.CurrentTrack == null)
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
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            var wasPlaying = conn.CurrentState.CurrentTrack;
            await conn.StopAsync();
            if (musicQueue.TryDequeue(out var music))
            {
                await conn.PlayAsync(music);
                await ctx.RespondAsync($"Skipped {wasPlaying.Title} and started playing {music.Title}");
            }
            else
            {
                await ctx.RespondAsync($"Skipped {wasPlaying.Title}.");
            }
        }

        [Command, Aliases("np")]
        public async Task nowPlaying(CommandContext ctx)
        {
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            var currentTrack = conn.CurrentState.CurrentTrack;
            if(currentTrack != null) 
            {
                await ctx.RespondAsync($"The current track is {currentTrack.Title} by {currentTrack.Author}");

            }
            else
            {
                await ctx.RespondAsync("No track to skip");
                return;
            }
        }

        public async void playbackFinished(LavalinkGuildConnection conn)
        {
            if (musicQueue.TryDequeue(out var nextTrack))
            {
                await conn.PlayAsync(nextTrack);
            }
            else return;
        }

        public async Task<(LavalinkTrack, int?)> AddtoQueue(LavalinkGuildConnection conn, int trackNr)
        {
            var track = tracksLoadResult[trackNr];

            if (conn.CurrentState.CurrentTrack == null)
            {
                await conn.PlayAsync(track);

                return (track, null);
            }
            else
            {
                Console.WriteLine(track.Title);
                musicQueue.Enqueue(track);

                return (track, musicQueue.Count);
            }
        }
    }
}
