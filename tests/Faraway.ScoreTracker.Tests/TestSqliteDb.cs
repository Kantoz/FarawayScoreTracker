using System;
using System.Threading.Tasks;
using Faraway.ScoreTracker.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Faraway.ScoreTracker.Tests;

public sealed class TestSqliteDb : IAsyncDisposable
{
    private readonly SqliteConnection _conn;
    public ScoreTrackerDbContext Db { get; }

    public TestSqliteDb()
    {
        _conn = new SqliteConnection("DataSource=:memory:");
        _conn.Open();
        var options = new DbContextOptionsBuilder<ScoreTrackerDbContext>()
            .UseSqlite(_conn)
            .EnableSensitiveDataLogging()
            .Options;
        Db = new ScoreTrackerDbContext(options);
        Db.Database.EnsureCreated();
    }

    public async ValueTask DisposeAsync()
    {
        await Db.DisposeAsync();
        _conn.Close();
        _conn.Dispose();
    }
}