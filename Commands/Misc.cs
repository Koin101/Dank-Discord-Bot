
using Discord;
using Discord.Interactions;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using OpenAI.GPT3;
using OpenAI.GPT3.Managers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;



namespace Discord_Bot.Commands
{
    public class Misc : BaseCommandModule
    {
        OpenAI openAI = new OpenAI();
        RedditAPi reddit = new RedditAPi();


        [Command("chatGPT"), Description("this is chatGPT description"), Aliases( "chatgpt" )]
        /// <summary>
        /// Ask anything to the completion AI of OpenAI and it will respond
        /// <param name="prompt"/> The prompt which will be asked to the AI. Make sure to use "" around your prompt! </param>
        /// <param name="model"> Specify the model to be used. Default = davinci. Other options include: curie, babbage, ada. See OpenAI for more information on these models </param>
        /// <param name="temp"/> The temparature of the model, it determines the "creativity" of the model the higher the more creative. </param>
        /// 
        /// </summary>

        
        public async Task OpenAITextCall(CommandContext ctx, 
            [Description("The prompt which will be asked to the AI. Make sure to use \"\" around your prompt!")] string prompt,
            [Description("Specify the model to be used. Default = davinci. Other options include: curie, babbage, ada. See OpenAI for more information on these models")] string model="davinci", 
            [Description("The temparature of the model, it determines the \"creativity\" of the model the higher the more creative. Default=0.5")] string temp="0.5")
        {
            await ctx.TriggerTypingAsync();

            Task<string> result = openAI.Textrequest(prompt, model, temp);
            try
            {
                Console.WriteLine(result.Result);
                await ctx.RespondAsync(result.Result);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                if (e.HResult == -2146233088)
                    await ctx.RespondAsync("Too many requests (aka ya ran out of free requests)");
                else
                    await ctx.RespondAsync("An error occured");
            }

        }

        
        

        [Command("imageGen"), Description("Generate an image based off a prompt. Will fail if prompt is not within the terms of OpenAI")]
        public async Task OpenAiImageCall(CommandContext ctx,
            [Description("The prompt for which u want a generated image. Make sure to use \"\" around the prompt")]  string prompt)
        {
            await ctx.TriggerTypingAsync();
            Task<string> result = openAI.ImageRequest(prompt);
            if (result.Result == "fail" )
            { 
                await ctx.RespondAsync("Image generation failed, request is not within guidelines of OpenAI");
            }
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                Color = DiscordColor.Black,
                Description = prompt,
                ImageUrl = result.Result


            };
            await ctx.RespondAsync(embed:embed);
            
        }

        [Command("redditPost")]
        public async Task RandomRedditPost(CommandContext ctx, [Description("The subreddit u want a post from")] string subreddit)
        {
            await ctx.TriggerTypingAsync();
            DiscordMessage message;
            var post = reddit.RetrieveRandomPostFromSubreddit(subreddit);
            DiscordEmbedBuilder.EmbedFooter footer = new DiscordEmbedBuilder.EmbedFooter();
            footer.Text = "this is a footer";
            footer.IconUrl = "https://www.iconpacks.net/icons/2/free-reddit-logo-icon-2436-thumb.png";

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                Color = DiscordColor.Black,
                Title = post.Title,
                ImageUrl = post.URL,
                Footer = footer,

            };

            if (post.NSFW && !ctx.Channel.IsNSFW) { message = await ctx.RespondAsync("This is a non nsfw channel. Please ask for nsfw subreddits in an nsfw channel."); }
            else { message = await ctx.RespondAsync(embed: embed); }

            await message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":+1:"));

        }

       
    }

   

}
