using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Bot.RiotApiClasses
{
    public class SummonerInfo
    {
        public string AccountId { get; set; }
        public int ProfileIconId { get; set; }
        public long RevisionDate { get; set; }
        public string Name { get; set; }
        public string Id { get; set; }
        public string Puuid { get; set; }
        public long SummonerLevel { get; set; }

        public override string ToString()
        {
            return $"Summoner Name: {Name}\n" +
                   $"Summoner Level: {SummonerLevel}\n" +
                   $"Account ID: {AccountId}\n" +
                   $"Profile Icon ID: {ProfileIconId}\n" +
                   $"Revision Date: {DateTimeOffset.FromUnixTimeMilliseconds(RevisionDate)}\n" +
                   $"Summoner ID: {Id}\n" +
                   $"PUUID: {Puuid}";
        }
    }
}
