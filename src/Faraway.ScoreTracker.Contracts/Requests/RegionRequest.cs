using Faraway.ScoreTracker.Core.Enums;

namespace Faraway.ScoreTracker.Contracts.Requests;
public record RegionRequest(
    int Number,
    TimeValue Value,
    bool HasHint,
    Color Area,
    List<NatureWonder> Wonders,
    List<NatureWonder> Condition,
    ScoringRuleRequest ScoringRule
);
