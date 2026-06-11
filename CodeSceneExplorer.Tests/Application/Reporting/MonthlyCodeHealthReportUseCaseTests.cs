using CodeSceneExplorer.Application.Reporting;
using Xunit;

namespace CodeSceneExplorer.Tests.Application.Reporting;

public sealed class MonthlyCodeHealthReportUseCaseTests
{
    [Fact]
    public async Task Build_returns_monthly_average_code_health_rows()
    {
        var source = new FakeMonthlyCodeHealthSource();
        var sut = new MonthlyCodeHealthReportUseCase(
            source,
            new MonthlyPeriodGenerator(),
            new MonthlyCodeHealthAggregator());

        var result = await sut.Build(new DateOnly(2025, 9, 10), new DateOnly(2025, 10, 20));

        Assert.Collection(
            result,
            row =>
            {
                Assert.Equal("2025-09", row.YearMonth);
                Assert.Equal(15m, row.AverageCodeHealth);
            },
            row =>
            {
                Assert.Equal("2025-10", row.YearMonth);
                Assert.Equal(30m, row.AverageCodeHealth);
            });
    }

    private sealed class FakeMonthlyCodeHealthSource : IMonthlyCodeHealthSource
    {
        public Task<IReadOnlyList<MonthlyCodeHealthReading>> GetReadingsAsync(DateOnly start, DateOnly end, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<MonthlyCodeHealthReading> readings =
                start.Month == 9
                    ? [
                        new MonthlyCodeHealthReading("2025-09", 10),
                        new MonthlyCodeHealthReading("2025-09", 20)
                    ]
                    : [
                        new MonthlyCodeHealthReading("2025-10", 30)
                    ];

            return Task.FromResult(readings);
        }
    }
}
