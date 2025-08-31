namespace Faraway.ScoreTracker.Contracts.Responses;

public record GameResponse
{
    public Guid Id { get; set; }
    public DateTime PlayTime { get; set; }

    public IEnumerable<PlayerResponse> Players { get; set; } = [];
}