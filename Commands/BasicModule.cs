
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using OpenAI.GPT3;
using OpenAI.GPT3.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;



namespace Discord_Bot.Commands
{
    public class BasicModule : BaseCommandModule
    {
        OpenAI openAI = new OpenAI();


        [Command("chatGPT")]
        public async Task OpenAITextCall(CommandContext ctx, string prompt)
        {
            Task<string> result = openAI.Textrequest(prompt);
            Console.WriteLine(result.Result);
            await ctx.RespondAsync(result.Result);
        }

        [Command("imageGen")]
        public async Task OpenAiImageCall(CommandContext ctx, string prompt)
        {
            await ctx.TriggerTypingAsync();
            Task<string> result = openAI.ImageRequest(prompt);
            if (result.Result == "fail" )
            { 
                await ctx.RespondAsync("Image generation failed, try another prompt");
                await ctx.TriggerTypingAsync();
            }
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                Color = DiscordColor.Black,
                Description = prompt,
                ImageUrl = result.Result

            };
            await ctx.RespondAsync(embed:embed);
            
        }


    }

   

}
