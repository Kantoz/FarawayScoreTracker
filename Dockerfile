FROM mcr.microsoft.com/dotnet/sdk:8.0-bookworm-slim AS build
WORKDIR /src

COPY ./ ./

RUN dotnet restore src/ScoreTracker.Api/ScoreTracker.Api.csproj
RUN dotnet publish src/ScoreTracker.Api/ScoreTracker.Api.csproj -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:8.0-bookworm-slim
WORKDIR /app

RUN addgroup --system appgroup \
 && adduser --system --ingroup appgroup appuser \
 && mkdir -p /app/data \
 && chown -R appuser:appgroup /app

USER appuser

ENV ASPNETCORE_URLS=http://0.0.0.0:8080
COPY --from=build /out ./
EXPOSE 8080

ENTRYPOINT ["dotnet", "ScoreTracker.Api.dll"]
