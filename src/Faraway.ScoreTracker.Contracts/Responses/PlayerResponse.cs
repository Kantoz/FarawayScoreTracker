namespace Faraway.ScoreTracker.Contracts.Responses;

public record PlayerResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
    
    public IEnumerable<RegionResponse> Regions { get; set; } = [];
    
    public IEnumerable<ShrineResponse> Shrines { get; set; } = [];
}
