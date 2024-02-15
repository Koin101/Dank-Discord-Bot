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

namespace Discord_Bot
{
    public class RedditAPi
    {
        RedditClient r = new RedditClient(appId: Environment.GetEnvironmentVariable("RedditID"),
                             appSecret: Environment.GetEnvironmentVariable("RedditSecret"), refreshToken: Environment.GetEnvironmentVariable("RedditRefreshToken"));
        Random rdm = new Random();



        public LinkPost RetrieveRandomPostFromSubreddit(string subreddit)
        {
            var posts = r.Subreddit(subreddit).Posts.Hot;

            var randomPost = posts[rdm.Next(posts.Count)];

            //while(randomPost.Listing.IsSelf) { randomPost = posts[rdm.Next(posts.Count)]; }

            return (LinkPost) randomPost;

        }
    }
}
            