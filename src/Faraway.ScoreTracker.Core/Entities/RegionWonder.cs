using Faraway.ScoreTracker.Core.Enums;

namespace Faraway.ScoreTracker.Core.Entities;
public record RegionWonder : BaseEntity
{
    public NatureWonder Wonder { get; set; }
}
