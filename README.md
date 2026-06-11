# CodeScene Explorer

Small .NET 10 console app for reading CodeScene data and shaping it into a report.

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
- Monthly report output as a table

## Scope
- Projects
- Analyses
- Code health
- Repository evolution

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