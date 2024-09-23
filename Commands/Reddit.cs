using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Reddit.Controllers.EventArgs;

namespace Discord_Bot.Commands;

public class Reddit : ApplicationCommandModule
{
    
    private readonly RedditAPi _reddit = new();
    
    
    
    [SlashCommand("redditPost", "Get a random post from a specified subreddit")]
    public async Task RandomRedditPost(InteractionContext ctx, 
        [Option("subreddit","The subreddit u want a post from")] string subreddit)
    {
      
        var post = _reddit.RetrieveRandomPostFromSubreddit(subreddit);

        if (post.URL.Contains("redgifs"))
        {
            await ctx.CreateResponseAsync(post.URL);
        }
        DiscordEmbedBuilder.EmbedFooter footer = new DiscordEmbedBuilder.EmbedFooter();
        footer.Text = "this is a footer";
        footer.IconUrl = "https://www.iconpacks.net/icons/2/free-reddit-logo-icon-2436-thumb.png";
        
        Console.WriteLine(post.URL);

        DiscordEmbedBuilder embed = new DiscordEmbedBuilder
        {
            Color = DiscordColor.Black,
            Title = post.Title,
            Author = new DiscordEmbedBuilder.EmbedAuthor{Name = post.Author},
            ImageUrl = post.URL,
            Footer = footer,
        };
            
        if (post.NSFW && !ctx.Channel.IsNSFW) { await ctx.CreateResponseAsync("This is not a NSFW channel!"); }
        else {await ctx.CreateResponseAsync(embed: embed); }

    }
    
    
    
    
}