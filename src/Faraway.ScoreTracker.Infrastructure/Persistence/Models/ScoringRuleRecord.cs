using Faraway.ScoreTracker.Core.Enums;

namespace Faraway.ScoreTracker.Infrastructure.Persistence.Models;

public record ScoringRuleRecord : BaseRecord
{
    public ScoringType Type { get; set; }
    public int Points { get; set; }
    public Color? ColorOne { get; set; }
    public Color? ColorTwo { get; set; }
}
