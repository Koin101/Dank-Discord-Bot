﻿using System;
using System.IO;
using DSharpPlus;
using DSharpPlus.Lavalink;
using DSharpPlus.Net;
using Lavalink4NET;
using Lavalink4NET.Extensions;
using Lavalink4NET.InactivityTracking.Extensions;
using Lavalink4NET.Lyrics.Extensions;
using Lavalink4NET.Players;
using Lavalink4NET.Rest;
using Lavalink4NET.Rest.Entities.Tracks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Discord_Bot;

public class Program
{
    //This does a lot of stuff which I do not really understand what it does
    //All the builder stuff is for the music part of the bot
    static void Main(string[] args)
    {
        var root = Directory.GetCurrentDirectory();
        var dotenv = Path.Combine(root, "Data/.env");
        DotEnv.Load(dotenv);
        var builder = Host.CreateApplicationBuilder(args);

        builder.Services.AddMemoryCache();
        builder.Services.AddHostedService<Bot>();
        builder.Services.AddSingleton<DiscordClient>();
        builder.Services.AddSingleton<DiscordConfiguration>(_ =>
        {
            var token = Environment.GetEnvironmentVariable("DiscordToken");
            var config = new DiscordConfiguration
            {
                Token = token,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.AllUnprivileged,
                MinimumLogLevel = LogLevel.Debug,
                LogUnknownEvents = false
            };

            return config;
        });

        builder.Services.AddLavalink();
        builder.Services.ConfigureLavalink(cfg =>
        {
            cfg.BaseAddress = new Uri("http://lavalink:2333");
        });
        builder.Services.AddLyrics();
        builder.Services.AddInactivityTracking();
        
        builder.Services.ConfigureInactivityTracking(cfg =>
        {
            //Leave VC after 60 sec of no music playing
            cfg.DefaultTimeout = TimeSpan.FromSeconds(360);
        });
        
        
    

        var host = builder.Build();
        host.Run();  
    }
}