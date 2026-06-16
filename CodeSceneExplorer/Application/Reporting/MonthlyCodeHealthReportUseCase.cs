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
        var projectTrends = BuildProjectTrends(filteredReadings);

        var topDecliners = projectTrends
            .Where(trend => trend.Delta < 0m)
            .OrderBy(trend => trend.Delta)
            .ThenBy(trend => trend.ProjectId)
            .Take(3)
            .ToList();

        var smallImprovers = projectTrends
            .Where(trend => trend.Delta > 0m && trend.Delta < 0.1m)
            .OrderBy(trend => trend.Delta)
            .ThenBy(trend => trend.ProjectId)
            .Take(3)
            .ToList();

        return new MonthlyCodeHealthReport(rows, topDecliners, smallImprovers);
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

            var averageCodeHealth = periodReadings.Count == 0
                ? 0m
                : aggregator.Calculate(periodReadings).Single().AverageCodeHealth;

            rows.Add(new MonthlyCodeHealthRow(period.Label, averageCodeHealth));
        }

        return rows;
    }

    private static IReadOnlyList<ProjectCodeHealthTrend> BuildProjectTrends(
        IReadOnlyList<MonthlyCodeHealthReading> filteredReadings)
    {
        return filteredReadings
            .GroupBy(reading => reading.ProjectId)
            .Select(group =>
            {
                var ordered = group.OrderBy(reading => reading.YearMonth, StringComparer.Ordinal).ToList();
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
}
