using System;
using Faraway.ScoreTracker.Core.Entities;
using Faraway.ScoreTracker.Core.Enums;
using Xunit;

namespace Faraway.ScoreTracker.Tests;

public class ScoreTests
{
    [Fact]
    public void Computes_Simple_Scoring()
    {
        Player player = new() { Id = Guid.NewGuid(), Name = "Test", };
        Region regionOne = new()
        {
            Number = 5,
            Value = TimeValue.Nacht,
            HasHint = true,
            Area = Color.Blau,
            ScoringRule = new ScoringRule { Type = ScoringType.PerHint, Points = 2, },
            Wonders = [NatureWonder.Stein, ],
        };
        player.AddRegionAppend(regionOne);
        Shrine shrine = new()
        {
            HasHint = true,
            Area = Color.Grau,
            Value = TimeValue.Nacht,
            ScoringRule = new ScoringRule { Type = ScoringType.Flat, Points = 3, },
            Wonders = [ NatureWonder.Chimaere, ],
        };
        player.AddShrine(shrine);
        int score = player.ComputePlayerScore();
        Assert.True(score >= 5);
    }
    
    [Fact]
    public void Computes_Simple_Scoring2()
    {
        Player player = new() { Id = Guid.NewGuid(), Name = "Test", };
        Region regionOne = new()
        {
            Number = 2,
            Value = TimeValue.Tag,
            HasHint = false,
            Area = Color.Blau,
            ScoringRule = null,
            Wonders = [NatureWonder.Stein, NatureWonder.Stein, ],
        };
        Region regionTwo = new()
        {
            Number = 67,
            Value = TimeValue.Tag,
            HasHint = true,
            Area = Color.Gruen,
            Condition = [NatureWonder.Chimaere, NatureWonder.Chimaere, NatureWonder.Distel, NatureWonder.Distel, ],
            ScoringRule = new ScoringRule { Type = ScoringType.Flat, Points = 19, },
        };
        Region regionThree = new()
        {
            Number = 41,
            Value = TimeValue.Tag,
            HasHint = false,
            Area = Color.Gruen,
            Condition = [NatureWonder.Chimaere, NatureWonder.Stein, NatureWonder.Stein, ],
            ScoringRule = new ScoringRule { Type = ScoringType.PerNightSymbol, Points = 4, },
        };
        player.AddRegionAppend(regionOne);
        player.AddRegionAppend(regionTwo);
        player.AddRegionAppend(regionThree);
        Shrine shrineOne = new()
        {
            HasHint = false,
            Area = Color.Grau,
            Value = TimeValue.Tag,
            ScoringRule = new ScoringRule { Type = ScoringType.PerChimaere, Points = 1, },
            Wonders = [NatureWonder.Chimaere, ],
        };
        Shrine shrineTwo = new()
        {
            HasHint = false,
            Area = Color.Blau,
            Value = TimeValue.Tag,
            ScoringRule = null,
            Wonders = [ NatureWonder.Chimaere, ],
        };
        Shrine shrineThree = new()
        {
            HasHint = false,
            Area = Color.Blau,
            Value = TimeValue.Tag,
            ScoringRule = null,
            Wonders = [NatureWonder.Distel, ],
        };
        Shrine shrineFour = new()
        {
            HasHint = false,
            Area = Color.Grau,
            Value = TimeValue.Nacht,
            ScoringRule = null,
            Wonders = [NatureWonder.Distel, ],
        };
        player.AddShrine(shrineOne);
        player.AddShrine(shrineTwo);
        player.AddShrine(shrineThree);
        player.AddShrine(shrineFour);
        int score = player.ComputePlayerScore();
        Assert.Equal(21, score);
    }
    
    [Fact]
    public void Computes_Simple_Scoring3()
    {
        Player player = new() { Id = Guid.NewGuid(), Name = "Test", };
        Region regionOne = new()
        {
            Number = 64,
            Value = TimeValue.Tag,
            HasHint = true,
            Area = Color.Blau,
            Condition = [
            
                    NatureWonder.Stein,
                    NatureWonder.Stein,
                    NatureWonder.Distel,
                    NatureWonder.Distel,
            ],
            
            ScoringRule = new ScoringRule {Type = ScoringType.Flat, Points = 18, },
            Wonders = [],
        };
        Region regionTwo = new()
        {
            Number = 29,
            Value = TimeValue.Nacht,
            HasHint = true,
            Area = Color.Gelb,
            Condition = null,
            Wonders = [NatureWonder.Distel, ],
            ScoringRule = new ScoringRule { Type = ScoringType.PerDistel, Points = 2, },
        };
        Region regionThree = new()
        {
            Number = 30,
            Value = TimeValue.Nacht,
            HasHint = false,
            Area = Color.Rot,
            Condition = null,
            Wonders = [NatureWonder.Stein, ],
            ScoringRule = new ScoringRule { Type = ScoringType.PerBlueStone, Points = 2, },
        };
        Region regionFour = new()
        {
            Number = 41,
            Value = TimeValue.Tag,
            HasHint = false,
            Area = Color.Gruen,
            Condition =  
                [
                    NatureWonder.Chimaere,
                    NatureWonder.Stein,
                    NatureWonder.Stein,
                ]
            ,
            Wonders = [NatureWonder.Distel ],
            ScoringRule = new ScoringRule { Type = ScoringType.PerNightSymbol, Points = 4, },
        };
        Region regionFive = new()
        {
            Number = 2,
            Value = TimeValue.Tag,
            HasHint = false,
            Area = Color.Blau,
            Condition = null,
            Wonders = [NatureWonder.Stein , NatureWonder.Stein, ],
            ScoringRule = null,
        };
        Region regionSix = new()
        {
            Number = 7,
            Value = TimeValue.Tag,
            HasHint = false,
            Area = Color.Rot,
            Condition = null,
            Wonders = [NatureWonder.Chimaere , NatureWonder.Distel, ],
            ScoringRule = null,
        };
        player.AddRegionAppend(regionOne);
        player.AddRegionAppend(regionTwo);
        player.AddRegionAppend(regionThree);
        player.AddRegionAppend(regionFour);
        player.AddRegionAppend(regionFive);
        player.AddRegionAppend(regionSix);
        
        Shrine shrineOne = new()
        {
            HasHint = false,
            Area = Color.Blau,
            Value = TimeValue.Tag,
            Wonders = [NatureWonder.Distel, ],
        };
        Shrine shrineTwo = new()
        {
            HasHint = false,
            Area = Color.Grau,
            Value = TimeValue.Nacht,
            ScoringRule = null,
            Wonders = [NatureWonder.Distel, ],
        };
        player.AddShrine(shrineOne);
        player.AddShrine(shrineTwo);
        int score = player.ComputePlayerScore();
        Assert.Equal(38, score);
    }
}
