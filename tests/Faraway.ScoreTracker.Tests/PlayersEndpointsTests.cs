using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Faraway.ScoreTracker.Api.Endpoints.Players;
using Faraway.ScoreTracker.Contracts.Responses;
using Faraway.ScoreTracker.Infrastructure.Persistence;
using Faraway.ScoreTracker.Infrastructure.Persistence.Models;
using Faraway.ScoreTracker.Tests.Helper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Faraway.ScoreTracker.Tests;

public class PlayersEndpointsTests
{
    private static async Task<(TestWebApplicationFactory factory, HttpClient client)> NewAppAsync(
        Func<ScoreTrackerDbContext, Task>? seed = null)
    {
        var factory = new TestWebApplicationFactory();
        var client  = factory.CreateClient();

        if (seed is not null)
        {
            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ScoreTrackerDbContext>();
            await seed(db);
        }

        return (factory, client);
    }

    [Fact]
    public async Task GetAll_returns_empty_when_no_players()
    {
        await using var _ = (await NewAppAsync()).factory;
        HttpClient client = (await NewAppAsync()).client;
        HttpResponseMessage resp = await client.GetAsync($"{PlayersEndpoints.PrefixApiRoute}/");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var list = await resp.Content.ReadFromJsonAsync<List<PlayerResponse>>();
        list!.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAll_returns_seeded_players()
    {
        (TestWebApplicationFactory factory, HttpClient client) ctx = await NewAppAsync();
        await using TestWebApplicationFactory _ = ctx.factory;              
        HttpClient client = ctx.client;

        using (IServiceScope scope = ctx.factory.Services.CreateScope())
        {
            ScoreTrackerDbContext db = scope.ServiceProvider.GetRequiredService<ScoreTrackerDbContext>();

            Guid gameId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            if (!await db.Games.AnyAsync(g => g.Id == gameId))
            {
                db.Games.Add(new GameRecord { Id = gameId, PlayTime = DateTime.Today });
            }

            db.Players.Add(new PlayerRecord { Id = Guid.NewGuid(), Name = "Alice", GameId = gameId });
            db.Players.Add(new PlayerRecord { Id = Guid.NewGuid(), Name = "Bob",   GameId = gameId });

            await db.SaveChangesAsync();
        }

        HttpResponseMessage resp = await client.GetAsync($"{PlayersEndpoints.PrefixApiRoute}/");
        resp.EnsureSuccessStatusCode();

        List<PlayerResponse>? list = await resp.Content.ReadFromJsonAsync<List<PlayerResponse>>();
        list!.Select(p => p.Name).Should().BeEquivalentTo("Alice", "Bob");
    }


    [Fact]
    public async Task GetById_returns_200_for_existing()
    {
        Guid playerId = Guid.NewGuid();
        Guid gameId   = Guid.Parse("11111111-1111-1111-1111-111111111111");

        (TestWebApplicationFactory factory, HttpClient client) = await NewAppAsync(async db =>
        {
            if (!await db.Games.AnyAsync(g => g.Id == gameId))
            {
                db.Games.Add(new GameRecord { Id = gameId, PlayTime = DateTime.Today });
            }

            db.Players.Add(new PlayerRecord
            {
                Id     = playerId,
                Name   = "Charlie",
                GameId = gameId
            });

            await db.SaveChangesAsync();
        });
        await using TestWebApplicationFactory _ = factory;

        HttpResponseMessage resp = await client.GetAsync($"{PlayersEndpoints.PrefixApiRoute}/{playerId}");
        string body = await resp.Content.ReadAsStringAsync();

        resp.StatusCode.Should().Be(HttpStatusCode.OK,
            $"Unexpected response: {(int)resp.StatusCode} {resp.ReasonPhrase}\nBody:\n{body}");

        PlayerResponse? dto = await resp.Content.ReadFromJsonAsync<PlayerResponse>();
        dto.Should().NotBeNull($"Body:\n{body}");
        dto.Id.Should().Be(playerId);
        dto.Name.Should().Be("Charlie");
    }


    [Fact]
    public async Task GetById_returns_404_for_missing()
    {
        var ctx = await NewAppAsync();  
        await using var _ = ctx.factory;
        var client = ctx.client;

        HttpResponseMessage resp = await client.GetAsync($"{PlayersEndpoints.PrefixApiRoute}/{Guid.NewGuid()}");
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_existing_returns_204_and_really_deletes()
    {
        var id = Guid.NewGuid();

        var ctx = await NewAppAsync();  
        await using var _ = ctx.factory;
        var client = ctx.client;

        HttpResponseMessage resp = await client.DeleteAsync($"{PlayersEndpoints.PrefixApiRoute}/{id}");
        resp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var scope = ctx.factory.Services.CreateScope();
        var db2 = scope.ServiceProvider.GetRequiredService<ScoreTrackerDbContext>();
        (await db2.Players.FindAsync(id)).Should().BeNull();
    }

    [Fact]
    public async Task Delete_missing_returns_204()
    {
        var ctx = await NewAppAsync();  
        await using var _ = ctx.factory;
        var client = ctx.client;

        HttpResponseMessage resp = await client.DeleteAsync($"{PlayersEndpoints.PrefixApiRoute}/{Guid.NewGuid()}");
        resp.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task GetScore_returns_200_for_existing_player()
    {
        Guid playerId = Guid.NewGuid();
        Guid gameId   = Guid.Parse("11111111-1111-1111-1111-111111111111");

        (TestWebApplicationFactory factory, HttpClient client) = await NewAppAsync(async db =>
        {
            if (!await db.Games.AnyAsync(g => g.Id == gameId))
                db.Games.Add(new GameRecord { Id = gameId, PlayTime = DateTime.Today });

            db.Players.Add(new PlayerRecord
            {
                Id     = playerId,
                Name   = "Dora",
                GameId = gameId
            });

            await db.SaveChangesAsync();
        });
        await using var _ = factory;

        HttpResponseMessage resp = await client.GetAsync($"{PlayersEndpoints.PrefixApiRoute}/{playerId}/score");
        string body = await resp.Content.ReadAsStringAsync();

        resp.StatusCode.Should().Be(HttpStatusCode.OK,
            $"Unexpected response: {(int)resp.StatusCode} {resp.ReasonPhrase}\nBody:\n{body}");

        var score = await resp.Content.ReadFromJsonAsync<ScoreResponse>();
        score.Should().NotBeNull($"Body:\n{body}");
        score.Points.Should().BeGreaterThanOrEqualTo(0, "score should be a non-negative number");
    }


    [Fact]
    public async Task GetScore_returns_404_for_missing()
    {
        var ctx = await NewAppAsync();  
        await using var _ = ctx.factory;
        var client = ctx.client;

        HttpResponseMessage resp = await client.GetAsync($"{PlayersEndpoints.PrefixApiRoute}/{Guid.NewGuid()}/score");
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}