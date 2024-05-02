using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Bot;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using JsonFlatFileDataStore;
using Lavalink4NET.Events.Players;
using Lavalink4NET.Players.Queued;

namespace Discord_Bot;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using System;
using System.IO;
using System.Threading.Tasks;
using Discord_Bot.Commands;
using DSharpPlus.Entities;
using DSharpPlus.Net;
using DSharpPlus.Lavalink;
using DSharpPlus.EventArgs;
using System.Linq;
using System.Timers;
using Lavalink4NET;
using Lavalink4NET.Extensions;
using Lavalink4NET.Players;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;


public class Bot(
        ILogger<Bot> logger,
        DiscordClient discord,
        IAudioService audioService,
        IServiceProvider serviceProvider,
        IServiceScopeFactory serviceScopeFactory) : BackgroundService
{
    private bool spamReactions = true;
    const string auke = "sonicos1";
    const string max = "maddestofmaxes";
    const string koen = "Neoblasterz";
    (string,string)[] pairs = {(auke, ":cum:"), (max, ":clown:"), (koen, ":men_wrestling:")};

    private static DataStore jsonDB = new DataStore(Path.Join(Directory.GetCurrentDirectory(), "DankUsers.json"));
    private IDocumentCollection<DankUser> dankUserCollection = jsonDB.GetCollection<DankUser>();
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        RegisterStandardCommands();
        
        RegisterSlashCommands();

        discord.ComponentInteractionCreated += ClientOnComponentInteractionCreated;
        audioService.WebSocketClosed += AudioServiceOnWebSocketClosed;

        await discord.ConnectAsync();

        await audioService.WaitForReadyAsync(stoppingToken);


        logger.LogInformation("Connected to Discord and Lavalink");
        
        // LeagueShit(); something is broken do not want to fix it
        FunnyReplies();
        Pickwick pickwick = new Pickwick(discord);
        pickwick.Init();

    }

    
    /// <summary>
    /// I don't know what this does
    /// </summary>
    private void LeagueShit()
    {
        LeagueModule leagueApi = new LeagueModule();

        Timer leagueTime = new(interval: 3600000); 
        leagueTime.Enabled = true;
        leagueTime.AutoReset = true;
        leagueTime.Start();
        
        leagueTime.Elapsed += async (s, e) =>
        {
            var channel = await discord.GetChannelAsync(572581995276664838);

            var embedGregoor = leagueApi.GetLastMatchFromSummonorTimerEvent("Gr3goor");
            if(embedGregoor != null) await channel.SendMessageAsync(embedGregoor);

            var embedMax = leagueApi.GetLastMatchFromSummonorTimerEvent("madismax");
            if(embedMax != null) await channel.SendMessageAsync(embedMax);
        };
    }
    /// <summary>
    /// Makes funny replies to messages
    /// </summary>
    private void FunnyReplies()
    {
        discord.MessageCreated += async (s, e) =>
        {
            string username = e.Author.Username;
            bool isMaintainer = username == auke || username == koen;
            string message = e.Message.Content.ToLower();

            if ((message == "who asked" || message == "who asked?") && !isMaintainer)
                await e.Message.RespondAsync("I did");
            
            if (((message.Contains("im busy") || message.Contains("i'm busy")) && username == max)
                || ((message.Contains("im straight") || message.Contains("i'm straight")) && username == auke)) 
                await e.Message.RespondAsync("Cap!");
        };
    }
    /// <summary>
    /// Adds funny reactions to messages
    /// </summary>
    private void FunnyReactions()
    {
        //TODO: very obvious code duplication, but the eventArgs make this dfficult to fix.
        discord.MessageCreated += async (s, e) =>
        {
            string username = e.Author.Username;
            if(spamReactions)
                foreach (var (name,emoji) in pairs)
                    if (username == name)
                        await e.Message.CreateReactionAsync(DiscordEmoji.FromName(s, emoji));
        };

        discord.MessageReactionRemovedEmoji += async (s, e) =>
        {
            string username = e.Message.Author.Username;
            if(spamReactions)
                foreach (var (name,emoji) in pairs)
                    if (username == name)
                        await e.Message.CreateReactionAsync(DiscordEmoji.FromName(s, emoji));
        };

        discord.MessageReactionsCleared += async (s, e) =>
        {
            string username = e.Message.Author.Username;
            if(spamReactions)
                foreach (var (name,emoji) in pairs)
                    if (username == name)
                        await e.Message.CreateReactionAsync(DiscordEmoji.FromName(s, emoji));
        };
    }
    /// <summary>
    /// Sets the prefix and registers the commands classes to be used by the bot
    /// </summary>
    private void RegisterStandardCommands()
    {
        var commands = discord.UseCommandsNext(new CommandsNextConfiguration()
        {
            StringPrefixes = new[] { "!!" }
        });

        commands.SetHelpFormatter<CustomHelpFormatter>();
        
        commands.RegisterCommands<Misc>();
        commands.RegisterCommands<LeagueModule>();

    }

    private void RegisterSlashCommands()
    {
        var commands = discord.UseSlashCommands(new SlashCommandsConfiguration
        {
            Services = serviceProvider
        });
        
        commands.RegisterCommands<CivRolls>(Convert.ToUInt64(
            Environment.GetEnvironmentVariable("GuildID")));
        commands.RegisterCommands<MusicLavalink40>(Convert.ToUInt64(
            Environment.GetEnvironmentVariable("GuildID")));
    }
    
    private Task AudioServiceOnWebSocketClosed(object sender, WebSocketClosedEventArgs eventargs)
    {
        logger.LogInformation("Socket Closed - Code: {Code}, Reason: {Reason}", eventargs.CloseCode, eventargs.Reason);
        return Task.CompletedTask;
    }
    
     private async Task ClientOnComponentInteractionCreated(DiscordClient sender,
        ComponentInteractionCreateEventArgs args)
    {
        // var scope = serviceScopeFactory.CreateScope();
        var id = args.Id!;
        
        var player = await audioService.Players.GetPlayerAsync<EmbedDisplayPlayer>(args.Guild.Id);
        if (player is null)
        {
            await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent("No player found").AsEphemeral());
            return;
        }

        var member = args.Guild.Members.GetValueOrDefault(args.User.Id);
        if (member is null)
        {
            await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent("No member found").AsEphemeral());
            return;
        }

        var vc = member.VoiceState?.Channel;
        if (vc?.Id != player.VoiceChannelId)
        {
            await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent("You are not in the same voice channel as the bot")
                    .AsEphemeral());
            return;
        }

        switch (id)
        {
            case "toggle_playback":
            {
                if (player.IsPaused)
                    await player.ResumeAsync();
                else
                    await player.PauseAsync();

                break;
            }
            case "skip":
            {
                await player.SkipAsync();
                break;
            }
            case "stop":
            {
                await player.StopAsync();
                break;
            }
            case "toggle_repeat":
            {
                player.RepeatMode = player.RepeatMode == TrackRepeatMode.None
                    ? TrackRepeatMode.Queue
                    : TrackRepeatMode.None;

                await player.TriggerMessageUpdate();

                break;
            }
            case "toggle_shuffle":
            {
                player.Shuffle = !player.Shuffle;

                await player.TriggerMessageUpdate();

                break;
            }
        }

        await args.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
    }

    private void LoadDankUsersFromFile()
    {
        var path = Directory.GetCurrentDirectory();
        var fileName = "DankUsers.json";

        foreach (var line in File.ReadLines(Path.Join(path, fileName)))
        {
            
        }
    }

}