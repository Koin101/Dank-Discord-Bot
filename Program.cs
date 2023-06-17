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

namespace Discord_Bot
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();

        }

        async void TimerEvent(DiscordClient discord)
        {
            var guild = await discord.GetGuildAsync(970439295724826704);
            var bot = await guild.GetMemberAsync(1089649232090234951);
            await bot.ModifyAsync(x => x.VoiceChannel = null);
            Console.WriteLine("The bot is disconnected");
            
        }

        static async Task MainAsync()
        {
            Timer timer = new(interval: 10000);

            var root = Directory.GetCurrentDirectory();
            var dotenv = Path.Combine(root, ".env");
            DotEnv.Load(dotenv);

            Program p = new Program();

            LeagueModule leagueApi = new LeagueModule();
            Music music = new Music();
            string apiKey = Environment.GetEnvironmentVariable("RiotApiKey");
            leagueApi.leagueClient.DefaultRequestHeaders.Add("X-Riot-Token", apiKey);

            Timer leagueTime = new(interval: 1800000);
            leagueTime.Start();

            var discord = new DiscordClient(new DiscordConfiguration()
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


            var lavalink = discord.UseLavalink();

            discord.MessageCreated += async (s, e) =>
            {
                if (e.Author.Username == "maddestofmaxes") { await e.Message.CreateReactionAsync(DiscordEmoji.FromName(s, ":clown:")); }
                if (e.Author.Username == "sonicos1") { await e.Message.CreateReactionAsync(DiscordEmoji.FromName(s, ":ping:")); }
                string username = e.Author.Username;
                string message = e.Message.Content.ToLower();
                if (username == "maddestofmaxes")
                    await e.Message.CreateReactionAsync(DiscordEmoji.FromName(s, ":clown:"));
                bool isMaintainer = username == "sonicos1" || username == "Neoblasterz";
                if ((message == "who asked" || message == "who asked?") && !isMaintainer)
                    await e.Message.RespondAsync("I did");
                if ((message.Contains("im busy") || message.Contains("i'm busy")) && username == "maddestofmaxes") await e.Message.RespondAsync("Cap!");
            };

            discord.MessageReactionRemoved += async (s, e) =>
            {
                if (e.Emoji.Name == "\U0001f921" && e.User.Username == "maddestofmaxes") { await e.Message.CreateReactionAsync(DiscordEmoji.FromName(s, ":clown:")); }
            };

            discord.MessageReactionRemovedEmoji += async (s, e) =>
            {
                if (e.Emoji.Name == "\U0001f921") { await e.Message.CreateReactionAsync(DiscordEmoji.FromName(s, ":clown:")); }
            };

            discord.MessageReactionsCleared += async (s, e) =>
            {
                
                await e.Message.CreateReactionAsync(DiscordEmoji.FromName(s, ":clown:"));
            };

            var commands = discord.UseCommandsNext(new CommandsNextConfiguration()
            {
                StringPrefixes = new[] { "!!" }
            });


            leagueTime.Elapsed += async (s, e) =>
            {

            };

            if (lavalink.ConnectedNodes.Any())
            {

                var lava = discord.GetLavalink();
                var node = lava.ConnectedNodes.Values.First();
                var conn = node.ConnectedGuilds.Values.First();

                conn.PlaybackFinished += async (s, e) =>
                {
                    music.playbackFinished(conn);
                };
            }

            discord.ComponentInteractionCreated += async (s, e) =>
            {
                var lava = discord.GetLavalink();
                var node = lava.ConnectedNodes.Values.First();
                var conn = node.ConnectedGuilds.Values.First();

                string buttonNr = e.Id.Split('_')[1];
                int trackNr = 0;
                if (buttonNr == "1") trackNr = 0;
                else if (buttonNr == "2")
                    trackNr = 1;
                else if (buttonNr == "3")
                    trackNr = 2;
                else if (buttonNr == "4")
                    trackNr = 3;
                else if (buttonNr == "5")
                    trackNr = 4;
                else if (buttonNr == "0")
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

            //discord.VoiceStateUpdated += async (s, e) =>
            //{
            //    if (e.User.IsBot) 
            //    {
            //        //start timer
            //        timer.Start();                    
            //    }
            //};

            //timer.Elapsed += async (s, e) =>
            //{
            //    Console.WriteLine("Timer finished");
            //    p.TimerEvent(discord);

            //};
            //is timer afgelopen en is de bot connected en speelt hij geen muziek. Disconnect the bot
            //if (lavalink.ConnectedNodes.Any())
            //{
            //    var lava = discord.GetLavalink();
            //    var node = lava.ConnectedNodes.Values.First();
            //    var conn = node.GetGuildConnection(discord.Guilds.Values.First());
            //    var currentState = conn.CurrentState;
            //    if(currentState == null) { await conn.DisconnectAsync(); }
                
            //}



            commands.SetHelpFormatter<CustomHelpFormatter>();
            commands.RegisterCommands<Misc>();
            commands.RegisterCommands<CivRolls>();
            commands.RegisterCommands<LeagueModule>();
            commands.RegisterCommands<Music>();
            

            await discord.ConnectAsync();
            await lavalink.ConnectAsync(lavalinkConfig);


            await Task.Delay(-1);
        }
    }

}

