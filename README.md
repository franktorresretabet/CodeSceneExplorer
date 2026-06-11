# CodeScene Explorer

Small .NET 10 console app for reading CodeScene data and shaping it into a report.

## Structure
- `CodeSceneExplorer/Domain` - shared domain types
- `CodeSceneExplorer/Application` - read-only port(s)
- `CodeSceneExplorer/Infrastructure` - CodeScene HTTP adapter and DTOs
- `CodeSceneExplorer.Tests` - test suite

## Design
- Read-only, test-first, DDD-style layout
- Single application port for now
- Configurable API base URL
- API token loaded from `~\CodeSceneApiToken.txt`
- Explicit date ranges for reproducible reports

## Scope
- Projects
- Analyses
- Code health
- Repository evolution

