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
            new MonthlyCodeHealthReading("2025-09", 10, HotspotCodeHealth: 7),
            new MonthlyCodeHealthReading("2025-09", 20, HotspotCodeHealth: 11),
            new MonthlyCodeHealthReading("2025-09", 30, HotspotCodeHealth: 13),
            new MonthlyCodeHealthReading("2025-10", 12, HotspotCodeHealth: 9),
            new MonthlyCodeHealthReading("2025-10", 18, HotspotCodeHealth: 15)
        });

        Assert.Collection(
            result,
            row =>
            {
                Assert.Equal("2025-09", row.YearMonth);
                Assert.Equal(20m, row.AverageCodeHealth);
                Assert.Equal(10.33m, row.AverageHotspotCodeHealth);
            },
            row =>
            {
                Assert.Equal("2025-10", row.YearMonth);
                Assert.Equal(15m, row.AverageCodeHealth);
                Assert.Equal(12m, row.AverageHotspotCodeHealth);
            });
    }
}
