namespace Faraway.ScoreTracker.Infrastructure.Persistence.Models;

public record PlayerRecord : BaseRecord
{
    public string Name { get; set; } = null!;
    public List<PlayerRegionRecord> PlayerRegions { get; set; } = new();
    public List<ShrineRecord> Shrines { get; set; } = new();
    
    public Guid GameId { get; set; }
    
    public GameRecord Game { get; set; } = null!;
}
