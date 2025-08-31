using System;
using System.Linq;
using Faraway.ScoreTracker.Core.Entities;
using Faraway.ScoreTracker.Core.Enums;
using Xunit;

namespace Faraway.ScoreTracker.Tests;

public class ScoringRulesTests
{
    private static Player CreatePlayer() => new() { Id = Guid.NewGuid(), Name = "Tester" };

    private static Region CreateRegion(Color color, NatureWonder[] wonders, TimeValue timeValue = TimeValue.Tag, bool hint = false)
    {
        return new Region { Area = color, Value = timeValue, HasHint = hint, Wonders = wonders.ToList(), };
    }

    private static Shrine CreateShrine(Color color, NatureWonder[] wonders, TimeValue timeValue = TimeValue.Tag, bool hint = false)
    {
        return new Shrine
        {
            Area = color, 
            Value = timeValue, 
            HasHint = hint, 
            Wonders = wonders
        };
    }

    [Fact]
    public void Flat_rule_scores_constant_points()
    {
        Player player = CreatePlayer();
        Shrine shrine = CreateShrine(Color.Grau, []);
        shrine.ScoringRule = new ScoringRule { Type = ScoringType.Flat, Points = 7 };
        player.AddShrine(shrine);

        int score = player.ComputePlayerScore();
        Assert.Equal(7, score);
    }

    [Fact]
    public void PerHint_counts_all_hints()
    {
        Player player = CreatePlayer();
        player.AddRegionAppend(CreateRegion(Color.Gelb, [], hint:true));
        player.AddRegionAppend(CreateRegion(Color.Rot, [], hint:true));
        player.AddRegionAppend(CreateRegion(Color.Blau, [], hint:false));
        Shrine shrine = CreateShrine(Color.Grau, [], hint:true);
        shrine.ScoringRule = new ScoringRule { Type = ScoringType.PerHint, Points = 2 };
        player.AddShrine(shrine);

        int score = player.ComputePlayerScore();
        Assert.Equal(6, score);
    }

    [Fact]
    public void PerBlueStone_counts_all_steine()
    {
        Player player = CreatePlayer();
        player.AddRegionAppend(CreateRegion(Color.Gruen, wonders: [NatureWonder.Stein, NatureWonder.Stein,]));
        player.AddRegionAppend(CreateRegion(Color.Rot, wonders: [NatureWonder.Stein,]));
        Shrine shrine = CreateShrine(Color.Grau, wonders: [NatureWonder.Stein,]);
        shrine.ScoringRule = new ScoringRule { Type = ScoringType.PerBlueStone, Points = 1, };
        player.AddShrine(shrine);

        int score = player.ComputePlayerScore();
        Assert.Equal(4, score);
    }

    [Fact]
    public void PerChimaere_counts_all_chimaeren_with_multiplier()
    {
        Player player = CreatePlayer();
        player.AddRegionAppend(CreateRegion(Color.Gruen, wonders: [NatureWonder.Chimaere,]));
        Shrine shrine = CreateShrine(Color.Grau, wonders: [NatureWonder.Chimaere,]);
        shrine.ScoringRule = new ScoringRule { Type = ScoringType.PerChimaere, Points = 3 };
        player.AddShrine(shrine);

        int score = player.ComputePlayerScore();
        Assert.Equal(6, score);
    }

    [Fact]
    public void PerDistel_counts_all_disteln()
    {
        Player player = CreatePlayer();
        player.AddRegionAppend(CreateRegion(Color.Gruen, wonders: [NatureWonder.Distel, NatureWonder.Distel,]));
        player.AddRegionAppend(CreateRegion(Color.Blau, wonders: [NatureWonder.Distel,]));
        Shrine shrine = CreateShrine(Color.Grau, wonders: [NatureWonder.Distel, NatureWonder.Distel,]);
        shrine.ScoringRule = new ScoringRule { Type = ScoringType.PerDistel, Points = 1 };
        player.AddShrine(shrine);

        int score = player.ComputePlayerScore();
        Assert.Equal(5, score);
    }

    [Fact]
    public void PerNightSymbol_counts_regions_and_shrines_with_nacht()
    {
        Player player = CreatePlayer();
        player.AddRegionAppend(CreateRegion(Color.Gelb, [], TimeValue.Nacht));
        player.AddRegionAppend(CreateRegion(Color.Rot, [], TimeValue.Nacht));
        player.AddRegionAppend(CreateRegion(Color.Blau, []));
        Shrine shrine = CreateShrine(Color.Grau, [], TimeValue.Nacht);
        shrine.ScoringRule = new ScoringRule { Type = ScoringType.PerNightSymbol, Points = 2 };
        player.AddShrine(shrine);

        int score = player.ComputePlayerScore();
        Assert.Equal(6, score);
    }

    [Fact]
    public void PerColor_counts_cards_of_one_color()
    {
        Player player = CreatePlayer();
        player.AddRegionAppend(CreateRegion(Color.Blau, []));
        player.AddRegionAppend(CreateRegion(Color.Blau, []));
        player.AddRegionAppend(CreateRegion(Color.Gruen, []));
        Shrine shrine = CreateShrine(Color.Blau, []);
        shrine.ScoringRule = new ScoringRule { Type = ScoringType.PerColor, Points = 2, ColorOne = Color.Blau };
        player.AddShrine(shrine);

        int score = player.ComputePlayerScore();
        Assert.Equal(6, score);
    }

    [Fact]
    public void PerColorTwo_sums_two_colors()
    {
        Player player = CreatePlayer();
        player.AddRegionAppend(CreateRegion(Color.Gelb, []));
        player.AddRegionAppend(CreateRegion(Color.Gelb, []));
        player.AddRegionAppend(CreateRegion(Color.Rot, []));
        Shrine shrine = CreateShrine(Color.Grau, []);
        shrine.ScoringRule = new ScoringRule { Type = ScoringType.PerColorTwo, Points = 1, ColorOne = Color.Gelb, ColorTwo = Color.Rot };
        player.AddShrine(shrine);

        int score = player.ComputePlayerScore();
        Assert.Equal(3, score);
    }

    [Fact]
    public void PerColorSet_uses_minimum_full_sets()
    {
        Player player = CreatePlayer();
        player.AddRegionAppend(CreateRegion(Color.Gelb, []));
        player.AddRegionAppend(CreateRegion(Color.Rot, []));
        player.AddRegionAppend(CreateRegion(Color.Gruen, []));
        player.AddRegionAppend(CreateRegion(Color.Blau, []));
        player.AddRegionAppend(CreateRegion(Color.Gruen, []));
        Shrine shrine = CreateShrine(Color.Gelb, []);
        shrine.ScoringRule = new ScoringRule { Type = ScoringType.PerColorSet, Points = 5 };
        player.AddShrine(shrine);

        int score = player.ComputePlayerScore();
        Assert.Equal(5, score);
    }
}
