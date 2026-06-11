# CodeScene Explorer plan

## Goal
Build a small .NET console app that reads data from the CodeScene API and turns it into a report.

## Decisions
- Read-only integration only.
- Single port for the application layer.
- Infrastructure owns HTTP + DTO mapping.
- API token is read from `~\CodeSceneApiToken.txt`.
- Base URL is configurable.
- Use explicit date ranges for reproducible reports.

## In-scope API areas
1. Projects
2. Analyses
3. Code health
4. Repository evolution

## Excluded API areas
- write endpoints
- author statistics
- hotspot code health
- components/files
- teams/developer settings
- code coverage

## Suggested first slice
1. Projects
2. Analyses
3. Code health
4. Repository evolution
