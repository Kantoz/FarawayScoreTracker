namespace Faraway.ScoreTracker.Contracts.Responses;

public record GameScoreResponse
{
    public IEnumerable<PlayerGameScoreResponse> PlayerScores { get; set; } = [];

    public IEnumerable<PlayerGameScoreResponse> Winner { get; set; } = [];
}