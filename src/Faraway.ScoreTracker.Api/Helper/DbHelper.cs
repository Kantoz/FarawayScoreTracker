using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Faraway.ScoreTracker.Api.Helper;

public static class DbHelper
{
    public static bool IsUnique(DbUpdateException ex)
    {
        return ex.InnerException is SqliteException s && s.SqliteErrorCode == 19 &&
               s.Message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase);
    }
}