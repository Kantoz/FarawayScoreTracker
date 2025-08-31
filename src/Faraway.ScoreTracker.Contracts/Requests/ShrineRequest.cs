using Faraway.ScoreTracker.Core.Enums;

namespace Faraway.ScoreTracker.Contracts.Requests;
public record ShrineRequest(
    bool HasHint,
    List<NatureWonder> Wonders,
    Color Area,
    TimeValue Value,
    ScoringRuleRequest ScoringRule
);
