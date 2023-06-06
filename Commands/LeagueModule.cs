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
using RiotSharp;
using RiotSharp.Misc;
using System.Net.Http;

namespace Discord_Bot.Commands
{
    public class LeagueModule : BaseCommandModule
    {

        static async Task Main(string[] args)
        {
            using var client = new HttpClient();

            var result = await client.GetAsync("https://127.0.0.1:2999/liveclientdata/allgamedata");
            Console.WriteLine(result.StatusCode);
        }



        RiotApi api = RiotApi.GetDevelopmentInstance("RGAPI-f8465efc-67c0-44b1-984f-8389698d66c1");

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


        public async Task GetSummonerInfo(CommandContext ctx, string summonerName)
        {


            string apiKey = Environment.GetEnvironmentVariable("RiotApiKey"); 

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("X-Riot-Token", apiKey);

                // Make a request to get summoner info
                string summonerInfoEndpoint = $"https:/euw1.api.riotgames.com/summoner/v4/summoners/by-name/{summonerName}";
                HttpResponseMessage response = await client.GetAsync(summonerInfoEndpoint);

                if (response.IsSuccessStatusCode)
                {
                    // Parse the response and extract the summoner info
                    var summonerInfo = await response.Content.ReadAsAsync<SummonerInfo>();

                    // Process the summoner info as needed
                    // You can access properties of the SummonerInfo object like summonerInfo.Name, summonerInfo.Level, etc.

                    await ctx.RespondAsync($"Summoner Name: {summonerInfo.Name}\nSummoner Level: {summonerInfo.SummonerLevel}");
                }
                else
                {
                    await ctx.RespondAsync("Failed to retrieve summoner info from the Riot Games API.");
                }
            }
        }

        public void GetMatchIds()
        {

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


}
