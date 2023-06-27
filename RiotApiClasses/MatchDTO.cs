using RiotSharp.Endpoints.MatchEndpoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Bot.RiotApiClasses
{
    public class MatchDto
    {
        public MetadataDto Metadata { get; set; }
        public InfoDto Info { get; set; }
    }

    public class MetadataDto
    {
        public string dataversion { get; set; }
        public string matchID { get; set; }
        public List<string> participants { get; set; }
    }

    public class InfoDto
    {
        public long? gameCreation { get; set; }

        public long? gameDuration { get; set; }

        public long? gameEndTimestamp { get; set; }

        public long? gameId { get; set; }

        public string gameMode { get; set; }

        public string gameName { get; set; }

        public long? gameStartTimeStamp { get; set; }

        public string gameType { get; set; }

        public string gameVersion { get; set; }

        public int? mapId { get; set; }

        public List<ParticipantDto> participants { get; set; }
        
        public string platformID { get; set; }  

        public int? queueId { get; set; }

        public List<TeamDto> teams { get; set; }

        public string tournamentCode { get; set; }


    }

    public class ParticipantDto
    {
        public int? assists { get; set; }

        public int? baronKills { get; set; }

        public int? bountyLevel { get; set; }

        public int? champExperience { get; set; }

        public int? champLevel { get; set; }

        public int? championId { get; set; }

        public string championName { get; set; }

        public int? championTransform { get; set; }

        public int? consumablesPurchased { get; set; }   

        public int? damageDealtToBuildings { get; set; }

        public int? damageDealtToObjectives { get; set; }

        public int? damageDealtToTurrets { get; set; }
        public int? DamageSelfMitigated { get; set; }
        public int? Deaths { get; set; }
        public int? DetectorWardsPlaced { get; set; }
        public int? DoubleKills { get; set; }
        public int? DragonKills { get; set; }
        public bool FirstBloodAssist { get; set; }
        public bool FirstBloodKill { get; set; }
        public bool FirstTowerAssist { get; set; }
        public bool FirstTowerKill { get; set; }
        public bool GameEndedInEarlySurrender { get; set; }
        public bool GameEndedInSurrender { get; set; }
        public int? GoldEarned { get; set; }
        public int? GoldSpent { get; set; }
        public string individealPosition { get; set; }
        public int? InhibitorKills { get; set; }
        public int? InhibitorTakedowns { get; set; }
        public int? InhibitorsLost { get; set; }
        public int? Item0 { get; set; }
        public int? Item1 { get; set; }
        public int? Item2 { get; set; }
        public int? Item3 { get; set; }
        public int? Item4 { get; set; }
        public int? Item5 { get; set; }
        public int? Item6 { get; set; }
        public int? ItemsPurchased { get; set; }
        public int? KillingSprees { get; set; }
        public int? Kills { get; set; }
        public string Lane { get; set; }
        public int? LargestCriticalStrike { get; set; }
        public int? LargestKillingSpree { get; set; }
        public int? LargestMultiKill { get; set; }
        public int? longestTimeSpentLiving { get; set; }
        public int? MagicDamageDealt { get; set; }
        public int? MagicDamageDealtToChampions { get; set; }
        public int? MagicDamageTaken { get; set; }
        public int? NeutralMinionsKilled { get; set; }
        public int? NexusKills { get; set; }
        public int? NexusTakedowns { get; set; }
        public int? NexusLost { get; set; }
        public int? ObjectivesStolen { get; set; }
        public int? ObjectivesStolenAssists { get; set; }
        public int? ParticipantId { get; set; }
        public int? PentaKills { get; set; }
        public PerksDto Perks { get; set; }
        public int? PhysicalDamageDealt { get; set; }
        public int? PhysicalDamageDealtToChampions { get; set; }
        public int? PhysicalDamageTaken { get; set; }
        public int? ProfileIcon { get; set; }
        public string Puuid { get; set; }
        public int? QuadraKills { get; set; }
        public string RiotIdName { get; set; }
        public string RiotIdTagline { get; set; }
        public string Role { get; set; }
        public int? SightWardsBoughtInGame { get; set; }
        public int? Spell1Casts { get; set; }
        public int? Spell2Casts { get; set; }
        public int? Spell3Casts { get; set; }
        public int? Spell4Casts { get; set; }
        public int? Summoner1Casts { get; set; }
        public int? Summoner1Id { get; set; }
        public int? Summoner2Casts { get; set; }
        public int? Summoner2Id { get; set; }
        public string SummonerId { get; set; }
        public int? SummonerLevel { get; set; }
        public string SummonerName { get; set; }
        public bool TeamEarlySurrendered { get; set; }
        public int? TeamId { get; set; }
        public string teamPosition { get; set; }
        public int? TimeCCingOthers { get; set; }
        public int? TimePlayed { get; set; }
        public int? TotalDamageDealt { get; set; }
        public int? TotalDamageDealtToChampions { get; set; }
        public int? TotalDamageShieldedOnTeammates { get; set; }
        public int? TotalDamageTaken { get; set; }
        public int? TotalHeal { get; set; }
        public int? TotalHealsOnTeammates { get; set; }
        public int? TotalMinionsKilled { get; set; }
        public int? TotalTimeCCDealt { get; set; }
        public int? TotalTimeSpentDead { get; set; }
        public int? TotalUnitsHealed { get; set; }
        public int? TripleKills { get; set; }
        public int? TrueDamageDealt { get; set; }
        public int? TrueDamageDealtToChampions { get; set; }
        public int? TrueDamageTaken { get; set; }
        public int? TurretKills { get; set; }
        public int? TurretTakedowns { get; set; }
        public int? TurretsLost { get; set; }
        public int? UnrealKills { get; set; }
        public int? VisionScore { get; set; }
        public int? VisionWardsBoughtInGame { get; set; }
        public int? WardsKilled { get; set; }
        public int? WardsPlaced { get; set; }
        public bool Win { get; set; }
    }

    public class PerksDto
    {
        public PerkStatsDto statPerks { get; set; }

        public List<PerkStyleDto> styles { get; set; }
    }

    public class PerkStatsDto
    {
        public int? defense { get; set; }
        public int? flex { get; set; }
        public int? offense { get; set; }
    }

    public class PerkStyleDto
    {
        public string description { get; set; }
        public List<PerkStyleSelectionDto> selections { get; set; }
        public int? style { get; set; }

    }

    public class PerkStyleSelectionDto
    {
        public int? perk { get; set; }
        public int? var1 { get; set; }
        public int? var2 { get; set; }
        public int? var3 { get; set; }
    }

    public class TeamDto
    {
        public List<BanDto> bans { get; set; }

        public ObjectivesDto objectives { get; set; }

        public int? teamId { get; set; }

        public bool win { get; set; }

    }

    public class BanDto
    {
        public int? championId { get; set;}

        public int? pickTurn { get; set; }
    }

    public class ObjectivesDto
    {
        public ObjectiveDto baron { get; set; }
        
        public ObjectiveDto champion { get; set; }

        public ObjectiveDto dragon { get; set; }

        public ObjectiveDto inhibitor { get; set; }

        public ObjectiveDto riftherald { get; set; }  
        
        public ObjectiveDto tower { get; set; }

    }

    public class ObjectiveDto
    {
        public bool first { get; set; }

        public int? kills { get; set; }
    }



}
