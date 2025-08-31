using Faraway.ScoreTracker.Core.Enums;

namespace Faraway.ScoreTracker.Contracts.Responses;

public record RegionResponse()
{
    public int Number { get; set; }
    public TimeValue Value { get; set; }
    public bool HasHint { get; set; }
    public Color Area { get; set; }
    public IEnumerable<NatureWonder> Wonders { get; set; } = [];
    public IEnumerable<NatureWonder>? Condition { get; set; } = [];
    public ScoringRuleResponse? ScoringRule { get; set; } = new();

    public bool IsFaceUp { get; set; } = false;
    public int Position { get; set; }
}