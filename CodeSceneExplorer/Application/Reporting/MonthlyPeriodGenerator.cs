namespace CodeSceneExplorer.Application.Reporting;

public sealed record MonthlyPeriod(DateOnly Start, DateOnly End, string Label);

public sealed class MonthlyPeriodGenerator
{
    public IReadOnlyList<MonthlyPeriod> Generate(DateOnly startInclusive, DateOnly endInclusive)
    {
        var periods = new List<MonthlyPeriod>();
        var current = new DateOnly(startInclusive.Year, startInclusive.Month, 1);
        var finalMonth = new DateOnly(endInclusive.Year, endInclusive.Month, 1);

        while (current <= finalMonth)
        {
            var monthEnd = current.AddMonths(1).AddDays(-1);
            var periodStart = current;
            var periodEnd = monthEnd > endInclusive ? endInclusive : monthEnd;

            periods.Add(new MonthlyPeriod(periodStart, periodEnd, current.ToString("yyyy-MM")));
            current = current.AddMonths(1);
        }

        return periods;
    }
}
