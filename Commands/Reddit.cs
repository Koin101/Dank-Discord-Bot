using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Reddit.Controllers;
using Reddit.Controllers.EventArgs;

namespace Discord_Bot.Commands;

public class Reddit : ApplicationCommandModule
{
    
    private readonly RedditApi _reddit = new(Environment.GetEnvironmentVariable("RedditRefreshToken"),
        Environment.GetEnvironmentVariable("RedditID"), Environment.GetEnvironmentVariable("RedditSecret"));
    
    
    
    [SlashCommand("redditPost", "Get a random post from a specified subreddit")]
    public async Task RandomRedditPost(InteractionContext ctx, 
        [Option("subreddit","The subreddit u want a post from")] string subreddit,
        [Option("SortBy","The sorting of the subreddit")] SortBy sortBy = SortBy.Hot)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        var post = await _reddit.GetRandomPostFromSubreddit(subreddit, GetSortBy(sortBy));
        
        if(post is null) await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("No posts found in this subreddit, maybe it does not exist."));
        var msgBuilder = new DiscordWebhookBuilder();
        
        DiscordEmbedBuilder.EmbedFooter footer = new DiscordEmbedBuilder.EmbedFooter();
        footer.Text = "If image doesn't load, click resend button";
        footer.IconUrl = "https://www.iconpacks.net/icons/2/free-reddit-logo-icon-2436-thumb.png";
        
        var url = post.URL.Contains("reddit.com") ? post.URL.Replace("e", "x") : post.URL;

        DiscordEmbedBuilder embed = new DiscordEmbedBuilder
        {
            Color = DiscordColor.Black,
            Title = "r/" + post.Subreddit + " " + post.Title ,
            Author = new DiscordEmbedBuilder.EmbedAuthor{Name = post.Author + "---" + "Sorted by: " + sortBy},
            ImageUrl = post.URL,
            Footer = footer,
        };

        msgBuilder.AddEmbed(embed);
        msgBuilder.AddComponents(new DiscordLinkButtonComponent(url, "Open in Reddit"),
            new DiscordButtonComponent(ButtonStyle.Primary, "link_resend", "Resend Link"));


        if (post.NSFW && !ctx.Channel.IsNSFW)
        {
            await ctx.CreateResponseAsync("This is not a NSFW channel!");
        }
        else
        {
            await ctx.EditResponseAsync(msgBuilder);
        }
        
    }
    
    
    

    private string GetSortBy(SortBy sortBy)
    {
        return sortBy switch
        {
            SortBy.Hot => "hot",
            SortBy.New => "new",
            SortBy.Top => "top",
            SortBy.Rising => "rising",
            _ => throw new ArgumentOutOfRangeException(nameof(sortBy), sortBy, null)
        };
    }
    
    
}



public enum SortBy
{
    Hot,
    New,
    Top,
    Rising
}