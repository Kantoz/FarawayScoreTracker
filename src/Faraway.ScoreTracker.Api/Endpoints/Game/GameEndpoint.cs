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
    public static readonly string PrefixApiRoute = "/api/game";
    private const int MaxRegions = 8;
    
    public static IEndpointRouteBuilder MapGames(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup(PrefixApiRoute).WithTags("Game");

        group.MapPost("/", CreateGame)
            .Produces<GameResponse>(StatusCodes.Status201Created)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest);

        group.MapGet("/", GetAllGames)
            .Produces<IEnumerable<GameResponse>>();

        group.MapDelete("/{gameId:guid}", DeleteGame)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{gameId:guid}/score", GetScore)
            .Produces<GameScoreResponse>()
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<Results<Ok<GameScoreResponse>, NotFound>> GetScore(
        Guid gameId,
        IGameRepository repo,
        CancellationToken ct = default)
    {
        Core.Entities.Game? game = await repo.GetAsync(gameId, ct);
        if (game is null)
        {
            return TypedResults.NotFound();
        }

        PlayerGameScoreResponse[] playerScores = game.Players
            .Select(p => new PlayerGameScoreResponse
            {
                Points = p.ComputePlayerScore(),
                PlayerName = p.Name,
            })
            .ToArray();

        if (playerScores.Length == 0)
        {
            return TypedResults.Ok(new GameScoreResponse
            {
                PlayerScores = [],
                Winner = [],
            });
        }

        int maxPoints = playerScores.Max(ps => ps.Points);

        return TypedResults.Ok(new GameScoreResponse
        {
            PlayerScores = playerScores,
            Winner = playerScores.Where(ps => ps.Points == maxPoints).ToArray(),
        });
    }

    private static async Task<Results<NoContent, NotFound>> DeleteGame(
        Guid gameId,
        ScoreTrackerDbContext db,
        CancellationToken ct = default)
    {
        int gamesToDelete = await db.Games.Where(g => g.Id == gameId).ExecuteDeleteAsync(ct);
        return gamesToDelete == 0 ? TypedResults.NotFound() : TypedResults.NoContent();
    }

    private static async Task<Ok<IEnumerable<GameResponse>>> GetAllGames(
        ScoreTrackerDbContext db,
        CancellationToken ct = default)
    {
        List<GameRecord> games = await db.Games
            .AsNoTracking()
            .Include(g => g.Players)
            .ThenInclude(p => p.PlayerRegions)
            .ThenInclude(pr => pr.Region)
            .ThenInclude(r => r.ScoringRule)
            .Include(g => g.Players)
            .ThenInclude(p => p.Shrines)
            .ThenInclude(s => s.ScoringRule)
            .ToListAsync(ct);

        return TypedResults.Ok(games.Adapt<IEnumerable<GameResponse>>());
    }

    private static async Task<Results<Created<GameResponse>, BadRequest<ErrorResponse>>> CreateGame(
        ScoreTrackerDbContext db,
        GameRequest req,
        CancellationToken ct)
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

        string[] playerRequestNames = req.Players.Select(p => p.Name).ToArray();
        if (playerRequestNames.Any(string.IsNullOrWhiteSpace))
        {
            return TypedResults.BadRequest(new ErrorResponse("Jeder Spieler benötigt einen Namen."));
        }

        string[] playersWithMaxRegions = req.Players
            .Where(p => p.Regions.Count() > MaxRegions)
            .Select(p => p.Name)
            .ToArray();

        if (playersWithMaxRegions.Any())
        {
            return TypedResults.BadRequest(
                new ErrorResponse(
                    $"Spieler '{string.Join(", ", playersWithMaxRegions)}': maximal {MaxRegions} Regionen."));
        }

        List<PlayerRecord> newPlayers = new();
        foreach (PlayerRequest playerRequest in req.Players)
        {
            newPlayers.Add(CreatePlayerRecord(playerRequest));
        }

        foreach (PlayerRecord playerRecord in newPlayers)
        {
            game.Players.Add(playerRecord);
        }

        db.Players.AddRange(newPlayers);

        try
        {
            await db.SaveChangesAsync(ct);
            return TypedResults.Created($"{PrefixApiRoute}/{game.Id}", game.Adapt<GameResponse>());
        }
        catch (DbUpdateException ex) when (DbHelper.IsUnique(ex))
        {
            return TypedResults.BadRequest(new ErrorResponse("Spielername bereits vergeben."));
        }
    }

    private static PlayerRecord CreatePlayerRecord(PlayerRequest playerRequest)
    {
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
            RegionRecord region = CreateRegionRecord(regionRequest);

            player.PlayerRegions.Add(CreatePlayerRegionRecord(player, region, pos));
        }

        foreach (ShrineRequest s in playerRequest.Shrines)
        {
            ShrineRecord shrine = CreateShrineRecord(player, s);

            player.Shrines.Add(shrine);
        }

        return player;
    }

    private static ShrineRecord CreateShrineRecord(PlayerRecord player, ShrineRequest s)
    {
        return new ShrineRecord
        {
            Id = Guid.NewGuid(),
            PlayerId = player.Id,
            HasHint = s.HasHint,
            Area = s.Area,
            Value = s.Value,
            ScoringRule = CreateScoringRule(s),
            Wonders = s.Wonders,
        };
    }

    private static ScoringRuleRecord CreateScoringRule(ShrineRequest s)
    {
        return new ScoringRuleRecord
        {
            Type = s.ScoringRule.Type,
            Points = s.ScoringRule.Points,
            ColorOne = s.ScoringRule.ColorOne,
            ColorTwo = s.ScoringRule.ColorTwo,
        };
    }

    private static PlayerRegionRecord CreatePlayerRegionRecord(PlayerRecord player, RegionRecord region, int pos)
    {
        return new PlayerRegionRecord
        {
            Id = Guid.NewGuid(),
            PlayerId = player.Id,
            Region = region,
            IsFaceUp = true,
            Position = pos,
        };
    }

    private static RegionRecord CreateRegionRecord(RegionRequest regionRequest)
    {
        return new RegionRecord
        {
            Id = Guid.NewGuid(),
            Number = regionRequest.Number,
            Value = regionRequest.Value,
            HasHint = regionRequest.HasHint,
            Area = regionRequest.Area,
            Wonders = regionRequest.Wonders.ToList(),
            Condition = regionRequest.Condition.ToList(),
            ScoringRule = CreateScoringRuleRecord(regionRequest),
        };
    }

    private static ScoringRuleRecord CreateScoringRuleRecord(RegionRequest regionRequest)
    {
        return new ScoringRuleRecord
        {
            Type = regionRequest.ScoringRule.Type,
            Points = regionRequest.ScoringRule.Points,
            ColorOne = regionRequest.ScoringRule.ColorOne,
            ColorTwo = regionRequest.ScoringRule.ColorTwo,
        };
    }
}