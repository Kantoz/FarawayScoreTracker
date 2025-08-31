using Faraway.ScoreTracker.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace Faraway.ScoreTracker.Infrastructure.Persistence;

public class ScoreTrackerDbContext : DbContext
{
    public DbSet<PlayerRecord> Players => Set<PlayerRecord>();
    public DbSet<PlayerRegionRecord> PlayerRegions => Set<PlayerRegionRecord>();
    public DbSet<RegionRecord> Regions => Set<RegionRecord>();
    public DbSet<ShrineRecord> Shrines => Set<ShrineRecord>();
    public DbSet<ScoringRuleRecord> ScoringRules => Set<ScoringRuleRecord>();

    public DbSet<GameRecord> Games => Set<GameRecord>();

    public ScoreTrackerDbContext(DbContextOptions<ScoreTrackerDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ScoreTrackerDbContext).Assembly);
    }
}
