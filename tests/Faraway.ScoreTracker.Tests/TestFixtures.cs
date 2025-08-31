using System.Linq;
using Faraway.ScoreTracker.Contracts.Requests;
using Faraway.ScoreTracker.Core.Enums;

namespace Faraway.ScoreTracker.Tests;

public static class TestFixtures
{
    public static GameRequest SimpleGame(string playerName = "Alice")
    {
        RegionRequest region = new RegionRequest(
            Number: 1,
            Value: TimeValue.Nacht,
            HasHint: true,
            Area: Color.Gelb,
            Wonders: [],
            Condition: [],
            ScoringRule: new ScoringRuleRequest(
                Type: ScoringType.PerColorSet,
                Points: 5,
                ColorOne: null,
                ColorTwo: null
            )
        );

        ShrineRequest shrine = new ShrineRequest(
            HasHint: false,
            Area: Color.Gelb,
            Value: TimeValue.Tag,
            Wonders: [],
            ScoringRule: new ScoringRuleRequest(
                Type: ScoringType.Flat,
                Points: 3,
                ColorOne: null,
                ColorTwo: null
            )
        );

        PlayerRequest player = new PlayerRequest(
            Name: playerName,
            Regions: [region,],
            Shrines: [shrine,]
        );

        return new GameRequest(
            PlayTime: null,
            Players: [player,]
        );
    }

    public static GameRequest GameWith8Regions(string playerName = "Max")
    {
        RegionRequest[] regions = Enumerable.Range(1, 8)
            .Select(i =>
                new RegionRequest(
                    Number: i,
                    Value: TimeValue.Tag,
                    HasHint: i % 2 == 0,
                    Area: Color.Gelb,
                    Wonders: [],
                    Condition: [],
                    ScoringRule: new ScoringRuleRequest(
                        Type: ScoringType.Flat,
                        Points: i,
                        ColorOne: null,
                        ColorTwo: null
                    )
                )
            )
            .ToArray();

        PlayerRequest player = new PlayerRequest(
            Name: playerName,
            Regions: regions,
            Shrines: []
        );

        return new GameRequest(
            PlayTime: null,
            Players: [player,]
        );
    }

    public static GameRequest TwoPlayers(string a = "Alice", string b = "Bob")
    {
        PlayerRequest p1 = new PlayerRequest(a, [], []);
        PlayerRequest p2 = new PlayerRequest(b, [], []);
        return new GameRequest(null, new[] { p1, p2 });
    }

    public static GameRequest NoPlayers()
    {
        return new GameRequest(null, []);
    }

    public static GameRequest SevenPlayers()
    {
        PlayerRequest[] players = Enumerable.Range(1, 7)
            .Select(i => new PlayerRequest($"P{i}", [], []))
            .ToArray();

        return new GameRequest(null, players);
    }
}