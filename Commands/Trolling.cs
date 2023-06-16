using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace Discord_Bot.Commands
{
    public class Trolling : BaseCommandModule
    {

        [Command]
        public async Task trol(CommandContext ctx)
        {
            var channel = ctx.Client.GetChannelAsync(1115023907972460644).Result;
            try
            {
                await ctx.Client.SendMessageAsync(channel, "Gay");

            }
            catch (Exception e)
            {

                Console.WriteLine(e.ToString());
            }
            Console.WriteLine(channel.ToString());
        }
    }
}
