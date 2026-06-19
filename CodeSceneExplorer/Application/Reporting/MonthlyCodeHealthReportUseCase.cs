using Microsoft.Extensions.Options;

namespace CodeSceneExplorer.Application.Reporting;

public interface IMonthlyCodeHealthReportUseCase
{
    Task<MonthlyCodeHealthReport> Build(
        DateOnly startInclusive,
        DateOnly endInclusive,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default);
}

public interface IMonthlyCodeHealthSource
{
    Task<IReadOnlyList<MonthlyCodeHealthReading>> GetReadingsAsync(
        DateOnly start,
        DateOnly end,
        CancellationToken cancellationToken = default);
}

public sealed class MonthlyCodeHealthReportUseCase(
    IMonthlyCodeHealthSource source,
    MonthlyPeriodGenerator periodGenerator,
    MonthlyCodeHealthAggregator aggregator,
    IOptions<ReportOptions>? options = null) : IMonthlyCodeHealthReportUseCase
{
    public async Task<MonthlyCodeHealthReport> Build(
        DateOnly startInclusive,
        DateOnly endInclusive,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var periods = periodGenerator.Generate(startInclusive, endInclusive);
        var scoreLimit = options?.Value.ScoreLimit;
        var readings = new List<MonthlyCodeHealthReading>();

        for (var index = 0; index < periods.Count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var period = periods[index];
            progress?.Report($"Processing {period.Label} ({index + 1}/{periods.Count})");

            readings.AddRange(await source.GetReadingsAsync(period.Start, period.End, cancellationToken)
                .ConfigureAwait(false));
        }

        var excludedProjectIds = scoreLimit.HasValue
            ? GetExcludedProjectIds(readings, scoreLimit.Value)
            : [];
        var filteredReadings = readings
            .Where(reading => !excludedProjectIds.Contains(reading.ProjectId))
            .ToList();

        var rows = BuildMonthlyRows(periods, filteredReadings);
        var projectMonthlyReadings = BuildProjectMonthlyReadings(filteredReadings);
        var thresholdCounts = BuildThresholdCounts(periods, projectMonthlyReadings);
        var projectTrends = BuildProjectTrends(projectMonthlyReadings);
        var largestRegressions = projectTrends
            .Where(trend => trend.Delta < 0m)
            .OrderBy(trend => trend.Delta)
            .ThenBy(trend => trend.ProjectId)
            .Take(3)
            .ToList();

        var recentTrendSummary = BuildRecentTrendSummary(periods, projectMonthlyReadings, projectTrends);

        return new MonthlyCodeHealthReport(rows, thresholdCounts, largestRegressions, recentTrendSummary);
    }

    private static HashSet<int> GetExcludedProjectIds(
        IEnumerable<MonthlyCodeHealthReading> readings,
        decimal scoreLimit)
    {
        return readings
            .GroupBy(reading => reading.ProjectId)
            .Where(group => group.All(reading => reading.CodeHealth > scoreLimit))
            .Select(group => group.Key)
            .ToHashSet();
    }

    private IReadOnlyList<MonthlyCodeHealthRow> BuildMonthlyRows(
        IReadOnlyList<MonthlyPeriod> periods,
        IReadOnlyList<MonthlyCodeHealthReading> filteredReadings)
    {
        var rows = new List<MonthlyCodeHealthRow>(periods.Count);

        foreach (var period in periods)
        {
            var periodReadings = filteredReadings
                .Where(reading => reading.YearMonth == period.Label)
                .ToList();

            var monthlyRow = periodReadings.Count == 0
                ? new MonthlyCodeHealthRow(period.Label, 0m, 0m)
                : aggregator.Calculate(periodReadings).Single();

            rows.Add(monthlyRow);
        }

        return rows;
    }

    private static IReadOnlyList<MonthlyCodeHealthThresholdCounts> BuildThresholdCounts(
        IReadOnlyList<MonthlyPeriod> periods,
        IReadOnlyDictionary<int, IReadOnlyList<MonthlyCodeHealthReading>> projectMonthlyReadings)
    {
        return periods
            .Select(period =>
            {
                var periodReadings = projectMonthlyReadings.Values
                    .SelectMany(readings => readings.Where(reading => reading.YearMonth == period.Label));

                return new MonthlyCodeHealthThresholdCounts(
                    period.Label,
                    periodReadings.Count(),
                    periodReadings.Count(reading => reading.CodeHealth < 5m),
                    periodReadings.Count(reading => reading.CodeHealth < 7m),
                    periodReadings.Count(reading => reading.CodeHealth < 8m));
            })
            .ToList();
    }

    private static MonthlyCodeHealthRecentTrendSummary? BuildRecentTrendSummary(
        IReadOnlyList<MonthlyPeriod> periods,
        IReadOnlyDictionary<int, IReadOnlyList<MonthlyCodeHealthReading>> projectMonthlyReadings,
        IReadOnlyList<ProjectCodeHealthTrend> projectTrends)
    {
        if (periods.Count < 3)
        {
            return null;
        }

        var startMonth = periods[^3].Label;
        var endMonth = periods[^1].Label;
        var decliningProjectDetails = projectTrends
            .Where(trend => trend.Delta < 0m)
            .OrderBy(trend => trend.Delta)
            .ThenBy(trend => trend.ProjectId)
            .ToList();

        var declining = 0;
        var improving = 0;
        var stable = 0;

        foreach (var projectReadings in projectMonthlyReadings.Values)
        {
            var startReading = projectReadings.FirstOrDefault(reading => reading.YearMonth == startMonth);
            var endReading = projectReadings.FirstOrDefault(reading => reading.YearMonth == endMonth);

            if (startReading is null || endReading is null)
            {
                continue;
            }

            var delta = endReading.CodeHealth - startReading.CodeHealth;

            if (delta < 0m)
            {
                declining++;
            }
            else if (delta > 0m)
            {
                improving++;
            }
            else
            {
                stable++;
            }
        }

        return new MonthlyCodeHealthRecentTrendSummary(
            startMonth,
            endMonth,
            declining,
            improving,
            stable,
            decliningProjectDetails);
    }

    private static IReadOnlyList<ProjectCodeHealthTrend> BuildProjectTrends(
        IReadOnlyDictionary<int, IReadOnlyList<MonthlyCodeHealthReading>> projectMonthlyReadings)
    {
        return projectMonthlyReadings
            .Select(group =>
            {
                var ordered = group.Value.OrderBy(reading => reading.YearMonth, StringComparer.Ordinal).ToList();
                var start = ordered.First();
                var end = ordered.Last();

                return new ProjectCodeHealthTrend(
                    group.Key,
                    ordered.FirstOrDefault(reading => !string.IsNullOrWhiteSpace(reading.ProjectName))?.ProjectName,
                    ordered,
                    start.CodeHealth,
                    end.CodeHealth);
            })
            .ToList();
    }

    private static IReadOnlyDictionary<int, IReadOnlyList<MonthlyCodeHealthReading>> BuildProjectMonthlyReadings(
        IReadOnlyList<MonthlyCodeHealthReading> filteredReadings)
    {
        return filteredReadings
            .GroupBy(reading => reading.ProjectId)
            .Select(group =>
            {
                var monthlyReadings = group
                    .GroupBy(reading => reading.YearMonth)
                    .OrderBy(monthGroup => monthGroup.Key, StringComparer.Ordinal)
                    .Select(monthGroup =>
                    {
                        var name = monthGroup.FirstOrDefault(reading => !string.IsNullOrWhiteSpace(reading.ProjectName))?.ProjectName;
                        var hotspotReadings = monthGroup
                            .Where(reading => reading.HotspotCodeHealth.HasValue)
                            .Select(reading => reading.HotspotCodeHealth!.Value)
                            .ToList();

                        return new MonthlyCodeHealthReading(
                            monthGroup.Key,
                            Math.Round(monthGroup.Average(reading => reading.CodeHealth), 2),
                            group.Key,
                            name,
                            hotspotReadings.Count == 0 ? null : Math.Round(hotspotReadings.Average(), 2));
                    })
                    .ToList();

                return new KeyValuePair<int, IReadOnlyList<MonthlyCodeHealthReading>>(group.Key, monthlyReadings);
            })
            .ToDictionary(pair => pair.Key, pair => pair.Value);
    }
}
