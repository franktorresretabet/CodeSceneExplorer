namespace CodeSceneExplorer.Application.Reporting;

public sealed record MonthlyCodeHealthReading(
    string YearMonth,
    decimal CodeHealth,
    int ProjectId = 0,
    string? ProjectName = null,
    decimal? HotspotCodeHealth = null);

public sealed record MonthlyCodeHealthRow(
    string YearMonth,
    decimal AverageCodeHealth,
    decimal AverageHotspotCodeHealth);

public sealed class MonthlyCodeHealthAggregator
{
    public IReadOnlyList<MonthlyCodeHealthRow> Calculate(IEnumerable<MonthlyCodeHealthReading> readings)
    {
        return readings
            .GroupBy(reading => reading.YearMonth)
            .OrderBy(group => group.Key)
            .Select(group => new MonthlyCodeHealthRow(
                group.Key,
                Math.Round(group.Average(reading => reading.CodeHealth), 2),
                CalculateHotspotAverage(group)))
            .ToList();
    }

    private static decimal CalculateHotspotAverage(IEnumerable<MonthlyCodeHealthReading> readings)
    {
        var hotspotReadings = readings
            .Where(reading => reading.HotspotCodeHealth.HasValue)
            .Select(reading => reading.HotspotCodeHealth!.Value)
            .ToList();

        return hotspotReadings.Count == 0
            ? 0m
            : Math.Round(hotspotReadings.Average(), 2);
    }
}
