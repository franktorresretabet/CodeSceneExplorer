using CodeSceneExplorer.Application.Reporting;
using Microsoft.Extensions.Options;
using Xunit;

namespace CodeSceneExplorer.Tests.Application.Reporting;

public sealed class MonthlyCodeHealthReportRunnerTests
{
    [Fact]
    public async Task Build_uses_september_last_year_as_the_start_date_when_no_config()
    {
        var useCase = new FakeMonthlyCodeHealthReportUseCase();
        var sut = new MonthlyCodeHealthReportRunner(useCase, new MonthlyCodeHealthReportFormatter(), Options.Create(new ReportOptions()));

        var result = await sut.Build(new DateOnly(2026, 6, 11));

        Assert.Equal(new DateOnly(2025, 9, 1), useCase.Start);
        Assert.Equal(new DateOnly(2026, 6, 11), useCase.End);
        Assert.Equal("""
| year-month | average code health | projects | < 5 | < 7 | < 8 |
| ---: | ---: | ---: | ---: | ---: | ---: |
| 2025-09 | 15 | 0 | 0 | 0 | 0 |
| 2025-10 | 30 | 0 | 0 | 0 | 0 |
""", result);
    }

    [Fact]
    public async Task Build_uses_configured_start_date_when_set()
    {
        var useCase = new FakeMonthlyCodeHealthReportUseCase();
        var sut = new MonthlyCodeHealthReportRunner(
            useCase,
            new MonthlyCodeHealthReportFormatter(),
            Options.Create(new ReportOptions { StartDate = "2026-05-01" }));

        await sut.Build(new DateOnly(2026, 6, 11));

        Assert.Equal(new DateOnly(2026, 5, 1), useCase.Start);
    }

    private sealed class FakeMonthlyCodeHealthReportUseCase : IMonthlyCodeHealthReportUseCase
    {
        public DateOnly? Start { get; private set; }

        public DateOnly? End { get; private set; }

        public Task<MonthlyCodeHealthReport> Build(
            DateOnly startInclusive,
            DateOnly endInclusive,
            IProgress<string>? progress = null,
            CancellationToken cancellationToken = default)
        {
            Start = startInclusive;
            End = endInclusive;

            var report = new MonthlyCodeHealthReport(
                [
                    new MonthlyCodeHealthRow("2025-09", 15m),
                    new MonthlyCodeHealthRow("2025-10", 30m)
                ],
                [],
                [],
                null);

            return Task.FromResult(report);
        }
    }
}
