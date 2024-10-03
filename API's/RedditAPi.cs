using Reddit;
using Reddit.Controllers;
using Reddit.Inputs;
using Reddit.Inputs.LinksAndComments;
using Reddit.Inputs.Subreddits;
using Reddit.Inputs.Users;
using Reddit.Things;
using System;
using System.Collections.Generic;
using System.IO;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Reddit.Controllers.EventArgs;
using Subreddit = Reddit.Controllers.Subreddit;

namespace Discord_Bot
{
    public class RedditAPi
    {
        private static readonly RedditClient Reddit = new RedditClient(appId: Environment.GetEnvironmentVariable("RedditID"),
                             appSecret: Environment.GetEnvironmentVariable("RedditSecret"), refreshToken: Environment.GetEnvironmentVariable("RedditRefreshToken"), userAgent: "Discord Bot",
                             accessToken: Environment.GetEnvironmentVariable("RedditAccessToken"));
        private readonly Random _rdm = new();
        
        public static Dictionary<string, ulong> subredditsToMonitor = new();
        private static string _path = Path.Combine(Directory.GetCurrentDirectory(), "subreddits.txt");
        

        public LinkPost RetrieveRandomPostFromSubreddit(string subreddit)
        {
            var search = Reddit.SearchSubredditNames(subreddit, exact:true);
            if (search[0].SubscriberCount <= 1000)
            {
                return null;
            }
            var sub = Reddit.Subreddit(search[0].Name);
            var url = sub.URL;
            var posts = sub.Posts.Hot;

            var randomPost = posts?[_rdm.Next(posts.Count)];
        
            return (LinkPost)randomPost;
            
        }
        
        public LinkPost RetrieveNewestPostFromSubreddit(string subreddit)
        {
            var posts = Reddit.Subreddit(subreddit).Posts.New;

            var newestPost = posts[0];
            
            return (LinkPost) newestPost;

        }
        
    }
}
            