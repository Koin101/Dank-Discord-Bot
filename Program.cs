using DSharpPlus;
using DSharpPlus.CommandsNext;
using System;
using System.IO;
using System.Threading.Tasks;
using Discord_Bot.Commands;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using System.Reflection.Metadata;

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

                string username = e.Author.Username;
                string message = e.Message.Content.ToLower();
                if (username == "maddestofmaxes")
                    await e.Message.CreateReactionAsync(DiscordEmoji.FromName(s, ":clown:"));
                bool isMaintainer = username == "sonicos1" || username == "Neoblasterz";
                if ((message == "who asked" || message == "who asked?") && !isMaintainer)
                    await e.Message.RespondAsync("I did");
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
                StringPrefixes = new[] { "!" }
            });
            commands.SetHelpFormatter<CustomHelpFormatter>();
            commands.RegisterCommands<Misc>();
            commands.RegisterCommands<CivRolls>();
            commands.RegisterCommands<LeagueModule>();

            

            await discord.ConnectAsync();
            await Task.Delay(-1);
        }
    }

}

