namespace Faraway.ScoreTracker.Core.Entities;

public record Game : BaseEntity
{
    public DateTime PlayTime { get; set; }
    
    public IEnumerable<Player> Players { get; set; } = [];
}