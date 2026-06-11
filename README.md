# CodeScene Explorer

CodeScene Explorer is a small .NET console application used to experiment with the CodeScene API. The goal of the project is to inspect how repositories evolve over time and turn that information into a readable report.

- API calls against CodeScene
- repository evolution analysis
- report generation from the collected data

## Purpose

The project exists as a proof of concept for exploring CodeScene programmatically. It is meant to help answer questions such as:

- How have all repositories changed over time?
- How can that data be summarized for a human-readable report?

## Project Structure

- `CodeSceneExplorer/Program.cs` - console entry point
- `CodeSceneExplorer/CodeSceneExplorer.csproj` - project configuration
- `README.md` - project overview and usage notes

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


