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

## Current status
- Done: read-only port, configurable base URL, file-based token, explicit date ranges
- Done: Projects read endpoints
- Done: Analyses read endpoints
- Done: Code health technical debt endpoint
- Done: Repository evolution endpoints for commit activity, commits, and issues
- Done: monthly period generator, monthly aggregation, report use case, source adapter, formatter, and console wiring
- Next: extend report slices only if the report needs more data

## Reporting boundary
- The reporting use case owns monthly period generation and code health aggregation.
- Infrastructure only fetches CodeScene data.
- Program only wires dependencies and triggers the use case.
- The final output is a plain monthly table.
