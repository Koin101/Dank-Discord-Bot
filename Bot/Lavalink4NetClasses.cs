namespace Discord_Bot;
using System;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using Lavalink4NET;
using Lavalink4NET.Extensions;
using Lavalink4NET.Players;
using Lavalink4NET.Players.Queued;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

class ApplicationHost : BackgroundService
{
    private readonly DiscordClient _discordClient;
    private readonly IAudioService _audioService;

    public ApplicationHost(DiscordClient discordClient, IAudioService audioService)
    {
        ArgumentNullException.ThrowIfNull(discordClient);
        ArgumentNullException.ThrowIfNull(audioService);

        _discordClient = discordClient;
        _audioService = audioService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // connect to discord gateway and initialize node connection
        await _discordClient
            .ConnectAsync()
            .ConfigureAwait(false);

        await _audioService
            .WaitForReadyAsync(stoppingToken)
            .ConfigureAwait(false);

        var playerOptions = new LavalinkPlayerOptions
        {
            InitialTrack = new TrackQueueItem("https://www.youtube.com/watch?v=dQw4w9WgXcQ"),
        };

        await _audioService.Players
            .JoinAsync(0, 0, playerOptions, stoppingToken) // Ids
            .ConfigureAwait(false);
    }
}