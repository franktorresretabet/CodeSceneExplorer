# CodeScene Explorer

Small .NET 10 console app for reading CodeScene data and shaping it into a monthly code health report.

## Structure
- `CodeSceneExplorer/Domain` - shared domain types
- `CodeSceneExplorer/Application` - report use case, month generation, aggregation, formatting
- `CodeSceneExplorer/Infrastructure` - CodeScene HTTP adapter and source adapter
- `CodeSceneExplorer.Tests` - test suite

## Design
- Read-only, test-first, DDD-style layout
- Single application port for now
- Configurable API base URL
- API token loaded from `~\CodeSceneApiToken.txt`
- Explicit date ranges for reproducible reports
- Monthly report output as a table with code health and hotspot code health
- Uses CodeScene KPI trend data as the source of monthly code health and hotspot code health

## Configuration

The app reads configuration from `CodeSceneExplorer/appsettings.json` and supports CLI overrides through the .NET configuration system.

- `Report:StartDate` - first month to include in the report. Example: `2025-03-01`
- `Report:ScoreLimit` - optional decimal threshold. Projects whose readings are always above this value are excluded before monthly averaging. Example: `9.0`

Examples:

```bash
dotnet run --project CodeSceneExplorer/CodeSceneExplorer.csproj -- --Report:StartDate 2025-03-01
dotnet run --project CodeSceneExplorer/CodeSceneExplorer.csproj -- --Report:ScoreLimit 9.0
```

The report writes structured logs through log4net to `CodeSceneExplorer/bin/<configuration>/net10.0/log/`.

## Scope
- Projects
- Code health
- Hotspot code health
- KPI trends

## Notes for new contributors

- The report collects the last KPI and hotspot KPI samples on or before each month end for every project, then aggregates the remaining readings by month.
- If a score limit is configured, exclusion happens after fetching the full window of data and before monthly averaging.
- `appsettings.json` is copied to the output folder, so changes there affect the next run without code changes.
- `log4net.config` controls file logging; the file name pattern is `yyyyMMdd-HHmmss.CodeSceneExplorer.log`.

## Prerequisites

- .NET 10 SDK
- Access to a CodeScene instance or API endpoint
- Any required authentication credentials for the CodeScene API

## Getting Started

1. Restore and build the solution.

	```bash
	dotnet build CodeSceneExplorer.slnx
	```

2. Run the console application.

	```bash
	dotnet run --project CodeSceneExplorer/CodeSceneExplorer.csproj
	```

3. Run the tests.

	```bash
	dotnet test
	```