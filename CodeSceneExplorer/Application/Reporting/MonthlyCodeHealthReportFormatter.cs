using System.Globalization;
using System.Text;

namespace CodeSceneExplorer.Application.Reporting;

public sealed class MonthlyCodeHealthReportFormatter
{
    public string Format(MonthlyCodeHealthReport report)
    {
        var builder = new StringBuilder();
        AppendMonthlyRows(builder, report.MonthlyRows);
        if (report.TopDecliners.Count > 0)
        {
            AppendDecliners(builder, report.TopDecliners);
        }

        if (report.SmallImprovers.Count > 0)
        {
            AppendSmallImprovers(builder, report.SmallImprovers);
        }
        return builder.ToString().TrimEnd();
    }

    public string Format(IEnumerable<MonthlyCodeHealthRow> rows)
    {
        return Format(new MonthlyCodeHealthReport(rows.ToList(), [], []));
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

    private static void AppendDecliners(StringBuilder builder, IReadOnlyList<ProjectCodeHealthTrend> trends)
    {
        builder.AppendLine("## Top 3 projects that decreased code health");
        builder.AppendLine();
        AppendTrendSummary(builder, trends);

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

    private static void AppendSmallImprovers(StringBuilder builder, IReadOnlyList<ProjectCodeHealthTrend> trends)
    {
        builder.AppendLine();
        builder.AppendLine("## Top 3 projects that improved code health by less than 0.1");
        builder.AppendLine();
        builder.AppendLine("| project | start | end | delta |");
        builder.AppendLine("| --- | ---: | ---: | ---: |");

        foreach (var trend in trends)
        {
            builder.AppendLine(
                $"| {trend.DisplayName} | {trend.StartCodeHealth.ToString(CultureInfo.InvariantCulture)} | {trend.EndCodeHealth.ToString(CultureInfo.InvariantCulture)} | {trend.Delta.ToString(CultureInfo.InvariantCulture)} |");
        }
    }

    private static void AppendTrendSummary(StringBuilder builder, IReadOnlyList<ProjectCodeHealthTrend> trends)
    {
        builder.AppendLine("| project | start | end | delta |");
        builder.AppendLine("| --- | ---: | ---: | ---: |");

        foreach (var trend in trends)
        {
            builder.AppendLine(
                $"| {trend.DisplayName} | {trend.StartCodeHealth.ToString(CultureInfo.InvariantCulture)} | {trend.EndCodeHealth.ToString(CultureInfo.InvariantCulture)} | {trend.Delta.ToString(CultureInfo.InvariantCulture)} |");
        }
    }
}
