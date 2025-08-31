using System;
using System.IO;
using System.Text.Json.Serialization;
using Faraway.ScoreTracker.Api.Endpoints.Game;
using Faraway.ScoreTracker.Api.Endpoints.Players;
using Faraway.ScoreTracker.Infrastructure.Abstractions;
using Faraway.ScoreTracker.Infrastructure.Mapping;
using Faraway.ScoreTracker.Infrastructure.Persistence;
using Faraway.ScoreTracker.Infrastructure.Repositories;
using Mapster;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);


string connectionString = builder.Configuration.GetConnectionString("Default")
                          ?? throw new InvalidOperationException("Missing connection string 'Default'.");

try
{
    string filePath = connectionString.Replace("Data Source=", "", StringComparison.OrdinalIgnoreCase).Trim(' ', '"');
    string? dir = Path.GetDirectoryName(filePath);
    if (!string.IsNullOrWhiteSpace(dir))
    {
        Directory.CreateDirectory(dir);
    }
}
catch
{
    /* ignore */
}


builder.Services.AddDbContext<ScoreTrackerDbContext>(opt =>
{
    opt.UseSqlite(connectionString,
        b => b.MigrationsAssembly(typeof(ScoreTrackerDbContext).Assembly.FullName)
    );
    opt.EnableSensitiveDataLogging();
    opt.EnableDetailedErrors();
});

builder.Services.AddScoped<IPlayerRepository, PlayerRepository>();
builder.Services.AddScoped<IGameRepository, GameRepository>();
builder.Services.AddMapster();
MapsterConfig.RegisterMappings();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureHttpJsonOptions(o => { o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()); });
WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// --- DB sicher migrieren (optional Logging) ---
using (IServiceScope scope = app.Services.CreateScope())
{
    ScoreTrackerDbContext db = scope.ServiceProvider.GetRequiredService<ScoreTrackerDbContext>();
    db.Database.Migrate();
}

app.MapPlayers();
app.MapGames();

app.Run();