using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;

namespace Discord_Bot;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using System;
using System.IO;
using System.Threading.Tasks;
using Discord_Bot.Commands;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using System.Reflection.Metadata;
using DSharpPlus.Net;
using DSharpPlus.Lavalink;
using System.Linq;
using DSharpPlus.Lavalink.Entities;
using System.Timers;
using RiotSharp.Endpoints.LeagueEndpoint;
using KGySoft.CoreLibraries;
using KGySoft.ComponentModel;

public class Bot
{
    private DiscordClient discord;
    private LavalinkExtension lavalink;
    private bool spamReactions = true;
    
    const string auke = "sonicos1";
    const string max = "maddestofmaxes";
    const string koen = "Neoblasterz";
    (string,string)[] pairs = {(auke, ":cum:"), (max, ":clown:"), (koen, ":men_wrestling:")};
    
    public async Task Init()
    {
        var root = Directory.GetCurrentDirectory();
        var dotenv = Path.Combine(root, ".env");
        DotEnv.Load(dotenv);
        discord = new DiscordClient(new DiscordConfiguration()
        {
            Token = Environment.GetEnvironmentVariable("DiscordToken"),
            TokenType = TokenType.Bot,
            Intents = DiscordIntents.All,
            MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Debug,
            LogTimestampFormat = "dd MMM yyyy - hh:mm:ss"
        });
        
        var endpoint = new ConnectionEndpoint
        {
            Hostname = "127.0.0.1", // From your server configuration.
            Port = 2333 // From your server configuration
        };
        var lavalinkConfig = new LavalinkConfiguration
        {
            Password = "youshallnotpass", // From your server configuration.
            RestEndpoint = endpoint,
            SocketEndpoint = endpoint
        };
        lavalink = discord.UseLavalink();

        await discord.ConnectAsync();
        await lavalink.ConnectAsync(lavalinkConfig);

        funnyReactions();
        funnyReplies();
        registerCommands();
        musicShit();
        leagueShit();
        Pickwick pickwick = new Pickwick(discord);
        pickwick.Init();


        await Task.Delay(-1);
    }

    /// <summary>
    /// I don't know what this does
    /// </summary>
    private void musicShit()
    {
        Music music = new Music();
        var test = lavalink.GetIdealNodeConnection();

        if (lavalink.ConnectedNodes.Any())
        {

            var lava = discord.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            
            node.PlaybackFinished += (s, e) => {
                music.playbackFinished(node);
                return Task.CompletedTask;
            };
        }

        discord.ComponentInteractionCreated += async (s, e) =>
        {
            var lava = discord.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.ConnectedGuilds.Values.First();

            string buttonNr = e.Id.Split('_')[1];
            int trackNr = int.Parse(buttonNr) - 1; 
            if (trackNr == -1)
            {
                await e.Message.RespondAsync("You cancelled the command!");
                return;
            }

            (LavalinkTrack track, int? count) = music.AddtoQueue(conn, trackNr).Result;

            if(count is null)
            {
                await e.Interaction.CreateResponseAsync(
                    InteractionResponseType.UpdateMessage,
                    new DiscordInteractionResponseBuilder()
                        .WithContent($"Now playing {track.Title}!"));
            }
            else
            {
                await e.Interaction.CreateResponseAsync(
                    InteractionResponseType.UpdateMessage,
                    new DiscordInteractionResponseBuilder()
                        .WithContent($"Put {track.Title} into the queue. It's in {count}th place."));
            }
        };
    }
    /// <summary>
    /// I don't know what this does
    /// </summary>
    private void leagueShit()
    {
        LeagueModule leagueApi = new LeagueModule();
        //string apiKey = Environment.GetEnvironmentVariable("RiotApiKey");
        //leagueApi.leagueClient.DefaultRequestHeaders.Add("X-Riot-Token", apiKey);
        //leagueApi.leagueClient.DefaultRequestHeaders.Add("Origin", "https://developer.riotgames.com");

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
    private void funnyReplies()
    {
        discord.MessageCreated += async (s, e) =>
        {
            string username = e.Author.Username;
            bool isMaintainer = username == auke || username == koen;
            string message = e.Message.Content.ToLower();

            if ((message == "who asked" || message == "who asked?") && !isMaintainer)
                await e.Message.RespondAsync("I did");
            
            if (((message.Contains("im busy") || message.Contains("i'm busy")) && username == max)
                || ((message.Contains("im straight") || message.Contains("i'm straight")) && username == auke)) await e.Message.RespondAsync("Cap!");
        };
    }
    /// <summary>
    /// Adds funny reactions to messages
    /// </summary>
    private void funnyReactions()
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
    private void registerCommands()
    {
        var commands = discord.UseCommandsNext(new CommandsNextConfiguration()
        {
            StringPrefixes = new[] { "!" }
        });
        commands.SetHelpFormatter<CustomHelpFormatter>();
        commands.RegisterCommands<Misc>();
        commands.RegisterCommands<CivRolls>();
        commands.RegisterCommands<LeagueModule>();
        commands.RegisterCommands<Music>();
    }
}