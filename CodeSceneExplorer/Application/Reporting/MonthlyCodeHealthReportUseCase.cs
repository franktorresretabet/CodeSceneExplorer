namespace CodeSceneExplorer.Application.Reporting;

public interface IMonthlyCodeHealthReportUseCase
{
    Task<IReadOnlyList<MonthlyCodeHealthRow>> Build(
        DateOnly startInclusive,
        DateOnly endInclusive,
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
        CancellationToken cancellationToken = default)
    {
        var readings = new List<MonthlyCodeHealthReading>();

        foreach (var period in periodGenerator.Generate(startInclusive, endInclusive))
        {
            var periodReadings = await source.GetReadingsAsync(period.Start, period.End, cancellationToken)
                .ConfigureAwait(false);

            readings.AddRange(periodReadings);
        }

        return aggregator.Calculate(readings);
    }
}
