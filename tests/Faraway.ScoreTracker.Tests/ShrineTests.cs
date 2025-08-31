using Faraway.ScoreTracker.Core.Entities;
using Faraway.ScoreTracker.Core.Enums;
using FluentAssertions;
using Xunit;

namespace Faraway.ScoreTracker.Tests;

public class ShrineTests
{
    [Fact]
    public void AddShrine_keeps_values_and_optional_rule()
    {
        var player = new Player { Name = "A" };
        var rule = new ScoringRule { Type = ScoringType.PerDistel, Points = 5 };
        var shrine = new Shrine()
        {
            Area = Color.Gruen,
            HasHint = true,
            Value = TimeValue.Nacht,
            ScoringRule = rule,
        };

        player.AddShrine(shrine);

        player.Shrines.Should().ContainSingle();
        var s = player.Shrines[0];
        s.Area.Should().Be(0);
        s.HasHint.Should().BeTrue();
        s.Value.Should().Be(TimeValue.Nacht);
        s.ScoringRule.Should().NotBeNull();
        s.ScoringRule!.Points.Should().Be(5);
    }
}