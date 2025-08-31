using Faraway.ScoreTracker.Api.Helper;
using Faraway.ScoreTracker.Contracts.Errors;
using Faraway.ScoreTracker.Contracts.Requests;
using Faraway.ScoreTracker.Contracts.Responses;
using Faraway.ScoreTracker.Infrastructure.Abstractions;
using Faraway.ScoreTracker.Infrastructure.Persistence;
using Faraway.ScoreTracker.Infrastructure.Persistence.Models;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Faraway.ScoreTracker.Api.Endpoints.Game;

public static class GameEndpoint
{
    public static IEndpointRouteBuilder MapGames(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/game").WithTags("Game");

        group.MapPost("/", CreateGame)
            .Produces<CreateIdResponse>(StatusCodes.Status201Created)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest);

        group.MapGet("/", GetAllGames)
            .Produces<IEnumerable<GameResponse>>();

        group.MapDelete("/", DeleteGame)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status204NoContent);

        group.MapGet("/{gameId:guid}/score", GetScore)
            .Produces<GameScoreResponse>()
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<Results<Ok<GameScoreResponse>, NotFound>> GetScore(Guid gameId,
        IGameRepository repo)
    {
        Core.Entities.Game? game = await repo.GetAsync(gameId);

        if (game is null)
        {
            return TypedResults.NotFound();
        }

        IEnumerable<PlayerGameScoreResponse> playerScores = game.Players.Select(p => new PlayerGameScoreResponse
        {
            Points = p.ComputePlayerScore(),
            PlayerName = p.Name,
        }).ToArray();

        int maxPoints = playerScores.Max(ps => ps.Points);
        
        return TypedResults.Ok(new GameScoreResponse
        {
            PlayerScores = playerScores,
            Winner = playerScores.Where(ps => ps.Points == maxPoints).ToArray(),
        });
    }

    private static async Task<Results<NoContent, NotFound>> DeleteGame(Guid gameId, ScoreTrackerDbContext db)
    {
        await db.Games.Where(g => g.Id == gameId).ExecuteDeleteAsync();

        return TypedResults.NoContent();
    }

    private static async Task<Ok<IEnumerable<GameResponse>>> GetAllGames(ScoreTrackerDbContext db)
    {
        var games = await db.Games
            .Include(g => g.Players)
            .ThenInclude(p => p.PlayerRegions)
            .ThenInclude(pr => pr.Region)
            .Include(g => g.Players)
            .ThenInclude(p => p.Shrines)
            .ThenInclude(s => s.ScoringRule)
            .Include(g => g.Players)
            .ThenInclude(p => p.PlayerRegions)
            .ThenInclude(pr => pr.Region.ScoringRule)
            .ToListAsync();

        return TypedResults.Ok(games.Adapt<IEnumerable<GameResponse>>());
    }

    private static async Task<Results<Created<GameResponse>, BadRequest<ErrorResponse>>> CreateGame(
        ScoreTrackerDbContext db, GameRequest req, CancellationToken ct)
    {
        if (req.Players.Count() is < 1 or > 6)
        {
            return TypedResults.BadRequest(new ErrorResponse("Spieleranzahl muss zwischen 1 und 6 liegen."));
        }

        GameRecord game = new()
        {
            Id = Guid.NewGuid(),
            PlayTime = req.PlayTime ?? DateTime.UtcNow,
        };
        db.Games.Add(game);

        List<PlayerRecord> newPlayers = [];

        foreach (PlayerRequest playerRequest in req.Players)
        {
            if (string.IsNullOrWhiteSpace(playerRequest.Name))
            {
                return TypedResults.BadRequest(new ErrorResponse("Jeder Spieler benötigt einen Namen."));
            }

            PlayerRecord player = new()
            {
                Id = Guid.NewGuid(),
                Name = playerRequest.Name,
                PlayerRegions = [],
                Shrines = [],
            };

            int pos = 0;
            foreach (RegionRequest regionRequest in playerRequest.Regions)
            {
                pos++;
                if (pos > 8)
                {
                    return TypedResults.BadRequest(
                        new ErrorResponse($"Spieler '{playerRequest.Name}': maximal 8 Regionen."));
                }

                RegionRecord region = new()
                {
                    Id = Guid.NewGuid(),
                    Number = regionRequest.Number,
                    Value = regionRequest.Value,
                    HasHint = regionRequest.HasHint,
                    Area = regionRequest.Area,
                    Wonders = regionRequest.Wonders.ToList(),
                    Condition = regionRequest.Condition.ToList(),
                    ScoringRule = new ScoringRuleRecord
                    {
                        Type = regionRequest.ScoringRule.Type,
                        Points = regionRequest.ScoringRule.Points,
                        ColorOne = regionRequest.ScoringRule.ColorOne,
                        ColorTwo = regionRequest.ScoringRule.ColorTwo,
                    },
                };

                player.PlayerRegions.Add(new PlayerRegionRecord
                {
                    Id = Guid.NewGuid(),
                    PlayerId = player.Id,
                    Region = region,
                    IsFaceUp = true,
                    Position = pos,
                });
            }

            foreach (ShrineRequest s in playerRequest.Shrines)
            {
                ShrineRecord shrine = new ShrineRecord
                {
                    Id = Guid.NewGuid(),
                    PlayerId = player.Id,
                    HasHint = s.HasHint,
                    Area = s.Area,
                    Value = s.Value,
                    ScoringRule = new ScoringRuleRecord
                    {
                        Type = s.ScoringRule.Type,
                        Points = s.ScoringRule.Points,
                        ColorOne = s.ScoringRule.ColorOne,
                        ColorTwo = s.ScoringRule.ColorTwo,
                    },
                    Wonders = s.Wonders,
                };

                player.Shrines.Add(shrine);
            }

            newPlayers.Add(player);
        }

        db.Players.AddRange(newPlayers);

        foreach (PlayerRecord p in newPlayers)
        {
            game.Players.Add(p);
        }

        db.Players.AddRange(newPlayers);
        
        
        try
        {
            await db.SaveChangesAsync(ct);
            return TypedResults.Created($"/games/{game.Id}", game.Adapt<GameResponse>());
        }
        catch (DbUpdateException ex) when (DbHelper.IsUnique(ex))
        {
            return TypedResults.BadRequest(new ErrorResponse("Spielername bereits vergeben."));
        }
    }
}