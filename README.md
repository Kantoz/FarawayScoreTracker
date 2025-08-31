# Faraway ScoreTracker

[![CI](https://github.com/Kantoz/FarawayScoreTracker/actions/workflows/ci.yml/badge.svg)](https://github.com/<ORG_ODER_USER>/FarawayScoreTracker/actions/workflows/ci.yml)

Ein .NET 8 Projekt zur Verwaltung von Spielergebnissen.  
CI/CD läuft über **GitHub Actions** mit automatischem Docker-Build und Push nach **GitHub Container Registry (GHCR)**.

---

## 🚀 Entwicklung lokal

### Voraussetzungen
- [.NET SDK 8.0](https://dotnet.microsoft.com/download)
- [Docker](https://docs.docker.com/get-docker/)

### Build & Test
```bash
dotnet restore Faraway.ScoreTracker.sln
dotnet build Faraway.ScoreTracker.sln -c Release
dotnet test tests/Faraway.ScoreTracker.Tests/Faraway.ScoreTracker.Tests.csproj -c Release
