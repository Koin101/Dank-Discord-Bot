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
    [RequireOwner]
    public class Music : BaseCommandModule
    {
        Queue<LavalinkTrack> musicQueue = new Queue<LavalinkTrack>();

        string[] NumberEmojis = new string[] { ":one:", ":two:", ":three:", ":four:", ":five:" };

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
            var tracks = loadResult.Tracks.ToList();

            var embedBuilder = new DiscordEmbedBuilder();
            var messageBuilder = new DiscordMessageBuilder();


            embedBuilder.Title = "Search results";
            embedBuilder.Description = "This is the top 5 of songs found. Please respond with the number of the song you want to play";
            DiscordComponent[] buttonList = new DiscordComponent[5];
            for (int i = 0; i < tracks.Count; i++)
            {
                embedBuilder.AddField((i + 1) + ". " + tracks[i].Title, tracks[i].Author);

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


            ctx.Client.ComponentInteractionCreated += async (s, e) =>
            {
                LavalinkTrack track = null;

                string buttonNr = e.Id.Split('_')[1];

                if (buttonNr == "1") track = tracks[0];
                else if (buttonNr == "2")
                    track = tracks[1];
                else if (buttonNr == "3")
                    track = tracks[2];
                else if (buttonNr == "4")
                    track = tracks[3];
                else if (buttonNr == "5")
                    track = tracks[4];
                else if (buttonNr == "0") { 
                    await ctx.RespondAsync("You cancelled the command!");
                    return; }

                if (conn.CurrentState.CurrentTrack == null) 
                {
                    await conn.PlayAsync(track);

                    await e.Interaction.CreateResponseAsync(
                    InteractionResponseType.UpdateMessage,
                    new DiscordInteractionResponseBuilder()
                        .WithContent($"Now playing {track.Title}!"));
                } 
                else
                {
                    Console.WriteLine(track.Title);
                    musicQueue.Enqueue(track);

                    await e.Interaction.CreateResponseAsync(
                    InteractionResponseType.UpdateMessage,
                    new DiscordInteractionResponseBuilder()
                        .WithContent($"Put {track.Title} into the queue. It's in {musicQueue.Count}th place."));
                }                
            };

            conn.PlaybackFinished += async (s, e) =>
            {
                if(musicQueue.TryDequeue(out var nextTrack))
                {
                    await conn.PlayAsync(nextTrack);

                }
                else
                {
                    return;
                }

            };


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
    }
}
