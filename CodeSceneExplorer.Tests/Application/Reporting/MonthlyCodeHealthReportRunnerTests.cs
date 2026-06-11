using CodeSceneExplorer.Application.Reporting;
using Xunit;

namespace CodeSceneExplorer.Tests.Application.Reporting;

public sealed class MonthlyCodeHealthReportRunnerTests
{
    [Fact]
    public async Task Build_uses_september_last_year_as_the_start_date()
    {
        var useCase = new FakeMonthlyCodeHealthReportUseCase();
        var sut = new MonthlyCodeHealthReportRunner(useCase, new MonthlyCodeHealthReportFormatter());

        var result = await sut.Build(new DateOnly(2026, 6, 11));

        Assert.Equal(new DateOnly(2025, 9, 1), useCase.Start);
        Assert.Equal(new DateOnly(2026, 6, 11), useCase.End);
        Assert.Equal("""
| year-month | average code health |
| --- | ---: |
| 2025-09 | 15 |
| 2025-10 | 30 |
""", result);
    }

    private sealed class FakeMonthlyCodeHealthReportUseCase : IMonthlyCodeHealthReportUseCase
    {
        public DateOnly? Start { get; private set; }

        public DateOnly? End { get; private set; }

        public Task<IReadOnlyList<MonthlyCodeHealthRow>> Build(
            DateOnly startInclusive,
            DateOnly endInclusive,
            CancellationToken cancellationToken = default)
        {
            Start = startInclusive;
            End = endInclusive;

            IReadOnlyList<MonthlyCodeHealthRow> rows =
            [
                new MonthlyCodeHealthRow("2025-09", 15m),
                new MonthlyCodeHealthRow("2025-10", 30m)
            ];

            return Task.FromResult(rows);
        }
    }
}
