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

    [Fact]
    public async Task Build_reports_progress_for_each_month()
    {
        var source = new FakeMonthlyCodeHealthSource();
        var sut = new MonthlyCodeHealthReportUseCase(
            source,
            new MonthlyPeriodGenerator(),
            new MonthlyCodeHealthAggregator());
        var progressMessages = new List<string>();
        var progress = new InlineProgress(progressMessages);

        var result = await sut.Build(
            new DateOnly(2025, 9, 10),
            new DateOnly(2025, 10, 20),
            progress: progress);

        Assert.NotEmpty(result);
        Assert.Equal([
            "Processing 2025-09 (1/2)",
            "Processing 2025-10 (2/2)"
        ], progressMessages);
    }

    [Fact]
    public async Task Build_stops_when_cancellation_is_requested()
    {
        var source = new CancelAwareMonthlyCodeHealthSource();
        var sut = new MonthlyCodeHealthReportUseCase(
            source,
            new MonthlyPeriodGenerator(),
            new MonthlyCodeHealthAggregator());
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() => sut.Build(
            new DateOnly(2025, 9, 10),
            new DateOnly(2025, 10, 20),
            cancellationToken: cancellationTokenSource.Token));

        Assert.False(source.WasCalled);
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

    private sealed class CancelAwareMonthlyCodeHealthSource : IMonthlyCodeHealthSource
    {
        public bool WasCalled { get; private set; }

        public Task<IReadOnlyList<MonthlyCodeHealthReading>> GetReadingsAsync(
            DateOnly start,
            DateOnly end,
            CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            return Task.FromResult<IReadOnlyList<MonthlyCodeHealthReading>>([]);
        }
    }

    private sealed class InlineProgress(List<string> messages) : IProgress<string>
    {
        public void Report(string value) => messages.Add(value);
    }
}
