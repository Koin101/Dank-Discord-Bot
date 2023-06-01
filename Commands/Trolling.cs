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


namespace Discord_Bot.Commands
{
    public class Trolling : BaseCommandModule
    {

        [Command]
        public async Task RotOp(CommandContext ctx)
        {
            var user = ctx.User;
            var channel = ctx.Member.VoiceState.Channel;
            if (ctx.Member.VoiceState.Channel != null)
            {
                
            }
        }
    }
}
