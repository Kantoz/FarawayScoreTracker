namespace Faraway.ScoreTracker.Contracts.Responses;

public record PlayerGameScoreResponse
{
    public string PlayerName { get; set; } = string.Empty;

    public int Points { get; set; }
    
}