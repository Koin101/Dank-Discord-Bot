using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Reddit.Controllers.EventArgs;

namespace Discord_Bot.Commands;

public class Reddit : BaseCommandModule
{
    
    private readonly RedditAPi _reddit = new RedditAPi();

    [SlashCommand("redditPost", "Get a random post from a specified subreddit")]
    public async Task RandomRedditPost(InteractionContext ctx, 
        [Option("subreddit","The subreddit u want a post from")] string subreddit)
    {
        await ctx.DeferAsync();
        var post = _reddit.RetrieveRandomPostFromSubreddit(subreddit);
        DiscordEmbedBuilder.EmbedFooter footer = new DiscordEmbedBuilder.EmbedFooter();
        footer.Text = "this is a footer";
        footer.IconUrl = "https://www.iconpacks.net/icons/2/free-reddit-logo-icon-2436-thumb.png";

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
    
    
    public static void C_NewPostsUpdated(object sender, PostsUpdateEventArgs e)
    {
        foreach (var post in e.Added)
        {
            Console.WriteLine("New Post by " + post.Author + ": " + post.Title);
        }
    }
}