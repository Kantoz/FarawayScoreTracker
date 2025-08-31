# ScoreTracker – Neue Entities + Contracts + Create-Endpoints

## Start
```bash
dotnet build
dotnet run --project src/ScoreTracker.Api
```

## Swagger
In Development automatisch aktiv.

## Beispiel-Requests (HTTPie)
### Spieler anlegen
http POST :5078/players name="Alice"

### Region erstellen
http POST :5078/regions Nummer:=1 Wert:=Tag HinweisVorhanden:=true Farbe:=Blau Wonders:='["Stein"]' Conditions:='["Stein"]' ScoringType:=PerHint Points:=2 Color1:=null Color2:=null

### Heiligtum erstellen
http POST :5078/shrines HinweisVorhanden:=false Wonders:='["Chimaere"]' Farbe:=Grau Wert:=Nacht ScoringType:=Flat Points:=3 Color1:=null Color2:=null

### Region dem Spieler zuweisen
http POST :5078/players/{playerId}/regions RegionId:={regionId} IsFaceUp:=true

### Heiligtum dem Spieler zuweisen
http POST :5078/players/{playerId}/shrines ShrineId:={shrineId}

### Score berechnen
http :5078/players/{playerId}/score
