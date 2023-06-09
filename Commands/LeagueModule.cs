using Discord;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using OpenAI.GPT3;
using OpenAI.GPT3.Managers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using RiotSharp.Endpoints.SummonerEndpoint;

namespace Discord_Bot.Commands
{
    public class LeagueModule : BaseCommandModule
    {



        public HttpClient leagueClient = new HttpClient();
        

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
            var summonerInfo = GetSummoner(summonerName);

            await (summonerInfo != null ?  
                ctx.RespondAsync($"Summoner Name: {summonerInfo.Name}\nSummoner Level: {summonerInfo.SummonerLevel}") 
                : 
                ctx.RespondAsync("Summoner not found. Did you spell it correctly?"));

        }

        public SummonerInfo GetSummoner(string username)
        {
            string summonerInfoEndpoint = $"https:/euw1.api.riotgames.com/summoner/v4/summoners/by-name/{username}";
            HttpResponseMessage response = leagueClient.GetAsync(summonerInfoEndpoint).Result;

            if (response.IsSuccessStatusCode)
            {
                // Parse the response and extract the summoner info
                var summonerInfo = response.Content.ReadAsAsync<SummonerInfo>().Result;

                return summonerInfo;

            }
            else
            {
                return null;
            }
        }

        public List<string> GetMatchIds(string puuid, long? startTime = null, long? endTime = null, int? queue = null, string? type = null, int? start = null, int? count = null)
        {

            string Endpoint = $"https:/euw1.api.riotgames.com/lol/match/v5/matches/by-puuid/{puuid}/ids";

            UriBuilder uriBuilder = new UriBuilder(Endpoint);
            uriBuilder.Query = 
                       $"{(startTime != null ? $"&startTime={Uri.EscapeDataString(startTime.ToString())}" : string.Empty)}" +
                       $"{(endTime != null ? $"&endTime={Uri.EscapeDataString(endTime.ToString())}" : string.Empty)}" +
                       $"{(queue != null ? $"queue={Uri.EscapeDataString(queue.ToString())}" : string.Empty)}" +
                       $"{(type != null ? $"type={Uri.EscapeDataString(type.ToString())}" : string.Empty)}" +
                       $"{(start != null ? $"start={Uri.EscapeDataString(start.ToString())}" : string.Empty)}" +
                       $"{(count != null ? $"count={Uri.EscapeDataString(count.ToString())}" : string.Empty)}";

            


            var response = leagueClient.GetAsync(uriBuilder.Uri).Result;

            if (response.IsSuccessStatusCode)
            {
                var matchids = response.Content.ReadAsAsync<MatchId>().Result;
                return matchids.MatchIds;
            }
            else
            {

                return null;
            }
        }

        public static long ConvertToEpoch(DateTime dateTime)
        {
            DateTimeOffset dateTimeOffset = new DateTimeOffset(dateTime);
            return dateTimeOffset.ToUnixTimeSeconds();
        }

        public void GetlastMatchData(string username)
        {
            string summonerInfoEndpoint = $"https:/euw1.api.riotgames.com/summoner/v4/summoners/by-name/{username}";

            var response = leagueClient.GetAsync(summonerInfoEndpoint).Result;

            DateTime dateNow = DateTime.Now;
            var dateBefore = dateNow.AddMinutes(-40);

            long startTime = ConvertToEpoch(dateBefore);

            if (response.IsSuccessStatusCode)
            {
                SummonerInfo summoner = response.Content.ReadAsAsync<SummonerInfo>().Result;

                var lastMatchIds = GetMatchIds(summoner.Puuid, startTime);


            }
            else
            {

            }

        }

    }

    public class SummonerInfo
    {
        public string AccountId { get; set; }
        public int ProfileIconId { get; set; }
        public long RevisionDate { get; set; }
        public string Name { get; set; }
        public string Id { get; set; }
        public string Puuid { get; set; }
        public long SummonerLevel { get; set; }
    }

    public class MatchId
    {
        public List<string> MatchIds { get; set; }
    }




}
