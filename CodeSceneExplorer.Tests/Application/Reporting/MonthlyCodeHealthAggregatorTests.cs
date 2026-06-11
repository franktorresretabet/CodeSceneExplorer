using CodeSceneExplorer.Application.Reporting;
using Xunit;

namespace CodeSceneExplorer.Tests.Application.Reporting;

public sealed class MonthlyCodeHealthAggregatorTests
{
    [Fact]
    public void Calculate_groups_readings_by_month_and_averages_code_health()
    {
        var sut = new MonthlyCodeHealthAggregator();

        var result = sut.Calculate(new[]
        {
            new MonthlyCodeHealthReading("2025-09", 10),
            new MonthlyCodeHealthReading("2025-09", 20),
            new MonthlyCodeHealthReading("2025-09", 30),
            new MonthlyCodeHealthReading("2025-10", 12),
            new MonthlyCodeHealthReading("2025-10", 18)
        });

        Assert.Collection(
            result,
            row =>
            {
                Assert.Equal("2025-09", row.YearMonth);
                Assert.Equal(20m, row.AverageCodeHealth);
            },
            row =>
            {
                Assert.Equal("2025-10", row.YearMonth);
                Assert.Equal(15m, row.AverageCodeHealth);
            });
    }
}
