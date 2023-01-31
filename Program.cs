using DSharpPlus;
using DSharpPlus.CommandsNext;
using System;
using System.IO;
using System.Threading.Tasks;
using Discord_Bot.Commands;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;

namespace Discord_Bot
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();

        }

        static async Task MainAsync()
        {
            var root = Directory.GetCurrentDirectory();
            var dotenv = Path.Combine(root, ".env");
            DotEnv.Load(dotenv);

            var discord = new DiscordClient(new DiscordConfiguration()
            {
                Token = Environment.GetEnvironmentVariable("DiscordToken"),
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.All,
                MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Debug,
                LogTimestampFormat = "dd MMM yyyy - hh:mm:ss"
            }); 

            discord.MessageCreated += async (s, e) =>
            {
                if (e.Author.Username == "maddestofmaxes") { await e.Message.CreateReactionAsync(DiscordEmoji.FromName(s, ":clown:")); }
            };

            discord.MessageReactionRemoved += async (s, e) =>
            {
                if (e.Emoji.Name == "\U0001f921") { await e.Message.CreateReactionAsync(DiscordEmoji.FromName(s, ":clown:")); }
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
                StringPrefixes = new[] { "!" }
            });
            commands.SetHelpFormatter<CustomHelpFormatter>();
            commands.RegisterCommands<BasicModule>();
            commands.RegisterCommands<CivRolls>();
            commands.RegisterCommands<LeagueModule>();

            

            await discord.ConnectAsync();
            await Task.Delay(-1);
        }
    }

}

