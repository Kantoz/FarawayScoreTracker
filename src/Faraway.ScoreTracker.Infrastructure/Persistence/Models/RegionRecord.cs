using Faraway.ScoreTracker.Core.Enums;

namespace Faraway.ScoreTracker.Infrastructure.Persistence.Models;

public record RegionRecord : BaseRecord
{
    public int Number { get; set; }
    public TimeValue Value { get; set; }
    public bool HasHint { get; set; }
    public Color Area { get; set; }

    public List<NatureWonder> Wonders { get; set; } = [];
    public List<NatureWonder> Condition { get; set; } = [];
    public ScoringRuleRecord? ScoringRule { get; set; }
}
