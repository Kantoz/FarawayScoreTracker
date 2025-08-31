using Faraway.ScoreTracker.Core.Enums;

namespace Faraway.ScoreTracker.Core.Entities;
public record Shrine : BaseEntity
{
    public bool HasHint { get; set; }
    public IEnumerable<NatureWonder> Wonders { get; set; } = [];
    public Color Area { get; set; }
    public TimeValue Value { get; set; }
    public ScoringRule? ScoringRule { get; set; } = new();
}
