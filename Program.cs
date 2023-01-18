using DSharpPlus;
using DSharpPlus.CommandsNext;
using System;
using System.IO;
using System.Threading.Tasks;
using Discord_Bot.Commands;
using DSharpPlus.CommandsNext.Converters;

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
            }); ;

            var commands = discord.UseCommandsNext(new CommandsNextConfiguration()
            {
                StringPrefixes = new[] { "!" }
            });
            commands.SetHelpFormatter<DefaultHelpFormatter>();
            commands.RegisterCommands<BasicModule>();
            commands.RegisterCommands<CivRolls>();
            await discord.ConnectAsync();
            await Task.Delay(-1);
        }
    }

}
