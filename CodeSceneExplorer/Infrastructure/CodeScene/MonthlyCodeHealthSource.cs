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
            var response = await api.GetAnalysesByDateAsync(projectId, period, cancellationToken).ConfigureAwait(false);
            var codeHealth = TryExtractCodeHealth(response);

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

        while (true)
        {
            var response = await api.ListProjectsAsync(page: page, fields: "id", cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            var currentPageProjectIds = ExtractProjectIds(response);

            if (currentPageProjectIds.Count == 0)
            {
                break;
            }

            projectIds.AddRange(currentPageProjectIds);
            page++;
        }

        return projectIds;
    }

    private static IReadOnlyList<int> ExtractProjectIds(string response)
    {
        using var document = JsonDocument.Parse(response);

        if (!document.RootElement.TryGetProperty("items", out var items) || items.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var projectIds = new List<int>();

        foreach (var item in items.EnumerateArray())
        {
            if (item.TryGetProperty("id", out var idProperty) && idProperty.TryGetInt32(out var projectId))
            {
                projectIds.Add(projectId);
            }
        }

        return projectIds;
    }

    private static decimal? TryExtractCodeHealth(string response)
    {
        using var document = JsonDocument.Parse(response);
        return TryExtractCodeHealth(document.RootElement);
    }

    private static decimal? TryExtractCodeHealth(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            if (element.TryGetProperty("now", out var nowProperty) && nowProperty.TryGetDecimal(out var codeHealth))
            {
                return codeHealth;
            }

            foreach (var property in element.EnumerateObject())
            {
                var nestedCodeHealth = TryExtractCodeHealth(property.Value);

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
                var nestedCodeHealth = TryExtractCodeHealth(item);

                if (nestedCodeHealth.HasValue)
                {
                    return nestedCodeHealth;
                }
            }
        }

        return null;
    }
}
