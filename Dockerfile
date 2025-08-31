FROM mcr.microsoft.com/dotnet/sdk:8.0-bookworm-slim AS build
WORKDIR /src

COPY src/Faraway.ScoreTracker.Api/Faraway.ScoreTracker.Api.csproj src/Faraway.ScoreTracker.Api/
COPY src/Faraway.ScoreTracker.Core/Faraway.ScoreTracker.Core.csproj src/Faraway.ScoreTracker.Core/
COPY src/Faraway.ScoreTracker.Contracts/Faraway.ScoreTracker.Contracts.csproj src/Faraway.ScoreTracker.Contracts/
COPY src/Faraway.ScoreTracker.Infrastructure/Faraway.ScoreTracker.Infrastructure.csproj src/Faraway.ScoreTracker.Infrastructure/

RUN dotnet restore src/Faraway.ScoreTracker.Api/Faraway.ScoreTracker.Api.csproj

COPY . .

RUN dotnet publish src/Faraway.ScoreTracker.Api/Faraway.ScoreTracker.Api.csproj -c Release -o /out --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0-bookworm-slim
WORKDIR /app

RUN addgroup --system appgroup \
 && adduser --system --ingroup appgroup appuser \
 && mkdir -p /app/data \
 && chown -R appuser:appgroup /app

ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

COPY --from=build /out ./

USER appuser

ENTRYPOINT ["dotnet", "Faraway.ScoreTracker.Api.dll"]
