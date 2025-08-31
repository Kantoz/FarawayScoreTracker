@echo off
setlocal EnableExtensions

REM In den Ordner der Batch wechseln (Repo-Root)
cd /d "%~dp0"

echo [CHECK] Docker CLI ...
docker --version >NUL 2>&1
if errorlevel 1 (
  echo [ERROR] Docker nicht gefunden. Bitte Docker Desktop installieren/starten.
  exit /b 1
)

echo [CHECK] docker compose v2 ...
docker compose version >NUL 2>&1
if errorlevel 1 (
  echo [ERROR] docker compose v2 nicht gefunden.
  exit /b 1
)

echo.
echo [STEP] Stoppe Container, entferne Volumes und verwaiste Ressourcen ...
docker compose down --volumes --remove-orphans
if errorlevel 1 (
  echo [WARN] docker compose down meldete einen Fehler - eventuell lief nichts
)

echo.
echo [STEP] Baue Images komplett neu (ohne Cache) ...
docker compose build --no-cache
if errorlevel 1 (
  echo [ERROR] Build fehlgeschlagen.
  exit /b 1
)

echo.
echo [STEP] Starte Container im Hintergrund (force recreate) ...
docker compose up -d --force-recreate
if errorlevel 1 (
  echo [ERROR] Start fehlgeschlagen.
  exit /b 1
)

echo.
echo [INFO] Container laufen. Oeffne Swagger ...
start "" http://localhost:8080/swagger

REM Optional: Mit "builddocker.bat logs" direkt Logs anzeigen
if /i "%~1"=="logs" (
  echo.
  echo [INFO] Oeffne Logs (Strg+C beendet)...
  docker compose logs -f
)

endlocal
exit /b 0
