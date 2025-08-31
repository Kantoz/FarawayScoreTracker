using System.Text.Json.Serialization;
using Faraway.ScoreTracker.Api.Endpoints.Game;
using Faraway.ScoreTracker.Api.Endpoints.HealthCheck;
using Faraway.ScoreTracker.Api.Endpoints.Players;
using Faraway.ScoreTracker.Infrastructure.Abstractions;
using Faraway.ScoreTracker.Infrastructure.Mapping;
using Faraway.ScoreTracker.Infrastructure.Persistence;
using Faraway.ScoreTracker.Infrastructure.Repositories;
using Mapster;
using Microsoft.EntityFrameworkCore;

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

builder.Services.AddHealthChecks()
    .AddDbContextCheck<ScoreTrackerDbContext>("Database");

WebApplication app = builder.Build();

app.MapHealthChecks("/health").WithTags("System").WithOpenApi();

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
app.MapHealth();

app.Run();