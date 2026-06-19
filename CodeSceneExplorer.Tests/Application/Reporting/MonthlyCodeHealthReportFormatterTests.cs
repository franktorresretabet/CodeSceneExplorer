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
            new MonthlyCodeHealthRow("2025-09", 15m, 12m),
            new MonthlyCodeHealthRow("2025-10", 30m, 24m)
        });

        Assert.Equal("""
| year-month | average code health | average hotspot code health | projects | < 5 | < 7 | < 8 |
| ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| 2025-09 | 15 | 12 | 0 | 0 | 0 | 0 |
| 2025-10 | 30 | 24 | 0 | 0 | 0 | 0 |
""", result);
    }

    [Fact]
    public void Format_combines_monthly_average_and_threshold_counts()
    {
        var sut = new MonthlyCodeHealthReportFormatter();
        var report = new MonthlyCodeHealthReport(
            [
                new MonthlyCodeHealthRow("2025-09", 15m, 12m),
                new MonthlyCodeHealthRow("2025-10", 30m, 24m)
            ],
            [
                new MonthlyCodeHealthThresholdCounts("2025-09", 10, 1, 2, 3),
                new MonthlyCodeHealthThresholdCounts("2025-10", 11, 4, 5, 6)
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
            new MonthlyCodeHealthRecentTrendSummary(
                "2025-07",
                "2025-10",
                4,
                5,
                6,
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
                ]));

        var result = sut.Format(report);

        Assert.Contains("| 2025-09 | 15 | 12 | 10 | 1 | 2 | 3 |", result);
        Assert.Contains("| 2025-10 | 30 | 24 | 11 | 4 | 5 | 6 |", result);
        Assert.Contains("## Projects declining vs improving in the last 3 months", result);
        Assert.Contains("| 2025-07 to 2025-10 | 4 | 5 | 6 |", result);
        Assert.Contains("### Declining projects", result);
        Assert.Contains("| Project A (1) | -2 |", result);
        Assert.Contains("### Project A (1)", result);
    }
}
