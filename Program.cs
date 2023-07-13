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

namespace Discord_Bot
{
    class Program
    {
        static void Main(string[] args)
        {
            Bot bot = new Bot();
            bot.Init().GetAwaiter().GetResult();
        }
    }

}

