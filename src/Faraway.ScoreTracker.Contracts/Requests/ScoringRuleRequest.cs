using Faraway.ScoreTracker.Core.Enums;

namespace Faraway.ScoreTracker.Contracts.Requests;

public record ScoringRuleRequest(ScoringType Type, int Points, Color? ColorOne, Color? ColorTwo);