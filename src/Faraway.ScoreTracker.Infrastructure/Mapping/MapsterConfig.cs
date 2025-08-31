using Faraway.ScoreTracker.Contracts.Responses;
using Faraway.ScoreTracker.Core.Entities;
using Faraway.ScoreTracker.Infrastructure.Persistence.Models;
using Mapster;

namespace Faraway.ScoreTracker.Infrastructure.Mapping;

public static class MapsterConfig
{
    public static void RegisterMappings()
    {
        PlayerRecordToPlayer();

        TypeAdapterConfig<RegionRecord, Region>.NewConfig();
        TypeAdapterConfig<ShrineRecord, Shrine>.NewConfig();
        TypeAdapterConfig<ScoringRuleRecord, ScoringRule>.NewConfig();

        PlayerToPlayerRecord();

        TypeAdapterConfig<Region, RegionRecord>.NewConfig();
        TypeAdapterConfig<Shrine, ShrineRecord>.NewConfig();

        TypeAdapterConfig<GameRecord, GameResponse>
            .NewConfig()
            .Map(d => d.PlayTime, s => s.PlayTime)
            .Map(d => d.Players,  s => s.Players);

        PlayerRecordToPlayerResponse();

        TypeAdapterConfig<ScoringRuleRecord, ScoringRuleResponse>.NewConfig();
        
        TypeAdapterConfig<Game, GameResponse>
            .NewConfig()
            .Map(d => d.PlayTime, s => s.PlayTime)
            .Map(d => d.Players,  s => s.Players);

        PlayerToPlayerResponse();
    }

    private static void PlayerToPlayerResponse()
    {
        TypeAdapterConfig<Player, PlayerResponse>
            .NewConfig()
            .Map(d => d.Id,   s => s.Id)
            .Map(d => d.Name, s => s.Name)
            .Map(d => d.Regions, s => s.Regions
                .OrderBy(r => r.Position)
                .Select(r => new RegionResponse
                {
                    Number     = r.Region.Number,
                    Value      = r.Region.Value,
                    HasHint    = r.Region.HasHint,
                    Area       = r.Region.Area,
                    Wonders    = r.Region.Wonders,
                    Condition  = r.Region.Condition,
                    ScoringRule = r.Region.ScoringRule == null ? null : new ScoringRuleResponse
                    {
                        Type    = r.Region.ScoringRule.Type,
                        Points  = r.Region.ScoringRule.Points,
                        ColorOne = r.Region.ScoringRule.ColorOne,
                        ColorTwo = r.Region.ScoringRule.ColorTwo
                    },
                    IsFaceUp   = r.IsFaceUp,
                    Position   = r.Position
                }).ToList())
            .Map(d => d.Shrines, s => s.Shrines.Select(sh => new ShrineResponse
            {
                Id         = sh.Id,
                HasHint    = sh.HasHint,
                Wonders    = sh.Wonders,
                Area       = sh.Area,
                Value      = sh.Value,
                ScoringRule = sh.ScoringRule == null ? null : new ScoringRuleResponse
                {
                    Type    = sh.ScoringRule.Type,
                    Points  = sh.ScoringRule.Points,
                    ColorOne = sh.ScoringRule.ColorOne,
                    ColorTwo = sh.ScoringRule.ColorTwo
                }
            }).ToList());
    }

    private static void PlayerRecordToPlayerResponse()
    {
        TypeAdapterConfig<PlayerRecord, PlayerResponse>
            .NewConfig()
            .Map(d => d.Id,   s => s.Id)
            .Map(d => d.Name, s => s.Name)
            .Map(d => d.Regions, s => s.PlayerRegions
                .OrderBy(pr => pr.Position)
                .Select(pr => new RegionResponse
                {
                    Number    = pr.Region.Number,
                    Value     = pr.Region.Value,
                    HasHint   = pr.Region.HasHint,
                    Area      = pr.Region.Area,
                    Wonders   = pr.Region.Wonders,
                    Condition = pr.Region.Condition,
                    ScoringRule = pr.Region.ScoringRule == null ? null : new ScoringRuleResponse
                    {
                        Type    = pr.Region.ScoringRule.Type,
                        Points  = pr.Region.ScoringRule.Points,
                        ColorOne = pr.Region.ScoringRule.ColorOne,
                        ColorTwo = pr.Region.ScoringRule.ColorTwo,
                    },
                    IsFaceUp  = pr.IsFaceUp,
                    Position  = pr.Position,
                }).ToList())
            .Map(d => d.Shrines, s => s.Shrines.Select(sh => new ShrineResponse
            {
                Id         = sh.Id,
                HasHint    = sh.HasHint,
                Wonders    = sh.Wonders,
                Area       = sh.Area,
                Value      = sh.Value,
                ScoringRule = sh.ScoringRule == null ? null : new ScoringRuleResponse
                {
                    Type    = sh.ScoringRule.Type,
                    Points  = sh.ScoringRule.Points,
                    ColorOne = sh.ScoringRule.ColorOne,
                    ColorTwo = sh.ScoringRule.ColorTwo,
                }
            }).ToList());
    }

    private static void PlayerRecordToPlayer()
    {
        TypeAdapterConfig<PlayerRecord, Player>
            .NewConfig()
            .Ignore(d => d.Regions)
            .Ignore(d => d.Shrines)
            .AfterMapping((src, dest) =>
            {
                foreach (var pr in src.PlayerRegions.OrderBy(r => r.Position))
                {
                    var region = pr.Region.Adapt<Region>();
                    dest.AddRegionAppend(region, pr.IsFaceUp);
                }

                foreach (var s in src.Shrines)
                    dest.AddShrine(s.Adapt<Shrine>());
            });
    }

    private static void PlayerToPlayerRecord()
    {
        TypeAdapterConfig<Player, PlayerRecord>
            .NewConfig()
            .Map(d => d.Id, s => s.Id)
            .Map(d => d.Name, s => s.Name)
            .Map(d => d.PlayerRegions,
                s => s.Regions.Select(r => new PlayerRegionRecord
                {
                    Id       = r.Id == Guid.Empty ? Guid.NewGuid() : r.Id,
                    PlayerId = s.Id,
                    RegionId = r.Region.Id,
                    IsFaceUp = r.IsFaceUp,
                    Position = r.Position
                }).ToList())
            .Map(d => d.Shrines,
                s => s.Shrines.Select(sh => new ShrineRecord
                {
                    Id       = sh.Id == Guid.Empty ? Guid.NewGuid() : sh.Id,
                    PlayerId = s.Id,
                    Area     = sh.Area,
                    HasHint  = sh.HasHint,
                    Value    = sh.Value,
                    ScoringRule = sh.ScoringRule == null ? null : new ScoringRuleRecord
                    {
                        Id      = sh.ScoringRule.Id == Guid.Empty ? Guid.NewGuid() : sh.ScoringRule.Id,
                        Type    = sh.ScoringRule.Type,
                        Points  = sh.ScoringRule.Points,
                        ColorOne = sh.ScoringRule.ColorOne,
                        ColorTwo = sh.ScoringRule.ColorTwo,
                    },
                }).ToList());
    }
}
