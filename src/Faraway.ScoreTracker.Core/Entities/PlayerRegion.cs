namespace Faraway.ScoreTracker.Core.Entities;
public record PlayerRegion : BaseEntity 
{
    public bool IsFaceUp { get; set; } = true;
    public Region Region { get; set; } = null!;

    public int Position { get; set; }

    public void FlipRegion()
    {
        IsFaceUp = !IsFaceUp;
    }
    
    public void FlipRegionFaceDown()
    {
        IsFaceUp = false;
    }
}
