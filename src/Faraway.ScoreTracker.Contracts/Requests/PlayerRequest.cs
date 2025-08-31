namespace Faraway.ScoreTracker.Contracts.Requests;
public record PlayerRequest(string Name, IEnumerable<RegionRequest> Regions , IEnumerable<ShrineRequest> Shrines);
