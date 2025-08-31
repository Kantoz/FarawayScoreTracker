using Faraway.ScoreTracker.Core.Enums;

namespace Faraway.ScoreTracker.Contracts.Responses;

public record ScoringRuleResponse
{
    public ScoringType Type { get; set; }
    public int Points { get; set; }
    public Color? ColorOne { get; set; }
    public Color? ColorTwo { get; set; }
}