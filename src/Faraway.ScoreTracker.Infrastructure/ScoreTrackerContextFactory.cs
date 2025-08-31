using Faraway.ScoreTracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Faraway.ScoreTracker.Infrastructure;

public class ScoreTrackerContextFactory : IDesignTimeDbContextFactory<ScoreTrackerDbContext>
{
    public ScoreTrackerDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ScoreTrackerDbContext>();

        var dataDir = Path.Combine(AppContext.BaseDirectory, "Data");
        Directory.CreateDirectory(dataDir);
        var dbPath = Path.Combine(dataDir, "scoretracker.db");

        options.UseSqlite($"Data Source={dbPath}");
        return new ScoreTrackerDbContext(options.Options);
    }
}