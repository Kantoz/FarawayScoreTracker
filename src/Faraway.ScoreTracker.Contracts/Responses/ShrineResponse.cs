using Faraway.ScoreTracker.Core.Enums;

namespace Faraway.ScoreTracker.Contracts.Responses;

public record ShrineResponse()
{
    public Guid Id { get; set; }
    
    public bool HasHint { get; set; }
    
    public IEnumerable<NatureWonder> Wonders { get; set; } = [];
    
    public Color Area { get; set; }
    
    public TimeValue Value { get; set; }
    
    public ScoringRuleResponse? ScoringRule { get; set; } = new();
}