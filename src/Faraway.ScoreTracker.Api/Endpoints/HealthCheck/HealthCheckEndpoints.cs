using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Faraway.ScoreTracker.Api.Endpoints.HealthCheck;

public static class HealthEndpoint
{
    private static readonly string PrefixApiRoute = "/health";

    public static IEndpointRouteBuilder MapHealth(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup(PrefixApiRoute).WithTags("System");

        group.MapGet("/", RunHealthCheck)
            .WithName("Health")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status503ServiceUnavailable)
            .WithOpenApi();

        return app;
    }

    private static async Task<Results<Ok<object>, ProblemHttpResult>> RunHealthCheck(
        HealthCheckService healthChecks,
        CancellationToken ct)
    {
        HealthReport report = await healthChecks.CheckHealthAsync(ct);

        object payload = new
        {
            status = report.Status.ToString(),
            results = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
            }),
        };

        if (report.Status == HealthStatus.Healthy)
        {
            return TypedResults.Ok(payload);
        }

        return TypedResults.Problem(
            title: "Unhealthy",
            statusCode: StatusCodes.Status503ServiceUnavailable,
            extensions: new Dictionary<string, object?>
            {
                ["details"] = payload,
            });
    }
}