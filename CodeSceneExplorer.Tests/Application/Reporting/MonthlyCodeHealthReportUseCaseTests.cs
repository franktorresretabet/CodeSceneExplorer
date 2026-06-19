using CodeSceneExplorer.Application.Reporting;
using Microsoft.Extensions.Options;
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
            result.MonthlyRows,
            row =>
            {
                Assert.Equal("2025-09", row.YearMonth);
                Assert.Equal(15m, row.AverageCodeHealth);
                Assert.Equal(0m, row.AverageHotspotCodeHealth);
            },
            row =>
            {
                Assert.Equal("2025-10", row.YearMonth);
                Assert.Equal(30m, row.AverageCodeHealth);
                Assert.Equal(0m, row.AverageHotspotCodeHealth);
            });
    }

    [Fact]
    public async Task Build_keeps_months_even_when_a_month_has_no_readings()
    {
        var source = new MissingMonthMonthlyCodeHealthSource();
        var sut = new MonthlyCodeHealthReportUseCase(
            source,
            new MonthlyPeriodGenerator(),
            new MonthlyCodeHealthAggregator());

        var result = await sut.Build(new DateOnly(2025, 9, 10), new DateOnly(2025, 10, 20));

        Assert.Collection(
            result.MonthlyRows,
            row =>
            {
                Assert.Equal("2025-09", row.YearMonth);
                Assert.Equal(15m, row.AverageCodeHealth);
                Assert.Equal(0m, row.AverageHotspotCodeHealth);
            },
            row =>
            {
                Assert.Equal("2025-10", row.YearMonth);
                Assert.Equal(0m, row.AverageCodeHealth);
                Assert.Equal(0m, row.AverageHotspotCodeHealth);
            });
    }

    [Fact]
    public async Task Build_excludes_projects_that_are_always_above_the_score_limit()
    {
        var source = new ScoreLimitMonthlyCodeHealthSource();
        var sut = new MonthlyCodeHealthReportUseCase(
            source,
            new MonthlyPeriodGenerator(),
            new MonthlyCodeHealthAggregator(),
            Options.Create(new ReportOptions { ScoreLimit = 8m }));

        var result = await sut.Build(new DateOnly(2025, 9, 10), new DateOnly(2025, 10, 20));

        Assert.Collection(
            result.MonthlyRows,
            row =>
            {
                Assert.Equal("2025-09", row.YearMonth);
                Assert.Equal(6m, row.AverageCodeHealth);
                Assert.Equal(0m, row.AverageHotspotCodeHealth);
            },
            row =>
            {
                Assert.Equal("2025-10", row.YearMonth);
                Assert.Equal(7m, row.AverageCodeHealth);
                Assert.Equal(0m, row.AverageHotspotCodeHealth);
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

        Assert.NotEmpty(result.MonthlyRows);
        Assert.Equal([
            "Processing 2025-09 (1/2)",
            "Processing 2025-10 (2/2)"
        ], progressMessages);
    }

    [Fact]
    public async Task Build_includes_top_decliners_and_small_improvers()
    {
        var source = new TrendMonthlyCodeHealthSource();
        var sut = new MonthlyCodeHealthReportUseCase(
            source,
            new MonthlyPeriodGenerator(),
            new MonthlyCodeHealthAggregator());

        var result = await sut.Build(new DateOnly(2025, 9, 10), new DateOnly(2025, 11, 20));

        Assert.Collection(
            result.LargestRegressions,
            trend =>
            {
                Assert.Equal("Project 1 (1)", trend.DisplayName);
                Assert.Equal(9m, trend.StartCodeHealth);
                Assert.Equal(8m, trend.EndCodeHealth);
            },
            trend =>
            {
                Assert.Equal("Project 3 (3)", trend.DisplayName);
                Assert.Equal(7m, trend.StartCodeHealth);
                Assert.Equal(6m, trend.EndCodeHealth);
            },
            trend =>
            {
                Assert.Equal("Project 2 (2)", trend.DisplayName);
                Assert.Equal(8m, trend.StartCodeHealth);
                Assert.Equal(7.5m, trend.EndCodeHealth);
            });

        Assert.Collection(
            result.ThresholdCounts,
            row =>
            {
                Assert.Equal("2025-09", row.YearMonth);
                Assert.Equal(0, row.Below5);
                Assert.Equal(2, row.Below7);
                Assert.Equal(4, row.Below8);
            },
            row =>
            {
                Assert.Equal("2025-10", row.YearMonth);
                Assert.Equal(0, row.Below5);
                Assert.Equal(4, row.Below7);
                Assert.Equal(6, row.Below8);
            },
            row =>
            {
                Assert.Equal("2025-11", row.YearMonth);
                Assert.Equal(0, row.Below5);
                Assert.Equal(3, row.Below7);
                Assert.Equal(5, row.Below8);
            });

        Assert.NotNull(result.RecentTrendSummary);
        Assert.Equal("2025-09 to 2025-11", result.RecentTrendSummary!.Window);
        Assert.Equal(3, result.RecentTrendSummary.DecliningProjects);
        Assert.Equal(3, result.RecentTrendSummary.ImprovingProjects);
        Assert.Equal(0, result.RecentTrendSummary.StableProjects);
        Assert.Equal(3, result.RecentTrendSummary.DecliningProjectDetails.Count);
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

    private sealed class MissingMonthMonthlyCodeHealthSource : IMonthlyCodeHealthSource
    {
        public Task<IReadOnlyList<MonthlyCodeHealthReading>> GetReadingsAsync(DateOnly start, DateOnly end, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<MonthlyCodeHealthReading> readings =
                start.Month == 9
                    ? [new MonthlyCodeHealthReading("2025-09", 15)]
                    : [];

            return Task.FromResult(readings);
        }
    }

    private sealed class ScoreLimitMonthlyCodeHealthSource : IMonthlyCodeHealthSource
    {
        public Task<IReadOnlyList<MonthlyCodeHealthReading>> GetReadingsAsync(
            DateOnly start,
            DateOnly end,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<MonthlyCodeHealthReading> readings =
                start.Month == 9
                    ? [
                        new MonthlyCodeHealthReading("2025-09", 6m, 1),
                        new MonthlyCodeHealthReading("2025-09", 9m, 2)
                    ]
                    : [
                        new MonthlyCodeHealthReading("2025-10", 7m, 1),
                        new MonthlyCodeHealthReading("2025-10", 9.5m, 2)
                    ];

            return Task.FromResult(readings);
        }
    }

    private sealed class TrendMonthlyCodeHealthSource : IMonthlyCodeHealthSource
    {
        public Task<IReadOnlyList<MonthlyCodeHealthReading>> GetReadingsAsync(
            DateOnly start,
            DateOnly end,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<MonthlyCodeHealthReading> readings = start.Month switch
            {
                9 => [
                    new MonthlyCodeHealthReading("2025-09", 9m, 1, "Project 1"),
                    new MonthlyCodeHealthReading("2025-09", 8m, 2, "Project 2"),
                    new MonthlyCodeHealthReading("2025-09", 7m, 3, "Project 3"),
                    new MonthlyCodeHealthReading("2025-09", 6m, 4, "Project 4"),
                    new MonthlyCodeHealthReading("2025-09", 7m, 5, "Project 5"),
                    new MonthlyCodeHealthReading("2025-09", 6m, 6, "Project 6")
                ],
                10 => [
                    new MonthlyCodeHealthReading("2025-10", 7m, 1, "Project 1"),
                    new MonthlyCodeHealthReading("2025-10", 6.5m, 2, "Project 2"),
                    new MonthlyCodeHealthReading("2025-10", 5m, 3, "Project 3"),
                    new MonthlyCodeHealthReading("2025-10", 6.05m, 4, "Project 4"),
                    new MonthlyCodeHealthReading("2025-10", 7.07m, 5, "Project 5"),
                    new MonthlyCodeHealthReading("2025-10", 6.2m, 6, "Project 6")
                ],
                _ => [
                    new MonthlyCodeHealthReading("2025-11", 8m, 1, "Project 1"),
                    new MonthlyCodeHealthReading("2025-11", 7.5m, 2, "Project 2"),
                    new MonthlyCodeHealthReading("2025-11", 6m, 3, "Project 3"),
                    new MonthlyCodeHealthReading("2025-11", 6.1m, 4, "Project 4"),
                    new MonthlyCodeHealthReading("2025-11", 7.2m, 5, "Project 5"),
                    new MonthlyCodeHealthReading("2025-11", 6.5m, 6, "Project 6")
                ]
            };

            return Task.FromResult(readings);
        }
    }

    private sealed class InlineProgress(List<string> messages) : IProgress<string>
    {
        public void Report(string value) => messages.Add(value);
    }
}
