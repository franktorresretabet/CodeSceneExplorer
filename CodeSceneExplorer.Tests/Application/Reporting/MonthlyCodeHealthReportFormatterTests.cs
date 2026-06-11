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
}
