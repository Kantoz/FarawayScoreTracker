namespace Faraway.ScoreTracker.Infrastructure.Persistence.Models;

public record GameRecord : BaseRecord
{
    public DateTime PlayTime { get; set; }

    public List<PlayerRecord> Players { get; set; } = [];
}