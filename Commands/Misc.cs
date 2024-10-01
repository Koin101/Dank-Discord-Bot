using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;
using DSharpPlus.SlashCommands;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;


namespace Discord_Bot.Commands
{
    public class Misc : BaseCommandModule
    {
        private readonly GifCreator _gifCreator = new GifCreator();
        private readonly HttpClient _client = new HttpClient();
        

        [Command("creategif")]
        public async Task CreateGif(CommandContext ctx)
        {
            try
            {
                var attachments = ctx.Message.Attachments;
                if (attachments.Count == 0) await ctx.RespondAsync("Please attach an image (You can just copy paste the image)");

                for (var i = 0; i < attachments.Count; i++)
                {
                    Stream stream = await _client.GetStreamAsync(attachments[i].Url);
                    Image<Rgba32> image = SixLabors.ImageSharp.Image.Load<Rgba32>(stream);  

                    _gifCreator.CreateGifFromImg(image);

                    DiscordMessageBuilder messagefile = new DiscordMessageBuilder();
                    messagefile.AddFile("maxGay.gif", _gifCreator.memStream, true);

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
                Stream stream = await _client.GetStreamAsync(imageUrl + ".gif");
                Image<Rgba32> gif = SixLabors.ImageSharp.Image.Load<Rgba32>(stream);

                stream.Dispose();
                _gifCreator.AnimatedGifCreator(gif.Frames, gif);

                DiscordMessageBuilder messagefile = new DiscordMessageBuilder();

                messagefile.AddFile("maxGay.gif", _gifCreator.gifStream, true);

                await ctx.RespondAsync(messagefile);
                _gifCreator.gifStream.Dispose();
                
            }
            catch (HttpRequestException error) 
            {
                Console.WriteLine("\n\n\n\n\n\n-------------------------");
                Console.WriteLine(error.ToString());
                Console.WriteLine(error.StackTrace);
                Console.WriteLine("\n\n\n\n\n-------------------------");
                await ctx.RespondAsync("I got an HttpRequestException. \nI might not have permission to access the gif link.");
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
    }
}