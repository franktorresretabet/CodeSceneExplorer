using System.Globalization;
using System.Text;

namespace CodeSceneExplorer.Application.Reporting;

public sealed class MonthlyCodeHealthReportFormatter
{
    public string Format(MonthlyCodeHealthReport report)
    {
        var builder = new StringBuilder();
        AppendMonthlyRows(builder, report.MonthlyRows);
        if (report.ThresholdCounts.Count > 0)
        {
            AppendThresholdCounts(builder, report.ThresholdCounts);
        }

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

    private static void AppendMonthlyRows(StringBuilder builder, IReadOnlyList<MonthlyCodeHealthRow> rows)
    {
        builder.AppendLine("| year-month | average code health |");
        builder.AppendLine("| --- | ---: |");

        foreach (var row in rows)
        {
            builder.AppendLine($"| {row.YearMonth} | {row.AverageCodeHealth.ToString(CultureInfo.InvariantCulture)} |");
        }

        builder.AppendLine();
    }

    private static void AppendThresholdCounts(StringBuilder builder, IReadOnlyList<MonthlyCodeHealthThresholdCounts> counts)
    {
        builder.AppendLine("## Projects below code-health thresholds");
        builder.AppendLine();
        builder.AppendLine("| year-month | < 5 | < 7 | < 8 |");
        builder.AppendLine("| --- | ---: | ---: | ---: |");

        foreach (var row in counts)
        {
            builder.AppendLine(
                $"| {row.YearMonth} | {row.Below5} | {row.Below7} | {row.Below8} |");
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
