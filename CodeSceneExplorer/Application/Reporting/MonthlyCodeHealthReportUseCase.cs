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
    MonthlyCodeHealthAggregator aggregator) : IMonthlyCodeHealthReportUseCase
{
    public async Task<IReadOnlyList<MonthlyCodeHealthRow>> Build(
        DateOnly startInclusive,
        DateOnly endInclusive,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var readings = new List<MonthlyCodeHealthReading>();
        var periods = periodGenerator.Generate(startInclusive, endInclusive);

        for (var index = 0; index < periods.Count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var period = periods[index];
            progress?.Report($"Processing {period.Label} ({index + 1}/{periods.Count})");

            var periodReadings = await source.GetReadingsAsync(period.Start, period.End, cancellationToken)
                .ConfigureAwait(false);

            readings.AddRange(periodReadings);
        }

        return aggregator.Calculate(readings);
    }
}
