using CodeSceneExplorer.Application.Reporting;
using Xunit;

namespace CodeSceneExplorer.Tests.Application.Reporting;

public sealed class MonthlyCodeHealthReportFormatterTests
{
    [Fact]
    public void Format_returns_a_markdown_table()
    {
        var sut = new MonthlyCodeHealthReportFormatter();

        var result = sut.Format(new[]
        {
            new MonthlyCodeHealthRow("2025-09", 15m),
            new MonthlyCodeHealthRow("2025-10", 30m)
        });

        Assert.Equal("""
| year-month | average code health |
| --- | ---: |
| 2025-09 | 15 |
| 2025-10 | 30 |
""", result);
    }

    [Fact]
    public void Format_includes_decliners_and_small_improvers()
    {
        var sut = new MonthlyCodeHealthReportFormatter();
        var report = new MonthlyCodeHealthReport(
            [
                new MonthlyCodeHealthRow("2025-09", 15m),
                new MonthlyCodeHealthRow("2025-10", 30m)
            ],
            [
                new ProjectCodeHealthTrend(
                    1,
                    "Project A",
                    [
                        new MonthlyCodeHealthReading("2025-09", 9m, 1, "Project A"),
                        new MonthlyCodeHealthReading("2025-10", 7m, 1, "Project A")
                    ],
                    9m,
                    7m)
            ],
            [
                new ProjectCodeHealthTrend(
                    2,
                    "Project B",
                    [
                        new MonthlyCodeHealthReading("2025-09", 6m, 2, "Project B"),
                        new MonthlyCodeHealthReading("2025-10", 6.05m, 2, "Project B")
                    ],
                    6m,
                    6.05m)
            ]);

        var result = sut.Format(report);

        Assert.Contains("## Top 3 projects that decreased code health", result);
        Assert.Contains("### Project A (1)", result);
        Assert.Contains("## Top 3 projects that improved code health by less than 0.1", result);
        Assert.Contains("| Project B (2) | 6 | 6.05 | 0.05 |", result);
    }
}
