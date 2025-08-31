using Faraway.ScoreTracker.Core.Entities;
using Faraway.ScoreTracker.Infrastructure.Abstractions;
using Faraway.ScoreTracker.Infrastructure.Persistence;
using Faraway.ScoreTracker.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace Faraway.ScoreTracker.Infrastructure.Repositories;

public sealed class GameRepository : BaseRepository<Game, GameRecord>, IGameRepository
{
    public GameRepository(ScoreTrackerDbContext db) : base(db) { }
    protected override IQueryable<GameRecord> ApplyIncludes(IQueryable<GameRecord> q)
    {
        return q.Include(g => g.Players)
            .ThenInclude(p => p.PlayerRegions)
            .ThenInclude(r => r.Region)
            .ThenInclude(rr => rr.ScoringRule)
            .Include(g => g.Players)
            .ThenInclude(p => p.Shrines)
            .ThenInclude(s => s.ScoringRule);
    }
}
