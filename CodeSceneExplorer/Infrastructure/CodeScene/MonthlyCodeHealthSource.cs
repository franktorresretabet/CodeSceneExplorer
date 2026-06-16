using System.Globalization;
using System.Text.Json;
using CodeSceneExplorer.Application.Abstractions;
using CodeSceneExplorer.Application.Reporting;
using CodeSceneExplorer.Domain.Shared;
using Microsoft.Extensions.Logging;

namespace CodeSceneExplorer.Infrastructure.CodeScene;

public sealed class MonthlyCodeHealthSource(
    ICodeSceneApi api,
    ILogger<MonthlyCodeHealthSource>? logger = null) : IMonthlyCodeHealthSource
{
    // Caches are safe here: the source is registered as a singleton that lives
    // for exactly one report run per process execution.
    private IReadOnlyList<int>? _projectIds;
    private readonly Dictionary<int, IReadOnlyList<AnalysisEntry>> _analysisByProject = new();
    private readonly Dictionary<int, decimal?> _codeHealthByAnalysis = new();

    public async Task<IReadOnlyList<MonthlyCodeHealthReading>> GetReadingsAsync(
        DateOnly start,
        DateOnly end,
        CancellationToken cancellationToken = default)
    {
        var readings = new List<MonthlyCodeHealthReading>();
        var period = DateRange.Create(start, end);

        foreach (var projectId in await GetProjectIdsAsync(cancellationToken).ConfigureAwait(false))
        {
            var analysis = await GetLatestAnalysisOnOrBeforeAsync(projectId, period.To, cancellationToken)
                .ConfigureAwait(false);

            if (analysis is null)
            {
                logger?.LogInformation(
                    "No analysis found for project {ProjectId} by {PeriodEnd:yyyy-MM-dd}.",
                    projectId,
                    period.To);
                continue;
            }

            var codeHealth = await GetCodeHealthAsync(projectId, analysis, cancellationToken)
                .ConfigureAwait(false);

            if (codeHealth.HasValue)
            {
                readings.Add(new MonthlyCodeHealthReading(start.ToString("yyyy-MM"), codeHealth.Value));
                logger?.LogInformation(
                    "Project {ProjectId} month {YearMonth}: selected analysis {AnalysisId} from {AnalysisDate:yyyy-MM-dd} with code health {CodeHealth}.",
                    projectId,
                    start.ToString("yyyy-MM", CultureInfo.InvariantCulture),
                    analysis.Id,
                    analysis.AnalysisTime.UtcDateTime,
                    codeHealth.Value);
            }
            else
            {
                logger?.LogWarning(
                    "Analysis {AnalysisId} for project {ProjectId} has no month_score.",
                    analysis.Id,
                    projectId);
            }
        }

        return readings;
    }

    private async Task<IReadOnlyList<int>> GetProjectIdsAsync(CancellationToken cancellationToken)
    {
        if (_projectIds is not null)
        {
            return _projectIds;
        }

        var projectIds = new List<int>();
        var page = 1;
        var maxPages = 1;

        while (page <= maxPages)
        {
            var response = await api.ListProjectsAsync(page: page, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            var pageData = ParseProjectsPage(response);
            maxPages = pageData.MaxPages;

            if (pageData.ProjectIds.Count == 0)
            {
                break;
            }

            projectIds.AddRange(pageData.ProjectIds);
            page++;
        }

        _projectIds = projectIds;
        return _projectIds;
    }

    private async Task<IReadOnlyList<AnalysisEntry>> GetAllAnalysesAsync(
        int projectId,
        CancellationToken cancellationToken)
    {
        if (_analysisByProject.TryGetValue(projectId, out var cached))
        {
            return cached;
        }

        var allEntries = new List<AnalysisEntry>();
        var page = 1;
        var maxPages = 1;

        while (page <= maxPages)
        {
            var response = await api.ListAnalysesAsync(projectId, page, cancellationToken)
                .ConfigureAwait(false);
            var pageData = ParseAnalysesPage(response);
            maxPages = pageData.MaxPages;

            if (pageData.Entries.Count == 0)
            {
                break;
            }

            allEntries.AddRange(pageData.Entries);
            page++;
        }

        _analysisByProject[projectId] = allEntries;
        return allEntries;
    }

    private async Task<AnalysisEntry?> GetLatestAnalysisOnOrBeforeAsync(
        int projectId,
        DateOnly cutoff,
        CancellationToken cancellationToken)
    {
        var analyses = await GetAllAnalysesAsync(projectId, cancellationToken).ConfigureAwait(false);

        return analyses
            .Where(a => DateOnly.FromDateTime(a.AnalysisTime.UtcDateTime) <= cutoff)
            .OrderByDescending(a => a.AnalysisTime)
            .FirstOrDefault();
    }

    private async Task<decimal?> GetCodeHealthAsync(
        int projectId,
        AnalysisEntry analysis,
        CancellationToken cancellationToken)
    {
        if (_codeHealthByAnalysis.TryGetValue(analysis.Id, out var cached))
        {
            return cached;
        }

        var response = await api.GetAnalysisAsync(projectId, analysis.Id.ToString(), cancellationToken)
            .ConfigureAwait(false);
        var codeHealth = TryExtractMonthScore(response);
        _codeHealthByAnalysis[analysis.Id] = codeHealth;
        return codeHealth;
    }

    private static ProjectPage ParseProjectsPage(string response)
    {
        using var document = JsonDocument.Parse(response);

        if (!document.RootElement.TryGetProperty("projects", out var projects) || projects.ValueKind != JsonValueKind.Array)
        {
            return new ProjectPage([], 1);
        }

        var projectIds = new List<int>();

        foreach (var item in projects.EnumerateArray())
        {
            if (item.TryGetProperty("id", out var idProperty) && idProperty.TryGetInt32(out var projectId))
            {
                projectIds.Add(projectId);
            }
        }

        var maxPages = document.RootElement.TryGetProperty("max_pages", out var maxPagesProperty) && maxPagesProperty.TryGetInt32(out var parsedMaxPages)
            ? parsedMaxPages
            : 1;

        return new ProjectPage(projectIds, maxPages);
    }

    private static AnalysisPage ParseAnalysesPage(string response)
    {
        using var document = JsonDocument.Parse(response);

        var maxPages = document.RootElement.TryGetProperty("max_pages", out var maxPagesProperty)
            && maxPagesProperty.TryGetInt32(out var parsedMaxPages)
            ? parsedMaxPages
            : 1;

        if (!document.RootElement.TryGetProperty("analyses", out var analyses) || analyses.ValueKind != JsonValueKind.Array)
        {
            return new AnalysisPage([], maxPages);
        }

        var entries = new List<AnalysisEntry>();

        foreach (var item in analyses.EnumerateArray())
        {
            if (item.TryGetProperty("id", out var idProperty)
                && idProperty.TryGetInt32(out var analysisId)
                && item.TryGetProperty("analysistime", out var timeProperty)
                && DateTimeOffset.TryParse(timeProperty.GetString(), out var analysisTime))
            {
                entries.Add(new AnalysisEntry(analysisId, analysisTime));
            }
        }

        return new AnalysisPage(entries, maxPages);
    }

    private static decimal? TryExtractMonthScore(string response)
    {
        using var document = JsonDocument.Parse(response);
        return TryExtractMonthScore(document.RootElement);
    }

    private static decimal? TryExtractMonthScore(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            if (element.TryGetProperty("high_level_metrics", out var highLevelMetrics)
                && highLevelMetrics.ValueKind == JsonValueKind.Object
                && highLevelMetrics.TryGetProperty("month_score", out var monthScoreProperty)
                && monthScoreProperty.TryGetDecimal(out var codeHealth))
            {
                return codeHealth;
            }

            foreach (var property in element.EnumerateObject())
            {
                var nestedCodeHealth = TryExtractMonthScore(property.Value);

                if (nestedCodeHealth.HasValue)
                {
                    return nestedCodeHealth;
                }
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                var nestedCodeHealth = TryExtractMonthScore(item);

                if (nestedCodeHealth.HasValue)
                {
                    return nestedCodeHealth;
                }
            }
        }

        return null;
    }

    private sealed record ProjectPage(IReadOnlyList<int> ProjectIds, int MaxPages);

    private sealed record AnalysisPage(IReadOnlyList<AnalysisEntry> Entries, int MaxPages);

    private sealed record AnalysisEntry(int Id, DateTimeOffset AnalysisTime);
}
