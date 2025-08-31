using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Faraway.ScoreTracker.Contracts.Errors;
using Faraway.ScoreTracker.Contracts.Requests;
using Faraway.ScoreTracker.Contracts.Responses;
using Faraway.ScoreTracker.Core.Enums;
using Faraway.ScoreTracker.Infrastructure.Persistence;
using Faraway.ScoreTracker.Tests.Helper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Faraway.ScoreTracker.Tests;

public class GameEndpointsTests(TestWebApplicationFactory factory) 
    : IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient _client = factory.CreateClient();
    private static readonly JsonSerializerOptions ReadJson = new() { PropertyNameCaseInsensitive = true, };

    public async Task InitializeAsync()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        ScoreTrackerDbContext db = scope.ServiceProvider.GetRequiredService<ScoreTrackerDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.MigrateAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;
    


    [Fact]
    public async Task CreateGame_with_1_player_returns_201_and_persists()
    {
        string playerName = "Alice";
        GameRequest req = TestFixtures.SimpleGame(playerName);

        HttpResponseMessage resp = await _client.PostAsync("/api/game/", JsonHelpers.AsStringEnumJson(req));
        string body = await resp.Content.ReadAsStringAsync();

        resp.StatusCode.Should().Be(HttpStatusCode.Created, $"Body:\n{body}");

        GameResponse? created = await resp.Content.ReadFromJsonAsync<GameResponse>(JsonHelpers.ReadStringEnums);
        created.Should().NotBeNull();
        created!.Players.Should().HaveCount(1);
        created.Players.First().Name.Should().Be(playerName);
    }

    [Fact]
    public async Task CreateGame_returns_400_when_no_players()
    {
        GameRequest bad = TestFixtures.NoPlayers();

        HttpResponseMessage resp = await _client.PostAsync("/api/game/", JsonHelpers.AsStringEnumJson(bad));
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        ErrorResponse? err = await resp.Content.ReadFromJsonAsync<ErrorResponse>(ReadJson);
        err!.ErrorMessage.Should().Contain("zwischen 1 und 6");
    }

    [Fact]
    public async Task CreateGame_returns_400_when_player_name_empty()
    {
        GameRequest bad = new(
            PlayTime: null,
            Players:
            [
                new PlayerRequest(
                    Name: "",
                    Regions: [],
                    Shrines: []
                ),
            ]);

        HttpResponseMessage resp = await _client.PostAsync("/api/game/", JsonHelpers.AsStringEnumJson(bad));
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        ErrorResponse? err = await resp.Content.ReadFromJsonAsync<ErrorResponse>(ReadJson);
        err!.ErrorMessage.Should().Contain("Jeder Spieler benötigt einen Namen");
    }

    [Fact]
    public async Task CreateGame_with_exactly_8_regions_is_allowed()
    {
        string playerName = "Max";
        GameRequest req = TestFixtures.GameWith8Regions(playerName);

        HttpResponseMessage resp = await _client.PostAsync("/api/game/", JsonHelpers.AsStringEnumJson(req));
        string body = await resp.Content.ReadAsStringAsync();

        resp.StatusCode.Should().Be(HttpStatusCode.Created, $"Body:\n{body}");

        GameResponse? created = await resp.Content.ReadFromJsonAsync<GameResponse>(JsonHelpers.ReadStringEnums);
        created.Should().NotBeNull();

        PlayerResponse p = created!.Players.First();
        p.Regions.Should().HaveCount(8);
        IEnumerable<int> positions = p.Regions.Select(r => r.Position);
        positions.Should().BeEquivalentTo(Enumerable.Range(1, 8));
    }

    [Fact]
    public async Task CreateGame_with_9_regions_returns_400()
    {
        List<RegionRequest> nineRegions = Enumerable.Range(1, 9).Select(i =>
            new RegionRequest(
                Number: i,
                TimeValue.Tag,
                HasHint: false,
                Color.Gelb,
                Wonders: [],
                Condition: [],
                ScoringRule: new ScoringRuleRequest(
                    ScoringType.Flat,
                    Points: 1,
                    ColorOne: null,
                    ColorTwo: null
                )
            )).ToList();

        PlayerRequest player = new("Overloaded", nineRegions, []);
        GameRequest req = new(null, [player,]);

        HttpResponseMessage resp = await _client.PostAsync("/api/game/", JsonHelpers.AsStringEnumJson(req));
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAllGames_returns_created_game()
    {
        string playerName = "Alice";
        GameRequest req = TestFixtures.SimpleGame(playerName);

        HttpResponseMessage create = await _client.PostAsync("/api/game/", JsonHelpers.AsStringEnumJson(req));
        create.EnsureSuccessStatusCode();

        HttpResponseMessage resp = await _client.GetAsync("/api/game/");
        resp.EnsureSuccessStatusCode();

        List<GameResponse>? games = await resp.Content.ReadFromJsonAsync<List<GameResponse>>(JsonHelpers.ReadStringEnums);
        games.Should().NotBeNull();
        games!.Any(g => g.Players.Any(p => p.Name == playerName)).Should().BeTrue();
    }

    [Fact]
    public async Task GetScore_returns_winner_and_scores()
    {
        string playerAlice = "Alice";
        string playerBob = "Bob";
        GameRequest req = new GameRequest(null, [
            TestFixtures.SimpleGame(playerAlice).Players.First(),
            TestFixtures.SimpleGame(playerBob).Players.First(),
        ]);

        HttpResponseMessage create = await _client.PostAsync("/api/game/", JsonHelpers.AsStringEnumJson(req));
        create.EnsureSuccessStatusCode();
        GameResponse? created = await create.Content.ReadFromJsonAsync<GameResponse>(JsonHelpers.ReadStringEnums);
        Guid gameId = created!.Id;

        HttpResponseMessage resp = await _client.GetAsync($"/api/game/{gameId}/score");
        resp.EnsureSuccessStatusCode();

        GameScoreResponse? score = await resp.Content.ReadFromJsonAsync<GameScoreResponse>(ReadJson);

        score.Should().NotBeNull();
        score!.PlayerScores.Should().HaveCount(2);
        score.PlayerScores.Select(p => p.PlayerName).Should().Contain([playerAlice, playerBob,]);

        score.Winner.Select(w => w.PlayerName).Should().HaveCount(2);
    }

    [Fact]
    public async Task DeleteGame_existing_returns_204_and_removes_game()
    {
        GameRequest req = TestFixtures.SimpleGame("ToDelete");

        HttpResponseMessage create = await _client.PostAsync("/api/game/", JsonHelpers.AsStringEnumJson(req));
        create.EnsureSuccessStatusCode();

        GameResponse? created = await create.Content.ReadFromJsonAsync<GameResponse>(JsonHelpers.ReadStringEnums);
        Guid id = created!.Id;

        HttpResponseMessage del = await _client.DeleteAsync($"/api/game/{id}");
        del.StatusCode.Should().Be(HttpStatusCode.NoContent);

        HttpResponseMessage afterDel = await _client.GetAsync($"/api/game/{id}/score");
        afterDel.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
