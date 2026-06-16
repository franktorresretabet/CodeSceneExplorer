using System.Globalization;
using System.Text;

namespace CodeSceneExplorer.Application.Reporting;

public sealed class MonthlyCodeHealthReportFormatter
{
    public string Format(MonthlyCodeHealthReport report)
    {
        var builder = new StringBuilder();
        AppendMonthlySnapshotRows(builder, report.MonthlyRows, report.ThresholdCounts);

        if (report.RecentTrendSummary is not null)
        {
            AppendRecentTrendSummary(builder, report.RecentTrendSummary);
        }

        if (report.LargestRegressions.Count > 0)
        {
            AppendRegressions(builder, report.LargestRegressions);
        }
        return builder.ToString().TrimEnd();
    }

    public string Format(IEnumerable<MonthlyCodeHealthRow> rows)
    {
        return Format(new MonthlyCodeHealthReport(rows.ToList(), [], [], null));
    }

    private static void AppendMonthlySnapshotRows(
        StringBuilder builder,
        IReadOnlyList<MonthlyCodeHealthRow> rows,
        IReadOnlyList<MonthlyCodeHealthThresholdCounts> thresholdCounts)
    {
        var countsByMonth = thresholdCounts.ToDictionary(counts => counts.YearMonth);
        builder.AppendLine("| year-month | average code health | < 5 | < 7 | < 8 |");
        builder.AppendLine("| --- | ---: | ---: | ---: | ---: |");

        foreach (var row in rows)
        {
            var counts = countsByMonth.TryGetValue(row.YearMonth, out var monthlyCounts)
                ? monthlyCounts
                : new MonthlyCodeHealthThresholdCounts(row.YearMonth, 0, 0, 0);

            builder.AppendLine(
                $"| {row.YearMonth} | {row.AverageCodeHealth.ToString(CultureInfo.InvariantCulture)} | {counts.Below5} | {counts.Below7} | {counts.Below8} |");
        }

        builder.AppendLine();
    }

    private static void AppendRecentTrendSummary(StringBuilder builder, MonthlyCodeHealthRecentTrendSummary summary)
    {
        builder.AppendLine("## Projects declining vs improving in the last 3 months");
        builder.AppendLine();
        builder.AppendLine("| window | declining | improving | stable |");
        builder.AppendLine("| --- | ---: | ---: | ---: |");
        builder.AppendLine($"| {summary.Window} | {summary.DecliningProjects} | {summary.ImprovingProjects} | {summary.StableProjects} |");
        builder.AppendLine();

        if (summary.DecliningProjectDetails.Count > 0)
        {
            builder.AppendLine("### Declining projects");
            builder.AppendLine();
            builder.AppendLine("| project | delta |");
            builder.AppendLine("| --- | ---: |");

            foreach (var trend in summary.DecliningProjectDetails)
            {
                builder.AppendLine($"| {trend.DisplayName} | {trend.Delta.ToString(CultureInfo.InvariantCulture)} |");
            }
        }
    }

    private static void AppendRegressions(StringBuilder builder, IReadOnlyList<ProjectCodeHealthTrend> trends)
    {
        builder.AppendLine("## Largest regressions");
        builder.AppendLine();
        builder.AppendLine("| project | start | end | delta |");
        builder.AppendLine("| --- | ---: | ---: | ---: |");

        foreach (var trend in trends)
        {
            builder.AppendLine(
                $"| {trend.DisplayName} | {trend.StartCodeHealth.ToString(CultureInfo.InvariantCulture)} | {trend.EndCodeHealth.ToString(CultureInfo.InvariantCulture)} | {trend.Delta.ToString(CultureInfo.InvariantCulture)} |");
        }

        foreach (var trend in trends)
        {
            builder.AppendLine();
            builder.AppendLine($"### {trend.DisplayName}");
            builder.AppendLine();
            builder.AppendLine("| year-month | code health |");
            builder.AppendLine("| --- | ---: |");

            foreach (var reading in trend.Readings)
            {
                builder.AppendLine($"| {reading.YearMonth} | {reading.CodeHealth.ToString(CultureInfo.InvariantCulture)} |");
            }
        }
    }
}
