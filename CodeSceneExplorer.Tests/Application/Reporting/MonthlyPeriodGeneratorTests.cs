using CodeSceneExplorer.Application.Reporting;
using Xunit;

namespace CodeSceneExplorer.Tests.Application.Reporting;

public sealed class MonthlyPeriodGeneratorTests
{
    [Fact]
    public void Generate_returns_monthly_periods_from_start_to_end_inclusive()
    {
        var sut = new MonthlyPeriodGenerator();

        var result = sut.Generate(new DateOnly(2025, 9, 10), new DateOnly(2025, 11, 2));

        Assert.Collection(
            result,
            period =>
            {
                Assert.Equal("2025-09", period.Label);
                Assert.Equal(new DateOnly(2025, 9, 1), period.Start);
                Assert.Equal(new DateOnly(2025, 9, 30), period.End);
            },
            period =>
            {
                Assert.Equal("2025-10", period.Label);
                Assert.Equal(new DateOnly(2025, 10, 1), period.Start);
                Assert.Equal(new DateOnly(2025, 10, 31), period.End);
            },
            period =>
            {
                Assert.Equal("2025-11", period.Label);
                Assert.Equal(new DateOnly(2025, 11, 1), period.Start);
                Assert.Equal(new DateOnly(2025, 11, 2), period.End);
            });
    }
}
