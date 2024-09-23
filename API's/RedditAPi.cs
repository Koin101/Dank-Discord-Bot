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
                             appSecret: Environment.GetEnvironmentVariable("RedditSecret"), refreshToken: Environment.GetEnvironmentVariable("RedditRefreshToken"));
        private readonly Random _rdm = new();
        private DiscordClient _discord;
        
        public static Dictionary<string, ulong> subredditsToMonitor = new();
        private static string _path = Path.Combine(Directory.GetCurrentDirectory(), "subreddits.txt");
        
        public RedditAPi(DiscordClient discord)
        {
            _discord = discord;
            ReadTxt(_path);
        }

        public RedditAPi()
        {
            
        }

        public LinkPost RetrieveRandomPostFromSubreddit(string subreddit)
        {
            var posts = Reddit.Subreddit(subreddit).Posts.Hot;

            var randomPost = posts[_rdm.Next(posts.Count)];
            
            return (LinkPost) randomPost;

        }
        

        public void StartToMonitor()
        {
            foreach (var subreddit in subredditsToMonitor.Keys)
            {
                Console.WriteLine("Starting to monitor: " + subreddit);
                Reddit.Subreddit(subreddit).Posts.GetNew();
                Reddit.Subreddit(subreddit).Posts.NewUpdated += C_NewPostsUpdated;
                
            }
            
        }
        
        private static void ReadTxt(string path)
        {
            try
            {
                using var reader = new StreamReader(path);
                while (reader.ReadLine() is { } line)
                {
                    var splits = line.Split(",");
                    subredditsToMonitor[splits[0]] = Convert.ToUInt64(splits[1]);
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine("Error reading the file: " + ex.Message);
            }
        
        }

        private void C_NewPostsUpdated(object sender, PostsUpdateEventArgs e)
        {
            Console.WriteLine("monitor triggered");
            foreach (var post in e.Added)
            {
                var subreddit = post.Subreddit;
                var channel = subredditsToMonitor[subreddit];
                var discordChannel = _discord.GetChannelAsync(channel).Result;
                var embed = new DiscordEmbedBuilder
                {
                    Title = post.Title,
                    Url = post.Permalink,
                    Color = DiscordColor.Black,
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        Name = post.Author
                    }
                };
                
                discordChannel.SendMessageAsync(embed: embed);
                
            }
        }
    }
}
            