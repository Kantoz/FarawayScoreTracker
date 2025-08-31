namespace Faraway.ScoreTracker.Infrastructure.Persistence.Models;

public record PlayerRegionRecord: BaseRecord
{
    public Guid PlayerId { get; set; }
    public PlayerRecord Player { get; set; } = null!;

    public Guid RegionId { get; set; }
    public RegionRecord Region { get; set; } = null!;

    public int Position { get; set; }
    public bool IsFaceUp { get; set; }
}
