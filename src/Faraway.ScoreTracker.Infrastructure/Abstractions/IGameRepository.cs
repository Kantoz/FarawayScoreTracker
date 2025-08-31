using Faraway.ScoreTracker.Core.Entities;
using Faraway.ScoreTracker.Infrastructure.Persistence.Models;
using Faraway.ScoreTracker.Infrastructure.Repositories;

namespace Faraway.ScoreTracker.Infrastructure.Abstractions;

public interface IGameRepository : IBaseRepository<GameRecord, Game>;