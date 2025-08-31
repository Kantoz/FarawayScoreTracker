@echo off
setlocal

REM Nutzung: efm.bat MigrationName [weitere EF-Argumente]
if "%~1"=="" (
  echo Nutzung: %~nx0 MigrationName [weitere EF-Argumente]
  exit /b 1
)

set "MIG=%~1"
REM Optional: Leerzeichen durch Unterstriche ersetzen
set "MIG=%MIG: =_%"

REM Erstes Arg (den Namen) verwerfen
shift

REM Alle restlichen Args sauber einsammeln und quoten
set "EXTRA="
:collect
if "%~1"=="" goto run
set "EXTRA=%EXTRA% "%~1""
shift
goto collect

:run
pushd "%~dp0"
echo + dotnet ef migrations add "%MIG%" --project "src/ScoreTracker.Infrastructure" --startup-project "src/ScoreTracker.Api" --context "ScoreTrackerDbContext"%EXTRA%
dotnet ef migrations add "%MIG%" --project "src/ScoreTracker.Infrastructure" --startup-project "src/ScoreTracker.Api" --context "ScoreTrackerDbContext"%EXTRA%
set "rc=%ERRORLEVEL%"
popd
exit /b %rc%
