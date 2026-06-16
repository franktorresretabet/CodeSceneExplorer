namespace CodeSceneExplorer.Application.Reporting;

public sealed record MonthlyCodeHealthReading(string YearMonth, decimal CodeHealth, int ProjectId = 0);

public sealed record MonthlyCodeHealthRow(string YearMonth, decimal AverageCodeHealth);

public sealed class MonthlyCodeHealthAggregator
{
    public IReadOnlyList<MonthlyCodeHealthRow> Calculate(IEnumerable<MonthlyCodeHealthReading> readings)
    {
        return readings
            .GroupBy(reading => reading.YearMonth)
            .OrderBy(group => group.Key)
            .Select(group => new MonthlyCodeHealthRow(
                group.Key,
                Math.Round(group.Average(reading => reading.CodeHealth), 2)))
            .ToList();
    }
}
