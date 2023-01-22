
using Discord;
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
    public class BasicModule : BaseCommandModule
    {
        OpenAI openAI = new OpenAI();
        RedditAPi reddit = new RedditAPi();


        [Command("chatGPT"), Description("this is chatGPT description")]
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
            Console.WriteLine(result.Result);
            await ctx.RespondAsync(result.Result);
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

        //[Command("redditPost")]
        //public async Task RandomRedditPost(CommandContext ctx, [Description("The subreddit u want a post from")] string subreddit)
        //{
        //    await ctx.TriggerTypingAsync();
        //    DiscordMessage message;
        //    var post = reddit.RetrieveRandomPostFromSubreddit(subreddit);
        //    DiscordEmbedBuilder.EmbedFooter footer = new DiscordEmbedBuilder.EmbedFooter();
        //    footer.Text = post.URL.ToString();
        //    footer.IconUrl = "https://www.iconpacks.net/icons/2/free-reddit-logo-icon-2436-thumb.png";

        //    DiscordEmbedBuilder embed = new DiscordEmbedBuilder
        //    {
        //        Color = DiscordColor.Black,
        //        Title = post.Title,
        //        ImageUrl = post.URL,
        //        Footer = footer,

        //    };

        //    if(post.NSFW && !ctx.Channel.IsNSFW) { message = await ctx.RespondAsync("This is a non nsfw channel. Please ask for nsfw subreddits in an nsfw channel."); }
        //    else { message = await ctx.RespondAsync(embed:embed);}

        //    await message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":+1:"));

        //}

        [Command("5Stack"), Description("This command will ping everyone with the @league tag who isn't already in the voice channel.")]
        public async Task ping5Stack(CommandContext ctx)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync("You are not in a voiceChannel");
                return;
            }
            DiscordRole LeagueRole = ctx.Guild.GetRole(828775478231040031);
            var LeagueUsers = ctx.Guild.Members.Where(user => user.Value.Roles.Contains(LeagueRole));
            var LeagueUsersDict = LeagueUsers.ToDictionary(i => i.Key, i => i.Value);
            var ChannelUsers = ctx.Member.VoiceState.Channel.Users;
            
            StringBuilder stringBuilder = new StringBuilder();
           
            stringBuilder.Append("To those who do not touchgrass! \n");
            int i = 1;
            var downArrow = DiscordEmoji.FromName(ctx.Client, ":arrow_down:");
            foreach (var channelUser in ChannelUsers) LeagueUsersDict.Remove(channelUser.Id);            

            foreach (var user in LeagueUsersDict.Values)
            {
                
                stringBuilder.Append("- ");
                stringBuilder.Append(user.Mention);
                stringBuilder.Append("\n");
         
            }
            for (int j = 0; j < 10; j++)
            {
                stringBuilder.Append(downArrow + "\t\t");
            }
            stringBuilder.Append("\n");
            foreach (var user in ChannelUsers) 
            {

                stringBuilder.Append("- ");
                stringBuilder.Append(user.Mention);
                stringBuilder.Append("\n");
            }

            stringBuilder.Append("Really want to play League with a 5 stack. Join them or ur gay");

            await ctx.RespondAsync(stringBuilder.ToString());

        }
    }

   

}
