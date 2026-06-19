using CodeSceneExplorer.Application.Reporting;
using CodeSceneExplorer.Application.Abstractions;
using CodeSceneExplorer.Domain.Shared;
using CodeSceneExplorer.Infrastructure.CodeScene;
using Microsoft.Extensions.Logging;
using Xunit;

namespace CodeSceneExplorer.Tests.Infrastructure.CodeScene;

public sealed class MonthlyCodeHealthSourceTests
{
    [Fact]
    public async Task GetReadingsAsync_uses_the_latest_analysis_on_or_before_month_end()
    {
        var logger = new CapturingLogger<MonthlyCodeHealthSource>();
        var api = new FakeCodeSceneApi();
        var sut = new MonthlyCodeHealthSource(api, logger);

        var result = await sut.GetReadingsAsync(
            new DateOnly(2025, 10, 1),
            new DateOnly(2025, 10, 31));

        Assert.Collection(
            result,
            reading =>
            {
                Assert.Equal("2025-10", reading.YearMonth);
                Assert.Equal(6.12m, reading.CodeHealth);
                Assert.Equal(4.50m, reading.HotspotCodeHealth);
            },
            reading =>
            {
                Assert.Equal("2025-10", reading.YearMonth);
                Assert.Equal(4.59m, reading.CodeHealth);
                Assert.Equal(3.80m, reading.HotspotCodeHealth);
            });

        Assert.Contains(
            logger.Messages,
            message => message.Contains("KPI sample from", StringComparison.Ordinal)
                && message.Contains("2025-10-15", StringComparison.Ordinal)
                && message.Contains("6.12", StringComparison.Ordinal));
        Assert.Contains(
            logger.Messages,
            message => message.Contains("KPI sample from", StringComparison.Ordinal)
                && message.Contains("2025-09-20", StringComparison.Ordinal)
                && message.Contains("4.59", StringComparison.Ordinal));
    }

    [Fact]
    public async Task GetReadingsAsync_carries_forward_analysis_from_before_the_window()
    {
        // Project only has an analysis from Aug 2025, but the report window starts Sep 2025.
        // The Aug analysis should be carried forward for the Sep month.
        var api = new PreWindowFakeCodeSceneApi();
        var sut = new MonthlyCodeHealthSource(api);

        var result = await sut.GetReadingsAsync(
            new DateOnly(2025, 9, 1),
            new DateOnly(2025, 9, 30));

        Assert.Single(result);
        Assert.Equal("2025-09", result[0].YearMonth);
        Assert.Equal(5.00m, result[0].CodeHealth);
        Assert.Equal(4.25m, result[0].HotspotCodeHealth);
    }

    private sealed class FakeCodeSceneApi : ICodeSceneApi
    {
        public Task<string> ListProjectsAsync(int page = 1, string? orderBy = null, string? filter = null, string? fields = null, CancellationToken cancellationToken = default)
        {
            var payload = page == 1
                ? """
                  {"page":1,"max_pages":1,"projects":[{"id":1},{"id":2}]}
                  """
                : """
                  {"page":2,"max_pages":1,"projects":[]}
                  """;

            return Task.FromResult(payload);
        }

        public Task<string> GetProjectAsync(int projectId, CancellationToken cancellationToken = default) => Task.FromResult("{}");

        public Task<string> ListAnalysesAsync(int projectId, int page = 1, CancellationToken cancellationToken = default) => Task.FromResult("[]");

        public Task<string> GetLatestAnalysisAsync(int projectId, CancellationToken cancellationToken = default) => Task.FromResult("{}");

        public Task<string> GetAnalysisAsync(int projectId, string analysisId, CancellationToken cancellationToken = default) => Task.FromResult("{}");

        public Task<string> ListDeltaAnalysesAsync(int projectId, CancellationToken cancellationToken = default) => Task.FromResult("[]");

        public Task<string> GetDeltaAnalysisAsync(int projectId, int deltaAnalysisId, CancellationToken cancellationToken = default) => Task.FromResult("{}");

        public Task<string> GetTechnicalDebtAsync(int projectId, string analysisId, bool refactoringTargets = false, CancellationToken cancellationToken = default) => Task.FromResult("{}");

        public Task<string> GetCommitActivityTrendAsync(int projectId, string analysisId, CancellationToken cancellationToken = default) => Task.FromResult("{}");

        public Task<string> ListAnalysisCommitsAsync(int projectId, string analysisId, int page = 1, int pageSize = 200, CancellationToken cancellationToken = default) => Task.FromResult("[]");

        public Task<string> ListAnalysisIssuesAsync(int projectId, string analysisId, int page = 1, int pageSize = 200, CancellationToken cancellationToken = default) => Task.FromResult("[]");

        public Task<string> GetKpiTrendCodeHealthAverageAsync(int projectId, DateOnly? start = null, DateOnly? end = null, CancellationToken cancellationToken = default)
        {
            var payload = projectId == 1
                ? """
                  [{"date":"2025-09-15","kpi":6.00},{"date":"2025-10-15","kpi":6.12}]
                  """
                : """
                  [{"date":"2025-09-20","kpi":4.59}]
                  """;

            return Task.FromResult(payload);
        }

        public Task<string> GetKpiTrendHotspotCodeHealthAsync(int projectId, DateOnly? start = null, DateOnly? end = null, CancellationToken cancellationToken = default)
        {
            var payload = projectId == 1
                ? """
                  [{"date":"2025-09-15","kpi":4.00},{"date":"2025-10-15","kpi":4.50}]
                  """
                : """
                  [{"date":"2025-09-20","kpi":3.80}]
                  """;

            return Task.FromResult(payload);
        }

        public Task<string> GetAnalysesByDateAsync(int projectId, DateRange period, CancellationToken cancellationToken = default) => Task.FromResult("{}");
    }

    private sealed class PreWindowFakeCodeSceneApi : ICodeSceneApi
    {
        public Task<string> ListProjectsAsync(int page = 1, string? orderBy = null, string? filter = null, string? fields = null, CancellationToken cancellationToken = default)
        {
            var payload = page == 1
                ? """{"page":1,"max_pages":1,"projects":[{"id":1}]}"""
                : """{"page":2,"max_pages":1,"projects":[]}""";
            return Task.FromResult(payload);
        }

        public Task<string> GetProjectAsync(int projectId, CancellationToken cancellationToken = default) => Task.FromResult("{}");

        public Task<string> ListAnalysesAsync(int projectId, int page = 1, CancellationToken cancellationToken = default) => Task.FromResult("[]");

        public Task<string> GetLatestAnalysisAsync(int projectId, CancellationToken cancellationToken = default) => Task.FromResult("{}");

        public Task<string> GetAnalysisAsync(int projectId, string analysisId, CancellationToken cancellationToken = default) => Task.FromResult("{}");

        public Task<string> ListDeltaAnalysesAsync(int projectId, CancellationToken cancellationToken = default) => Task.FromResult("[]");

        public Task<string> GetDeltaAnalysisAsync(int projectId, int deltaAnalysisId, CancellationToken cancellationToken = default) => Task.FromResult("{}");

        public Task<string> GetTechnicalDebtAsync(int projectId, string analysisId, bool refactoringTargets = false, CancellationToken cancellationToken = default) => Task.FromResult("{}");

        public Task<string> GetCommitActivityTrendAsync(int projectId, string analysisId, CancellationToken cancellationToken = default) => Task.FromResult("{}");

        public Task<string> ListAnalysisCommitsAsync(int projectId, string analysisId, int page = 1, int pageSize = 200, CancellationToken cancellationToken = default) => Task.FromResult("[]");

        public Task<string> ListAnalysisIssuesAsync(int projectId, string analysisId, int page = 1, int pageSize = 200, CancellationToken cancellationToken = default) => Task.FromResult("[]");

        public Task<string> GetKpiTrendCodeHealthAverageAsync(int projectId, DateOnly? start = null, DateOnly? end = null, CancellationToken cancellationToken = default) =>
            Task.FromResult("""[{"date":"2025-08-10","kpi":5.00}]""");

        public Task<string> GetKpiTrendHotspotCodeHealthAsync(int projectId, DateOnly? start = null, DateOnly? end = null, CancellationToken cancellationToken = default) =>
            Task.FromResult("""[{"date":"2025-08-10","kpi":4.25}]""");

        public Task<string> GetAnalysesByDateAsync(int projectId, DateRange period, CancellationToken cancellationToken = default) => Task.FromResult("{}");
    }


    private sealed class CapturingLogger<T> : ILogger<T>
    {
        public List<string> Messages { get; } = [];

        public IDisposable BeginScope<TState>(TState state)
            where TState : notnull
        {
            return NullScope.Instance;
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Messages.Add(formatter(state, exception));
        }
    }

    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();

        public void Dispose()
        {
        }
    }
}
