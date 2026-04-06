using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;
using Newtonsoft.Json;
using Discord_Bot.RiotApiClasses;
using RiotSharp.Endpoints.TeamEndpoint;
using Microsoft.VisualBasic;
using KGySoft.CoreLibraries;

namespace Discord_Bot.Commands
{
    public class LeagueModule : BaseCommandModule
    {



        public static HttpClient leagueClient = new HttpClient();

        public LeagueModule()
        {
            string apiKey = Environment.GetEnvironmentVariable("RiotApiKey");
            if (!leagueClient.DefaultRequestHeaders.TryGetValues("X-Riot-Token", out _))
            {
                leagueClient.DefaultRequestHeaders.Add("X-Riot-Token", apiKey);
                leagueClient.DefaultRequestHeaders.Add("Origin", "https://developer.riotgames.com");
            }

        }

        static void Main(string[] args)
        {
            var root = Directory.GetCurrentDirectory();
            var dotenv = Path.Combine(root, "Data/.env");
            DotEnv.Load(dotenv);
            LeagueModule leagueModule = new LeagueModule();
            
            var summoner = leagueModule.GetSummoner("neoblasterzzz").Result;


            Console.WriteLine(summoner.ToString());

            //var matchids = leagueModule.GetMatchIds(summoner.Puuid).Result;
            //leagueModule.GetMatchData(matchids.MatchIds[0]);

            //leagueModule.GetlastMatchData("neoblasterzzz");

        }


        [Command("5Stack"), Description("This command will ping everyone with the @league tag who isn't already in the voice channel.")]
        public async Task ping5Stack(CommandContext ctx)
        {
            
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync("You are not in a voiceChannel");
                return;
            }
            DiscordRole LeagueRole = ctx.Guild.GetRole(828775478231040031);
            var LeagueUsers = ctx.Guild.Members.Where(user => user.Value.Roles.Contains(LeagueRole));
            var LeagueUsersDict = LeagueUsers.ToDictionary(i => i.Key, i => i.Value);
            var ChannelUsers = ctx.Member.VoiceState.Channel.Users;

            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("To those who do not touch grass! \n");
            var downArrow = DiscordEmoji.FromName(ctx.Client, ":arrow_down:");
            foreach (var channelUser in ChannelUsers) LeagueUsersDict.Remove(channelUser.Id);

            foreach (var user in LeagueUsersDict.Values)
            {

                stringBuilder.Append("- ");
                stringBuilder.Append(user.Mention);
                stringBuilder.Append("\n");

            }
            for (int j = 0; j < 10; j++)
            {
                stringBuilder.Append(downArrow + "\t\t");
            }
            stringBuilder.Append("\n");
            foreach (var user in ChannelUsers)
            {

                stringBuilder.Append("- ");
                stringBuilder.Append(user.Nickname);
                stringBuilder.Append("\n");
            }

            stringBuilder.Append("Really want to play League with a 5 stack. Join them or ur gay");

            await ctx.RespondAsync(stringBuilder.ToString());

        }


        //TODO Implement league api commands

        [Command("summonerInfo")]
        public async Task SummonerInfo(CommandContext ctx, string summonerName)
        {
            var summonerInfo = GetSummoner(summonerName).Result;

            await (summonerInfo != null ?  
                ctx.RespondAsync($"Summoner Name: {summonerInfo.Name}\nSummoner Level: {summonerInfo.SummonerLevel}") 
                : 
                ctx.RespondAsync("Summoner not found. Did you spell it correctly?"));

        }

        public async Task<SummonerInfo> GetSummoner(string username)
        {
            string summonerInfoEndpoint = $"https://euw1.api.riotgames.com/lol/summoner/v4/summoners/by-name/{username}";

            //string apiKey = Environment.GetEnvironmentVariable("RiotApiKey");
            //leagueClient.DefaultRequestHeaders.Add("X-Riot-Token", apiKey);
            //leagueClient.DefaultRequestHeaders.Add("Origin", "https://developer.riotgames.com");
            HttpResponseMessage response = await leagueClient.GetAsync(summonerInfoEndpoint);

            if (response.IsSuccessStatusCode)
            {
                // Parse the response and extract the summoner info
                // var summonerInfo = await response.Content.ReadAsAsync<SummonerInfo>();
                // return summonerInfo;
                return null;
            }
            else
            {
                return null;
            }
        }


        public async  Task<MatchId> GetMatchIds(string puuid, long? startTime = null, long? endTime = null, int? queue = null, string type = null, int? start = null, int? count = null)
        {

            string Endpoint = $"https://europe.api.riotgames.com/lol/match/v5/matches/by-puuid/{puuid}/ids";
           
            UriBuilder uriBuilder = new UriBuilder(Endpoint);
            uriBuilder.Query = 
                       $"{(startTime != null ? $"&startTime={Uri.EscapeDataString(startTime.ToString())}" : string.Empty)}" +
                       $"{(endTime != null ? $"&endTime={Uri.EscapeDataString(endTime.ToString())}" : string.Empty)}" +
                       $"{(queue != null ? $"queue={Uri.EscapeDataString(queue.ToString())}" : string.Empty)}" +
                       $"{(type != null ? $"type={Uri.EscapeDataString(type.ToString())}" : string.Empty)}" +
                       $"{(start != null ? $"start={Uri.EscapeDataString(start.ToString())}" : string.Empty)}" +
                       $"{(count != null ? $"count={Uri.EscapeDataString(count.ToString())}" : string.Empty)}";

            


            var response = await leagueClient.GetAsync(uriBuilder.Uri);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var matchList = JsonConvert.DeserializeObject<List<string>>(json);
                var matchids = new MatchId(){MatchIds = matchList};
                return matchids;
            }
            else
            {
                Console.WriteLine("-----------------------------");
                Console.WriteLine(response.StatusCode);
                return null;
            }
        }

        public static long ConvertToEpoch(DateTime dateTime)
        {
            DateTimeOffset dateTimeOffset = new DateTimeOffset(dateTime);
            return dateTimeOffset.ToUnixTimeSeconds();
        }

        public MatchId GetlastMatchId(string username)
        {
            
            DateTime dateNow = DateTime.Now;
            var dateBefore = dateNow.AddMinutes(-60);

            long startTime = ConvertToEpoch(dateBefore);

 
            SummonerInfo summoner = GetSummoner(username).Result;

            var lastMatchIds = GetMatchIds(summoner.Puuid, startTime).Result;
            
            //Console.WriteLine(lastMatchIds);        
            
            return lastMatchIds;
        }

        public async Task<MatchDto> GetMatchData(string matchId)
        {
            string endpoint = $"https://europe.api.riotgames.com/lol/match/v5/matches/{matchId}";


            var response = leagueClient.GetAsync(endpoint).Result;

            if(response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<MatchDto>(json);

                
            }
            else
            {
                Console.WriteLine("--------------------");
                Console.WriteLine(response.StatusCode);
                return null;
            }
        }

        public DiscordEmbed CreateLeagueEmbed(MatchDto match, string username)
        {
            InfoDto info = match.Info;
            string surrender, outcome;
            ParticipantDto participant = info.participants.Find(x => x.SummonerName.ToLower() == username.ToLower());

            (long matchMinutes, long matchSeconds) = GetMinutesAndSeconds((long) info.gameDuration);
            (long DeathMinutes, long DeathSeconds) = GetMinutesAndSeconds((long) participant.TotalTimeSpentDead);
            if (participant.Win) outcome = "won";
            else outcome = "lost";
            var gameMode = info.gameMode;
            string position = " in " + participant.teamPosition;
            if (gameMode == "ARAM") position = "";
            if (participant.GameEndedInEarlySurrender) surrender = $"Game ended with an early surrender" + "\n";
            else if (participant.GameEndedInSurrender) surrender = $"Game ended with a surrender" + "\n";
            else surrender = "";

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append($"Bro was playing {participant.championName} {position} \n");
            stringBuilder.Append($"The match took {matchMinutes}:{matchSeconds}. \n");
            stringBuilder.Append(surrender);
            stringBuilder.Append($"This guy spent {DeathMinutes}:{DeathSeconds} dead which is {((double) participant.TotalTimeSpentDead / (double) info.gameDuration * 100).ToString("0.00")}% of the total game time");



            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                Color = DiscordColor.Gold,
                Title = $"Guys, {username} {outcome} his last {gameMode} match! ",
                Description = stringBuilder.ToString(),
            };
            embed.AddField("Kills", participant.Kills.ToString(), true);
            embed.AddField("Deaths", participant.Deaths.ToString(), true);
            embed.AddField("Assists", participant.assists.ToString(), true);

            embed.AddField("CS", participant.TotalMinionsKilled.ToString(), true);
            embed.AddField("Total damage dealt to champions", participant.TotalDamageDealtToChampions.ToString(), true);
            embed.AddField("Gold earned", participant.GoldEarned.ToString(), true);

            return embed;
        }

        [Command("lm")]
        public async Task GetLastMatchFromSummoner(CommandContext ctx, string username)
        {
            MatchId matchId = GetlastMatchId(username);

            if(matchId.MatchIds.Count > 0)
            {
                string lastMatchId = matchId.MatchIds[0];

                MatchDto matchData = GetMatchData(lastMatchId).Result;

                await ctx.RespondAsync(CreateLeagueEmbed(matchData, username));
            }
            else
            {
                await ctx.RespondAsync("No match in the last hour.");
            }

        }

        public DiscordEmbed GetLastMatchFromSummonorTimerEvent(string username)
        {
            MatchId matchId = GetlastMatchId(username);

            if (matchId.MatchIds.Count > 0)
            {
                string lastMatchId = matchId.MatchIds[0];

                MatchDto matchData = GetMatchData(lastMatchId).Result;

                return CreateLeagueEmbed(matchData, username);
            }
            else
            {
                return null;
            }
        }

        public static (long minutes, long seconds) GetMinutesAndSeconds(long totalSeconds)
        {
            long minutes = totalSeconds / 60;
            long seconds = totalSeconds % 60;

            return (minutes, seconds);
        }

    }

   

    




}
