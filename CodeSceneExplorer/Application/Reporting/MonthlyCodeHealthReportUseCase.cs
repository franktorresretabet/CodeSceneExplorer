using Microsoft.Extensions.Options;

namespace CodeSceneExplorer.Application.Reporting;

public interface IMonthlyCodeHealthReportUseCase
{
    Task<IReadOnlyList<MonthlyCodeHealthRow>> Build(
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
    public async Task<IReadOnlyList<MonthlyCodeHealthRow>> Build(
        DateOnly startInclusive,
        DateOnly endInclusive,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var periods = periodGenerator.Generate(startInclusive, endInclusive);
        var scoreLimit = options?.Value.ScoreLimit;
        var readings = new List<MonthlyCodeHealthReading>();
        var rows = new List<MonthlyCodeHealthRow>(periods.Count);

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
        var filteredReadings = readings.Where(reading => !excludedProjectIds.Contains(reading.ProjectId));

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
}
