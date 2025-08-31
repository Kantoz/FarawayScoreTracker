name: CI

on:
  push:
    branches: [ "**" ]
  pull_request:
    branches: [ "**" ]

jobs:
  build-test-docker:
    runs-on: ubuntu-latest

    permissions:
      contents: read
      packages: read

    env:
      DOTNET_VERSION: "8.0.x"
      IMAGE_NAME: scoretracker            # optional: anpassen
      DOCKERFILE_PATH: ./Dockerfile       # im Repo-Root vorhanden
      CONTEXT_PATH: ./

    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}

      - name: Cache NuGet
        uses: actions/cache@v4
        with:
          path: ~/.nuget/packages
          key: nuget-${{ runner.os }}-${{ hashFiles('**/*.csproj') }}
          restore-keys: |
            nuget-${{ runner.os }}-

      - name: Restore
        run: dotnet restore ./ScoreTracker.sln

      - name: Build (Release)
        run: dotnet build ./ScoreTracker.sln -c Release --no-restore

      - name: Test
        run: dotnet test ./tests/ScoreTracker.Tests/ScoreTracker.Tests.csproj -c Release --no-build --logger "trx;LogFileName=testresults.trx"

      - name: Upload test results
        if: always()
        uses: actions/upload-artifact@v4
        with:
          name: testresults
          path: |
            **/TestResults/*.trx
            **/TestResults/**/*.xml
          if-no-files-found: ignore

      - name: Set image tags
        id: tags
        run: |
          SHORT_SHA=$(git rev-parse --short HEAD)
          DATE=$(date -u +%Y%m%d)
          echo "tags=${{ env.IMAGE_NAME }}:${DATE}-${SHORT_SHA},${{ env.IMAGE_NAME }}:${SHORT_SHA}" >> $GITHUB_OUTPUT

      - name: Build Docker image (no push)
        run: |
          docker build \
            --file "${{ env.DOCKERFILE_PATH }}" \
            --tag ${{ steps.tags.outputs.tags%%,* }} \
            "${{ env.CONTEXT_PATH }}"
