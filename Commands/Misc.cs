﻿
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
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Diagnostics.Tracing;
using SixLabors.ImageSharp.Formats;

namespace Discord_Bot.Commands
{
    public class Misc : BaseCommandModule
    {
        OpenAI openAI = new OpenAI();
        RedditAPi reddit = new RedditAPi();
        GifCreator gifCreator = new GifCreator();
        HttpClient client = new HttpClient();
        StableDiffusionApi StableDiffusionApi = new StableDiffusionApi();

        [Command("chatGPT"), Description("this is chatGPT description"), Obsolete("no longer in use")]
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


        [Command("creategif")]
        public async Task CreateGif(CommandContext ctx)
        {
            try
            {

                var attachments = ctx.Message.Attachments;

                if (attachments.Count == 0) await ctx.RespondAsync("Please attach an image (You can just copy paste the image)");

                for (int i = 0; i < attachments.Count; i++)
                {
                    Stream stream = await client.GetStreamAsync(attachments[i].Url);


                    Image<Rgba32> image = SixLabors.ImageSharp.Image.Load<Rgba32>(stream);  


                    gifCreator.CreateGifFromImg(image);

                    DiscordMessageBuilder messagefile = new DiscordMessageBuilder();

                    messagefile.AddFile("maxGay.gif", gifCreator.memStream, true);

                   await ctx.RespondAsync(messagefile);
                }
                


            }
            catch (Exception e)
            {   
                Console.WriteLine("\n\n\n\n\n\n\n------------------------------");
                Console.WriteLine(e.ToString());
                Console.WriteLine("----------------------------");
                Console.WriteLine("\n\n\n\n\n");
                await ctx.RespondAsync("I got an error oopsie");
            }



        }

        [Command("creategif")]
        public async Task CreateGif(CommandContext ctx, string imageUrl)
        {
            try
            {   

                Stream stream = await client.GetStreamAsync(imageUrl + ".gif");
                Image<Rgba32> gif = SixLabors.ImageSharp.Image.Load<Rgba32>(stream);

                stream.Dispose();
                gifCreator.AnimatedGifCreator(gif.Frames, gif);

                DiscordMessageBuilder messagefile = new DiscordMessageBuilder();

                messagefile.AddFile("maxGay.gif", gifCreator.gifStream, true);

                await ctx.RespondAsync(messagefile);
                gifCreator.gifStream.Dispose();
                
            }
            catch (HttpRequestException error) 
            {
                Console.WriteLine("\n\n\n\n\n\n-------------------------");
                Console.WriteLine(error.ToString());
                Console.WriteLine(error.StackTrace);
                Console.WriteLine("\n\n\n\n\n-------------------------");
                await ctx.RespondAsync("I got an HttpRequestException. \nI might not have permission to acces the gif link.");
            }

            catch (Exception e)
            {


                Console.WriteLine("\n\n\n\n\n\n\n------------------------------");
                Console.WriteLine(e.ToString());
                Console.WriteLine(e.StackTrace);
                Console.WriteLine("----------------------------");
                Console.WriteLine("\n\n\n\n\n");
                await ctx.RespondAsync("I got an error oopsie, blame Koen");
            }
        }

        [Command("txt2img")]
        public async Task Txt2Img(CommandContext ctx, string prompt, string negativePrompt="", 
            int width=512, int height=512, int seed= -1, string samplerName = "DPM++ 2M SDE")
        {
            Payload payload = new Payload(prompt, negativePrompt, width, height, seed, samplerName);

            var imageStream = StableDiffusionApi.txt2imgRequest(payload);
            if(imageStream == null) { await ctx.RespondAsync("The StableDiffusion API is not online. If u want to use it ping " +  ctx.Guild.GetMemberAsync(222675750778699776).Result.Nickname); return; }
            DiscordMessageBuilder messagefile = new DiscordMessageBuilder();

            messagefile.AddFile("maxGay.gif", imageStream, true);

            await ctx.RespondAsync(messagefile);
            
            imageStream.Dispose();
        }

    }


}
