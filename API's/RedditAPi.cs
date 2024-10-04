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
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Reddit.Controllers.EventArgs;
using Subreddit = Reddit.Controllers.Subreddit;

namespace Discord_Bot
{
    public class RedditPost
    {
        public string Title { get; set; }
        public string URL { get; set; } 
        public string Subreddit { get; set; }
        public string Author { get; set; }
        
        public bool NSFW { get; set; }

        public override string ToString()
        {
            return $"Title: {Title}\nUrl: {URL}\nSubreddit: {Subreddit}\nAuthor: {Author}";
        }
    }
    public class RedditApi
    {
        private static HttpClient _httpClient = new();
        private static Random _rdm = new();
        private readonly string  _redditAccessToken;
    
    
        public RedditApi(string redditAccessToken)
        {
            _redditAccessToken = redditAccessToken;
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "DankDiscordBot/1.0");
        }
        
        public async Task<RedditPost> GetRandomPostFromSubreddit(string subreddit, string sortBy)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://oauth.reddit.com/r/{subreddit}/{sortBy}");
            request.Headers.Add("Authorization", $"Bearer {_redditAccessToken}");
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var posts = ParseRedditJson(responseBody);
                var randomNr = _rdm.Next(posts.Count);
                return posts[randomNr];
            }
            else
            {
                Console.WriteLine(response);
                return null;
            }

        }
        
        
        
        
    
        static List<RedditPost> ParseRedditJson(string responseBody)
        {
            List<RedditPost> posts = new List<RedditPost?>();
        
            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;
            var data = root.GetProperty("data");
            var children = data.GetProperty("children");

            Console.WriteLine("Top Posts:");

            foreach (var child in children.EnumerateArray())
            {
                RedditPost post = new RedditPost();

                var postData = child.GetProperty("data");
                post.Title = postData.GetProperty("title").GetString();
                post.URL = postData.GetProperty("url").GetString();
                post.Subreddit = postData.GetProperty("subreddit").GetString();
                post.Author = postData.GetProperty("author").GetString();
                post.NSFW = postData.GetProperty("over_18").GetBoolean();
                
                posts.Add(post);
            }

            return posts;
        }
        
    }
    

}
            