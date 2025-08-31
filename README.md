# Faraway ScoreTracker

[![CI](https://github.com/Kantoz/FarawayScoreTracker/actions/workflows/ci.yml/badge.svg)](https://github.com/Kantoz/FarawayScoreTracker/actions/workflows/ci.yml)

Faraway ScoreTracker ist ein Beispielprojekt in **.NET 8**, das die Verwaltung und Auswertung von Spielergebnissen demonstriert.  
Es kombiniert moderne Softwareentwicklungspraktiken mit einem durchgängigen Setup: **Clean Architecture**, **xUnit-Tests**, **Docker-Container** und automatisiertes **CI/CD über GitHub Actions**.

---

## 🛠 Tech-Stack

- **.NET 8 / ASP.NET Core Web API**
- **Entity Framework Core** – Datenzugriff & Migrationen
- **xUnit + FluentAssertions** – Unit- und Integrationstests
- **Docker** – Multi-Stage-Build für kleine, sichere Images
- **GitHub Actions** – CI/CD Pipeline mit Test & Deployment
- **GitHub Container Registry (GHCR)** – Container-Registry für Images

---

## ⚡ Quickstart

### Voraussetzungen
- [.NET SDK 8.0](https://dotnet.microsoft.com/download)
- [Docker](https://docs.docker.com/get-docker/)

### Build & Test lokal
```bash
dotnet restore Faraway.ScoreTracker.sln
dotnet build Faraway.ScoreTracker.sln -c Release
dotnet test tests/Faraway.ScoreTracker.Tests/Faraway.ScoreTracker.Tests.csproj -c Release
```
### Build & Test lokal (Docker)
```bash
./builddocker.bat
```
---
## 📂 Projektstruktur
```bash
src/
  Faraway.ScoreTracker.Api/            # ASP.NET Core API
  Faraway.ScoreTracker.Core/           # Domänenlogik
  Faraway.ScoreTracker.Contracts/      # DTOs & Schnittstellen
  Faraway.ScoreTracker.Infrastructure/ # Datenbank/EF Core

tests/
  Faraway.ScoreTracker.Tests/          # xUnit Tests

```
---
## 🔄 CI/CD
```markdown
## 🔄 CI/CD
Bei jedem Commit wird über GitHub Actions automatisch:
- die Solution gebaut
- alle Tests ausgeführt
- ein Docker-Image erstellt und nach [GHCR](https://ghcr.io/kantoz/scoretracker) gepusht  
  (Tags: `latest`, `YYYYMMDD-SHA`, `SHA`)
```
---

##📝 Lizenz

MIT
