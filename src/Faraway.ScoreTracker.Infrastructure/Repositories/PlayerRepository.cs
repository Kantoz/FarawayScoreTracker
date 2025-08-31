using Faraway.ScoreTracker.Core.Entities;
using Faraway.ScoreTracker.Infrastructure.Abstractions;
using Faraway.ScoreTracker.Infrastructure.Persistence;
using Faraway.ScoreTracker.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace Faraway.ScoreTracker.Infrastructure.Repositories;

public sealed class PlayerRepository : BaseRepository<Player, PlayerRecord>, IPlayerRepository
{
    public PlayerRepository(ScoreTrackerDbContext db) : base(db) { }

    protected override IQueryable<PlayerRecord> ApplyIncludes(IQueryable<PlayerRecord> q)
    {
        return q.Include(p => p.PlayerRegions).ThenInclude(r => r.Region).ThenInclude(r => r.ScoringRule)
            .Include(p => p.Shrines).ThenInclude(s => s.ScoringRule);
        
    }
}