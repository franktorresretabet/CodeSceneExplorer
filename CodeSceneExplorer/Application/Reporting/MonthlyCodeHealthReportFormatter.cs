using System.Globalization;
using System.Text;

namespace CodeSceneExplorer.Application.Reporting;

public sealed class MonthlyCodeHealthReportFormatter
{
    public string Format(IEnumerable<MonthlyCodeHealthRow> rows)
    {
        var builder = new StringBuilder();
        builder.AppendLine("| year-month | average code health |");
        builder.AppendLine("| --- | ---: |");

        foreach (var row in rows)
        {
            builder.AppendLine($"| {row.YearMonth} | {row.AverageCodeHealth.ToString(CultureInfo.InvariantCulture)} |");
        }

        return builder.ToString().TrimEnd();
    }
}
