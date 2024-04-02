using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bot;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Lavalink4NET.Lyrics;
using Lavalink4NET.Players.Queued;
using Microsoft.Extensions.Logging;

namespace Discord_Bot.Commands;
using Lavalink4NET.Extensions;
using Lavalink4NET;
using Lavalink4NET.Players;
using Lavalink4NET.Rest.Entities.Tracks;

/// <summary>
///     Shamelessly copied from https://github.com/steele123/dj_x/blob/main/Bot/
///     Cause I did not know how to use lavalink4net with DSharpPlus, the documentation is at least to me very unclear
///     and you cannot search it for some reason...
/// </summary>
public class MusicLavalink40(IAudioService audioService, ILogger<MusicLavalink40> logger, ILyricsService lyricsService)
    : ApplicationCommandModule
{
    
    [SlashCommand("status", "Checks whether the bot is online and able to play music")]
    public async Task Status(InteractionContext ctx)
    {
        await ctx.DeferAsync();

        var embed = new DiscordEmbedBuilder()
            .WithDescription("Im ready to cook some music");

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
    }
    
    [SlashCommand("play", "Plays a song")]
    public async Task Play(InteractionContext ctx,
        [Option("query", "The song to play")] string query,
        [Option("provider", "The website to provide the sound (default: YouTube)")]
        SoundProvider provider = SoundProvider.YouTube,
        [Option("bump", "Whether to bump the song to the top of the queue")]
        bool bump = false)
    {
        logger.LogInformation("Playing {Query} from {Provider}", query, provider);
        await ctx.DeferAsync(true);
        
        var opts = new EmbedDisplayPlayerOptions
        {
            SelfDeaf = true,
            HistoryCapacity = 20,
            DisconnectOnDestroy = true,
            ClearQueueOnStop = true,
        };

        var vc = ctx.Member?.VoiceState?.Channel;
        if (vc is null)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("You must be in a voice channel."));
            return;
        }

        var player = await audioService.Players.JoinAsync<EmbedDisplayPlayer, EmbedDisplayPlayerOptions>(ctx.Guild.Id,
            vc.Id,
            CreatePlayerAsync, opts);
        
        
        if (player.State != PlayerState.Playing)
        {
            var embed = new DiscordEmbedBuilder()
                .WithDescription("I'm cookin that shit up for ya, gimme a sec")
                ;

            var msg = await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithContent("").AddEmbed(embed));

            if (msg is null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Failed to create embed message."));
                return;
            }

            player.EmbedMessage = msg;
        }

        var searchMode = GetTrackSearchMode(provider);
            
        var isPlaylist = query.Contains("playlist");
        if (isPlaylist)
        {
            var tracks = await audioService.Tracks.LoadTracksAsync(query, searchMode);
            if (tracks.Count is 0)
            {
                await ctx.EditResponseAsync(
                    new DiscordWebhookBuilder().WithContent(
                        "No tracks found, try a different provider with the command options."));
                return;
            }

            var firstTrack = tracks.Tracks.Take(1).First();
            var queueItems = tracks.Tracks.Select(x => new TrackQueueItem(new TrackReference(x))).ToList();
            await player.Queue.AddRangeAsync(queueItems);
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                $"Added {tracks.Count + 1} tracks to the queue from playlist {tracks.Playlist!.Name}"));
            await player.PlayAsync(firstTrack, false);
            return;
        }

        var track = await audioService.Tracks.LoadTrackAsync(query, searchMode);
        if (track is null)
        {
            await ctx.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent(
                    "No tracks found, try a different provider with the command options."));
            return;
        }

        var pos = await player.PlayAsync(track);

        if (pos is 0)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Playing {track.Title}"));
            return;
        }

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Added {track.Title} to the queue"));
    }
    
    [SlashCommand("nowplaying", "Shows the current song")]
    public async Task NowPlaying(InteractionContext ctx)
    {
        await ctx.DeferAsync();

        var player = await audioService.Players.GetPlayerAsync<EmbedDisplayPlayer>(ctx.Guild.Id);
        if (player is null)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("I am not playing anything."));
            return;
        }

        var currentTrack = player.CurrentTrack;
        if (currentTrack is null)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("No track playing."));
            return;
        }

        var embed = new DiscordEmbedBuilder()
            .WithTitle(currentTrack.Title)
            .WithDescription(currentTrack.Author)
            .WithUrl(currentTrack.Uri)
            .WithThumbnail(currentTrack.ArtworkUri);

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
    }
    
    [SlashCommand("pause", "Pauses the current song")]
    public async Task Pause(InteractionContext ctx)
    {
        await ctx.DeferAsync(true);

        var player = await audioService.Players.GetPlayerAsync<EmbedDisplayPlayer>(ctx.Guild.Id);
        if (player is null)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("I am not playing anything."));
            return;
        }

        await player.PauseAsync();
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Paused"));
    }

    [SlashCommand("queue", "Shows the current queue")]
    public async Task Queue(InteractionContext ctx)
    {
        await ctx.DeferAsync();

        var player = await audioService.Players.GetPlayerAsync<EmbedDisplayPlayer>(ctx.Guild.Id);
        if (player is null)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("I am not playing anything. " +
                "So there is no queue smartypants"));
            return;
        }

        var embed = new DiscordEmbedBuilder();

        var queue = player.Queue;
        int i = 1;
        if (!(queue.Count > 0))
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("There is nothing in the queue!"));
            return;
        }
        
        foreach (var song in  queue)
        {
            if (song.Track is null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("There was an error"));
                return;
            }

            embed.Title = "The Queue";
            embed.AddField($"{i}. ", song.Track.Title, false);
            i++;
        }

        await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithContent("").WithEmbed(embed));
    }

    [SlashCommand("position", "Shows the progress of the current track")]
    public async Task Position(InteractionContext ctx)
    {
        await ctx.DeferAsync(true);

        var player = await audioService.Players.GetPlayerAsync(ctx.Guild.Id);

        if (player is null)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                .WithContent("im not playing anything."));
            return;
        }

        if (player.CurrentTrack is null)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                .WithContent("im not playing anything."));
            return;
        }

        var embed =  new DiscordEmbedBuilder()
            .WithDescription($"Position: {player.Position?.Position} / {player.CurrentTrack.Duration}.");

        await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder()
            .WithContent("").WithEmbed(embed));
    }
    
    [SlashCommand("lyrics", "Shows the lyrics of the current song")]
    public async Task Lyrics(InteractionContext ctx)
    {
        await ctx.DeferAsync();

        var player = await audioService.Players.GetPlayerAsync<EmbedDisplayPlayer>(ctx.Guild.Id);
        if (player is null)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("I am not playing anything."));
            return;
        }

        var currentTrack = player.CurrentTrack;
        if (currentTrack is null)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("No track playing."));
            return;
        }
        
        var lyrics = await lyricsService.GetLyricsAsync(currentTrack.Author, 
            currentTrack.Title.Split(" - ")[1]);
        if (lyrics is null)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("No lyrics found."));
            return;
        }

        var embed = new DiscordEmbedBuilder()
            .WithTitle(currentTrack.Title)
            .WithDescription(lyrics)
            .WithThumbnail(currentTrack.ArtworkUri);

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
    }
    
    private static ValueTask<EmbedDisplayPlayer> CreatePlayerAsync(
        IPlayerProperties<EmbedDisplayPlayer, EmbedDisplayPlayerOptions> properties,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(properties);
        
        return ValueTask.FromResult(new EmbedDisplayPlayer(properties));
    }

    [SlashCommand("skip", "Skips the current song")]
    public async Task Skip(InteractionContext ctx)
    {
        await ctx.DeferAsync();
        var player = await audioService.Players.GetPlayerAsync<EmbedDisplayPlayer>(ctx.Guild.Id);

        if (player is null)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                .WithContent("I am not playing anything."));
            return;   
        }

        var skippedSong = player.CurrentTrack;
        await player.SkipAsync();
        await ctx.EditResponseAsync(new DiscordWebhookBuilder()
            .WithContent($"Skipped {skippedSong.Title}"));

    }
    
    
    /* A slash command which removes a specified song from the queue*/ 
    [SlashCommand("remove", "Removes a song from the queue")]
    public async Task Remove(InteractionContext ctx,
        [Option("index", "The index of the song to remove")] int index)
    {
        await ctx.DeferAsync();
        var player = await audioService.Players.GetPlayerAsync<EmbedDisplayPlayer>(ctx.Guild.Id);

        if (player is null)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                .WithContent("I am not playing anything."));
            return;   
        }

        if (index < 1 || index > player.Queue.Count)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                .WithContent("Invalid index."));
            return;
        }

        var removedSong = player.Queue.ElementAt(index - 1);
        await player.Queue.RemoveAtAsync(index - 1);
        await ctx.EditResponseAsync(new DiscordWebhookBuilder()
            .WithContent($"Removed {removedSong.Track!.Title}"));
    }
    
    [SlashCommand("stop", "Stops the current song")]
    public async Task Stop(InteractionContext ctx)
    {
        await ctx.DeferAsync();
        var player = await audioService.Players.GetPlayerAsync<EmbedDisplayPlayer>(ctx.Guild.Id);

        if (player is null)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                .WithContent("I am not playing anything."));
            return;   
        }

        await player.StopAsync();
        await ctx.EditResponseAsync(new DiscordWebhookBuilder()
            .WithContent("Stopped"));
    }
    
    [SlashCommand("resume", "Resumes the current song")]
    public async Task Resume(InteractionContext ctx)
    {
        await ctx.DeferAsync();
        var player = await audioService.Players.GetPlayerAsync<EmbedDisplayPlayer>(ctx.Guild.Id);

        if (player is null)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                .WithContent("I am not playing anything."));
            return;   
        }

        await player.ResumeAsync();
        await ctx.EditResponseAsync(new DiscordWebhookBuilder()
            .WithContent("Resumed"));
    }
    
    [SlashCommand("repeat", "Repeats the current song")]
    public async Task Repeat(InteractionContext ctx)
    {
        await ctx.DeferAsync();
        var player = await audioService.Players.GetPlayerAsync<EmbedDisplayPlayer>(ctx.Guild.Id);

        if (player is null)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                .WithContent("I am not playing anything."));
            return;   
        }

        player.RepeatMode = player.RepeatMode == TrackRepeatMode.None
            ? TrackRepeatMode.Queue
            : TrackRepeatMode.None;
        
        await ctx.EditResponseAsync(new DiscordWebhookBuilder()
            .WithContent("Repeat mode toggled"));
    }
    
    private static TrackSearchMode GetTrackSearchMode(SoundProvider provider)
    {
        return provider switch
        {
            SoundProvider.YouTube => TrackSearchMode.YouTube,
            SoundProvider.YouTubeMusic => TrackSearchMode.YouTubeMusic,
            SoundProvider.SoundCloud => TrackSearchMode.SoundCloud,
            SoundProvider.Spotify => TrackSearchMode.Spotify,
            SoundProvider.AppleMusic => TrackSearchMode.AppleMusic,
            SoundProvider.Deezer => TrackSearchMode.Deezer,
            SoundProvider.YandexMusic => TrackSearchMode.YandexMusic,
            SoundProvider.Plain => TrackSearchMode.None,
            _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, null)
        };
    }
    

}
public enum SoundProvider
{
    [ChoiceName("YouTube Music")] YouTubeMusic,
    [ChoiceName("Apple Music")] AppleMusic,
    [ChoiceName("SoundCloud")] SoundCloud,
    [ChoiceName("Deezer")] Deezer,
    [ChoiceName("YouTube")] YouTube,
    [ChoiceName("Spotify")] Spotify,
    [ChoiceName("Yandex Music")] YandexMusic,
    [ChoiceName("Plain")] Plain
}