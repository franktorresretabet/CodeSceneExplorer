using System.Text.Json;
using CodeSceneExplorer.Application.Abstractions;
using CodeSceneExplorer.Application.Reporting;
using CodeSceneExplorer.Domain.Shared;

namespace CodeSceneExplorer.Infrastructure.CodeScene;

public sealed class MonthlyCodeHealthSource(ICodeSceneApi api) : IMonthlyCodeHealthSource
{
    public async Task<IReadOnlyList<MonthlyCodeHealthReading>> GetReadingsAsync(
        DateOnly start,
        DateOnly end,
        CancellationToken cancellationToken = default)
    {
        var readings = new List<MonthlyCodeHealthReading>();
        var period = DateRange.Create(start, end);

        foreach (var projectId in await ListProjectIdsAsync(cancellationToken).ConfigureAwait(false))
        {
            var analysisId = await GetLatestAnalysisIdAsync(projectId, period, cancellationToken).ConfigureAwait(false);
            if (analysisId is null)
            {
                continue;
            }

            var response = await api.GetAnalysisAsync(projectId, analysisId.Value.ToString(), cancellationToken)
                .ConfigureAwait(false);
            var codeHealth = TryExtractMonthScore(response);

            if (codeHealth.HasValue)
            {
                readings.Add(new MonthlyCodeHealthReading(start.ToString("yyyy-MM"), codeHealth.Value));
            }
        }

        return readings;
    }

    private async Task<IReadOnlyList<int>> ListProjectIdsAsync(CancellationToken cancellationToken)
    {
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

        return projectIds;
    }

    private async Task<int?> GetLatestAnalysisIdAsync(
        int projectId,
        DateRange period,
        CancellationToken cancellationToken)
    {
        var response = await api.ListAnalysesAsync(projectId, cancellationToken).ConfigureAwait(false);
        var analyses = ParseAnalyses(response);

        return analyses
            .Where(analysis =>
            {
                var analysisDate = DateOnly.FromDateTime(analysis.AnalysisTime.UtcDateTime);
                return analysisDate >= period.From && analysisDate <= period.To;
            })
            .OrderByDescending(analysis => analysis.AnalysisTime)
            .Select(analysis => (int?)analysis.Id)
            .FirstOrDefault();
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

    private static IReadOnlyList<AnalysisEntry> ParseAnalyses(string response)
    {
        using var document = JsonDocument.Parse(response);

        if (!document.RootElement.TryGetProperty("analyses", out var analyses) || analyses.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var analysisEntries = new List<AnalysisEntry>();

        foreach (var item in analyses.EnumerateArray())
        {
            if (item.TryGetProperty("id", out var idProperty)
                && idProperty.TryGetInt32(out var analysisId)
                && item.TryGetProperty("analysistime", out var timeProperty)
                && DateTimeOffset.TryParse(timeProperty.GetString(), out var analysisTime))
            {
                analysisEntries.Add(new AnalysisEntry(analysisId, analysisTime));
            }
        }

        return analysisEntries;
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

    private sealed record AnalysisEntry(int Id, DateTimeOffset AnalysisTime);
}
