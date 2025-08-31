namespace Faraway.ScoreTracker.Infrastructure.Repositories;

public interface IBaseRepository<TRecord, TDomain>
{
    Task<IEnumerable<TDomain>> GetAllAsync(CancellationToken ct = default);
    Task<TDomain?> 
        GetAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(TDomain domain, CancellationToken ct = default);
    Task UpdateAsync(TDomain domain, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}