namespace Faraway.ScoreTracker.Contracts.Requests;

public record GameRequest(DateTime? PlayTime, IEnumerable<PlayerRequest> Players);