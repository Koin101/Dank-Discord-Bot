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

        static void Main(string[] args)
        {
            RedditClient r = new RedditClient(appId: Environment.GetEnvironmentVariable("RedditID"),
                appSecret: Environment.GetEnvironmentVariable("RedditSecret"), refreshToken: Environment.GetEnvironmentVariable("RedditRefreshToken"));
            Console.WriteLine("Username: " + r.Account.Me.Name);
            Console.WriteLine("Cake Day: " + r.Account.Me.Created.ToString("D"));

            foreach (Reddit.Controllers.Post post in r.Subreddit("").Posts.Hot)
            {
                Console.WriteLine("Title: " + post.Title);

                // Both LinkPost and SelfPost derive from the Post class.  --Kris
                Console.WriteLine(post.Listing.IsSelf
                    ? "Body: " + ((SelfPost)post).SelfText
                    : "URL: " + ((LinkPost)post).URL);
            }

        }
    }
}
