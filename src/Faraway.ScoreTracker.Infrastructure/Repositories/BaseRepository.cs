using Faraway.ScoreTracker.Core.Entities;
using Faraway.ScoreTracker.Infrastructure.Persistence;
using Faraway.ScoreTracker.Infrastructure.Persistence.Models;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Faraway.ScoreTracker.Infrastructure.Repositories;

public abstract class BaseRepository<TDomain, TRecord>(ScoreTrackerDbContext db) : IBaseRepository<TRecord, TDomain>
    where TRecord : BaseRecord, new()
    where TDomain : BaseEntity
{
    private DbSet<TRecord> Set => db.Set<TRecord>();

    private IQueryable<TRecord> Queryable() => Set.AsNoTracking().AsSplitQuery();
    
    private IQueryable<TRecord> QueryableTracked() => Set.AsSplitQuery();
    
    protected virtual IQueryable<TRecord> ApplyIncludesTracked(IQueryable<TRecord> query) => ApplyIncludes(query);
    
    protected virtual IQueryable<TRecord> ApplyIncludes(IQueryable<TRecord> query) => query;

    public async Task<IEnumerable<TDomain>> GetAllAsync(CancellationToken ct = default)
    {
        TRecord[] entities = await ApplyIncludes(Queryable()).ToArrayAsync(ct);
        return entities.Adapt<IEnumerable<TDomain>>();
    }
    
    public async Task SaveAsync(TDomain domain, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(domain);

        if (domain.Id == Guid.Empty)
        {
            TRecord record = domain.Adapt<TRecord>();
            await Set.AddAsync(record, ct);
            await db.SaveChangesAsync(ct);

            domain.Id = record.Id;
            return;
        }

        var tracked = await ApplyIncludesTracked(QueryableTracked())
            .SingleOrDefaultAsync(e => e.Id == domain.Id, ct);

        if (tracked is null)
        {
            throw new InvalidOperationException($"Entity with id '{domain.Id}' not found.");
        }

        domain.Adapt(tracked);

        await db.SaveChangesAsync(ct);
    }

    public async Task<TDomain?> GetAsync(Guid id, CancellationToken ct = default)
    {
        TRecord? entity = await ApplyIncludes(Queryable())
            .SingleOrDefaultAsync(e => e.Id == id, ct);

        return entity?.Adapt<TDomain>();
    }

    public async Task AddAsync(TDomain domain, CancellationToken ct = default)
    {
        TRecord record = domain.Adapt<TRecord>();
        await Set.AddAsync(record, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(TDomain domain, CancellationToken ct = default)
    {
        TRecord record = domain.Adapt<TRecord>();
        Set.Update(record);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await Set.Where(e => e.Id == id).ExecuteDeleteAsync(ct);
    }

    public async Task DeleteAllAsync(CancellationToken ct = default)
    {
        await Set.ExecuteDeleteAsync(ct);
    }
}
