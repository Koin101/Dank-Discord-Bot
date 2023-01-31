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


        [Command("SummonorInfo")]
        public async Task BasicSummonorInfo(CommandContext ctx, string summonorName)
        {
            try
            {
                var summonor = api.Summoner.GetSummonerByNameAsync(Region.Euw, summonorName).Result;
                var name = summonor.Name;
                var level = summonor.Level;
                var accountID = summonor.AccountId;

                await ctx.RespondAsync($"{name} is level {level} and has no life.");
            }
            catch (Exception e)
            {

                Console.WriteLine(e);
                await ctx.RespondAsync("Summonor not found!");
            }
        }
    }
}
