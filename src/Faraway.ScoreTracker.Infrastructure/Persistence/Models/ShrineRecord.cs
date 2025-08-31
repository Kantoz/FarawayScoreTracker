using Faraway.ScoreTracker.Core.Enums;

namespace Faraway.ScoreTracker.Infrastructure.Persistence.Models;

public record ShrineRecord : BaseRecord
{
    public Guid PlayerId { get; set; }
    public PlayerRecord Player { get; set; } = null!;

    public Color Area { get; set; }
    public bool HasHint { get; set; }

    public List<NatureWonder> Wonders { get; set; } = [];

    public TimeValue Value { get; set; }
    
    public ScoringRuleRecord? ScoringRule { get; set; }
}
