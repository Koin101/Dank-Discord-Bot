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
using Subreddit = Reddit.Controllers.Subreddit;

namespace Discord_Bot
{
    public class RedditAPi
    {
        private static readonly RedditClient Reddit = new RedditClient(appId: Environment.GetEnvironmentVariable("RedditID"),
                             appSecret: Environment.GetEnvironmentVariable("RedditSecret"), refreshToken: Environment.GetEnvironmentVariable("RedditRefreshToken"));
        private readonly Random _rdm = new Random();

        public LinkPost RetrieveRandomPostFromSubreddit(string subreddit)
        {
            var posts = Reddit.Subreddit(subreddit).Posts.Hot;

            var randomPost = posts[_rdm.Next(posts.Count)];
            
            return (LinkPost) randomPost;

        }
        
        private Subreddit _eyebleach = Reddit.Subreddit("eyebleach"); 
        
        
        
    }
}
            