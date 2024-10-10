
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;


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
        public static string  redditAccessToken;
        private static string _refreshToken;
        private readonly string _authString;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private static DateTime? _tokenExpiration = null;
    
        public RedditApi(string refreshToken,string clientId, string clientSecret)
        {
            _refreshToken = refreshToken;
            _clientId = clientId;
            _clientSecret = clientSecret;
            _authString = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_clientId}:{_clientSecret}"));
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "DankDiscordBot/1.0");
        }
        
        public async Task<RedditPost> GetRandomPostFromSubreddit(string subreddit, string sortBy)
        {
            if (_tokenExpiration == null || _tokenExpiration < DateTime.Now)
            {
                var result = await RefreshAccessToken();
                if(result is null) throw new Exception("No access token");
                redditAccessToken = ParseAccessTokenJson(await result.Content.ReadAsStringAsync());
                _tokenExpiration = DateTime.Now.AddHours(20);
            }
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://oauth.reddit.com/r/{subreddit}/{sortBy}");
            request.Headers.Add("Authorization", $"Bearer {redditAccessToken}");
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
        
        public async Task<HttpResponseMessage?> RefreshAccessToken()
        {
            
            var request = new HttpRequestMessage(HttpMethod.Post, "https://www.reddit.com/api/v1/access_token");
            request.Headers.Add("Authorization", $"Basic {_authString}");
        
            var collection = new List<KeyValuePair<string, string>>();
        
            collection.Add(new("grant_type", "refresh_token"));
            collection.Add(new("refresh_token", $"{_refreshToken}"));
        
            var content = new FormUrlEncodedContent(collection);
        
            request.Content = content;


            var response = await _httpClient.SendAsync(request);
            return response;

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
        
        static string? ParseAccessTokenJson(string responseBody)
        {
            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;
            var accessToken = root.GetProperty("access_token").GetString();
            return accessToken;
        }
        
    }
    

}
            