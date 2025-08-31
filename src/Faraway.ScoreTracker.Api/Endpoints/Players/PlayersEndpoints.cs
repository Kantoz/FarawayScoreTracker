using Faraway.ScoreTracker.Contracts.Responses;
using Faraway.ScoreTracker.Core.Entities;
using Faraway.ScoreTracker.Infrastructure.Abstractions;
using Faraway.ScoreTracker.Infrastructure.Persistence;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Faraway.ScoreTracker.Api.Endpoints.Players;

public static class PlayersEndpoints
{
    public static IEndpointRouteBuilder MapPlayers(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/player").WithTags("Player");

        group.MapGet("/", GetAllPlayer)
            .Produces<IEnumerable<PlayerResponse>>();
        
        group.MapGet("/{playerId:guid}", GetPlayer)
            .WithName("GetPlayer")
            .Produces<PlayerResponse>()
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{playerId:guid}", DeletePlayer)
            .WithName("DeletePlayer")
            .Produces(StatusCodes.Status204NoContent);

        group.MapGet("/{playerId:guid}/score", GetScore)
            .Produces<ScoreResponse>()
            .Produces(StatusCodes.Status404NotFound);
        return app;
    }

    private static async Task<Results<Ok<PlayerResponse>, NotFound>> GetPlayer(
        Guid playerId, IPlayerRepository repository)
    {
        var player = await repository.GetAsync(playerId);
        return player is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(player.Adapt<PlayerResponse>());
    }
    
    private static async Task<NoContent> DeletePlayer(Guid playerId,  ScoreTrackerDbContext db)
    {
        await db.Players
            .Where(p => p.Id == playerId)
            .ExecuteDeleteAsync();
        return TypedResults.NoContent();
    }

    private static async Task<Ok<IEnumerable<PlayerResponse>>> GetAllPlayer(IPlayerRepository repo)
    {
        IEnumerable<PlayerResponse> players = (await repo.GetAllAsync()).Select(CreatePlayerResponse);
        return TypedResults.Ok(players);
    }

    private static PlayerResponse CreatePlayerResponse(Player player)
    {
        return new PlayerResponse
        {
            Id = player.Id,
            Name = player.Name,
            Regions = CreateRegion(player),
            Shrines = CreateShrines(player),
        };
    }

    private static IEnumerable<ShrineResponse> CreateShrines(Player player)
    {
        return player.Shrines.Select(s =>
            new ShrineResponse
            {
                Area = s.Area,
                HasHint = s.HasHint,
                Wonders = s.Wonders,
                Value = s.Value,
                Id = s.Id,
                ScoringRule = s.ScoringRule.Adapt<ScoringRuleResponse>(),
            }
        );
    }

    private static IEnumerable<RegionResponse> CreateRegion(Player p)
    {
        return p.Regions.Select(r => new RegionResponse
        {
            Area = r.Region.Area,
            HasHint = r.Region.HasHint,
            Number = r.Region.Number,
            Value = r.Region.Value,
            Wonders = r.Region.Wonders,
            Condition = r.Region.Condition,
            ScoringRule = r.Region.ScoringRule.Adapt<ScoringRuleResponse>(),
            IsFaceUp = r.IsFaceUp,
            Position = r.Position,
        });
    }

    private static async Task<Results<Ok<ScoreResponse>, NotFound>> GetScore(
        IPlayerRepository repo,
        Guid playerId)
    {
        Player? player = (await repo.GetAsync(playerId)).Adapt<Player?>();
        if (player is null)
        {
            return TypedResults.NotFound();
        }

        int score = player.ComputePlayerScore();
        return TypedResults.Ok(new ScoreResponse(score));
    }
}