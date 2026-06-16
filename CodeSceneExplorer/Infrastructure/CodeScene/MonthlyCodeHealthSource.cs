using System.Globalization;
using System.Text.Json;
using CodeSceneExplorer.Application.Abstractions;
using CodeSceneExplorer.Application.Reporting;
using Microsoft.Extensions.Logging;

namespace CodeSceneExplorer.Infrastructure.CodeScene;

public sealed class MonthlyCodeHealthSource(
    ICodeSceneApi api,
    ILogger<MonthlyCodeHealthSource>? logger = null) : IMonthlyCodeHealthSource
{
    private IReadOnlyList<ProjectEntry>? _projects;
    private readonly Dictionary<int, IReadOnlyList<KpiSample>> _kpiTrendByProject = new();

    public async Task<IReadOnlyList<MonthlyCodeHealthReading>> GetReadingsAsync(
        DateOnly start,
        DateOnly end,
        CancellationToken cancellationToken = default)
    {
        var readings = new List<MonthlyCodeHealthReading>();

        foreach (var project in await GetProjectsAsync(cancellationToken).ConfigureAwait(false))
        {
            var sample = await GetLatestKpiSampleOnOrBeforeAsync(project.Id, end, cancellationToken)
                .ConfigureAwait(false);

            if (sample is null)
            {
                logger?.LogInformation(
                    "No KPI trend data found for project {ProjectId} by {Cutoff:yyyy-MM-dd}.",
                    project.Id,
                    end);
                continue;
            }

            readings.Add(new MonthlyCodeHealthReading(start.ToString("yyyy-MM"), sample.Kpi, project.Id, project.Name));
            logger?.LogInformation(
                "Project {ProjectId} month {YearMonth}: KPI sample from {SampleDate:yyyy-MM-dd} with code health {CodeHealth}.",
                project.Id,
                start.ToString("yyyy-MM", CultureInfo.InvariantCulture),
                sample.Date,
                sample.Kpi);
        }

        return readings;
    }

    private async Task<IReadOnlyList<ProjectEntry>> GetProjectsAsync(CancellationToken cancellationToken)
    {
        if (_projects is not null)
        {
            return _projects;
        }

        var projects = new List<ProjectEntry>();
        var page = 1;
        var maxPages = 1;

        while (page <= maxPages)
        {
            var response = await api.ListProjectsAsync(page: page, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            var pageData = ParseProjectsPage(response);
            maxPages = pageData.MaxPages;

            if (pageData.Projects.Count == 0)
            {
                break;
            }

            projects.AddRange(pageData.Projects);
            page++;
        }

        _projects = projects;
        return _projects;
    }

    private async Task<KpiSample?> GetLatestKpiSampleOnOrBeforeAsync(
        int projectId,
        DateOnly cutoff,
        CancellationToken cancellationToken)
    {
        var trend = await GetKpiTrendAsync(projectId, cancellationToken).ConfigureAwait(false);
        return trend.LastOrDefault(s => s.Date <= cutoff);
    }

    private async Task<IReadOnlyList<KpiSample>> GetKpiTrendAsync(
        int projectId,
        CancellationToken cancellationToken)
    {
        if (_kpiTrendByProject.TryGetValue(projectId, out var cached))
            return cached;

        var response = await api.GetKpiTrendCodeHealthAverageAsync(projectId, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        var samples = ParseKpiTrend(response);
        _kpiTrendByProject[projectId] = samples;
        return samples;
    }

    private static IReadOnlyList<KpiSample> ParseKpiTrend(string response)
    {
        using var document = JsonDocument.Parse(response);

        if (document.RootElement.ValueKind != JsonValueKind.Array)
            return [];

        var samples = new List<KpiSample>();

        foreach (var item in document.RootElement.EnumerateArray())
        {
            var sample = TryParseKpiSample(item);

            if (sample is not null)
            {
                samples.Add(sample);
            }
        }

        return samples;
    }

    private static KpiSample? TryParseKpiSample(JsonElement item)
    {
        if (!item.TryGetProperty("date", out var dateProp)
            || !DateOnly.TryParse(dateProp.GetString(), out var date))
        {
            return null;
        }

        if (!item.TryGetProperty("kpi", out var kpiProp)
            || !kpiProp.TryGetDecimal(out var kpi))
        {
            return null;
        }

        return new KpiSample(date, kpi);
    }

    private static ProjectPage ParseProjectsPage(string response)
    {
        using var document = JsonDocument.Parse(response);

        if (!document.RootElement.TryGetProperty("projects", out var projects) || projects.ValueKind != JsonValueKind.Array)
            return new ProjectPage([], 1);

        var projectEntries = new List<ProjectEntry>();

        foreach (var item in projects.EnumerateArray())
        {
            if (item.TryGetProperty("id", out var idProperty) && idProperty.TryGetInt32(out var projectId))
            {
                var projectName = item.TryGetProperty("name", out var nameProperty)
                    ? nameProperty.GetString()
                    : null;

                projectEntries.Add(new ProjectEntry(projectId, projectName));
            }
        }

        var maxPages = document.RootElement.TryGetProperty("max_pages", out var maxPagesProperty)
            && maxPagesProperty.TryGetInt32(out var parsedMaxPages)
            ? parsedMaxPages
            : 1;

        return new ProjectPage(projectEntries, maxPages);
    }

    private sealed record ProjectPage(IReadOnlyList<ProjectEntry> Projects, int MaxPages);

    private sealed record ProjectEntry(int Id, string? Name);

    private sealed record KpiSample(DateOnly Date, decimal Kpi);
}
