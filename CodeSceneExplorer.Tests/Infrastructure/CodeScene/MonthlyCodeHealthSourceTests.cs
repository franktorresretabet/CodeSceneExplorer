using CodeSceneExplorer.Application.Reporting;
using CodeSceneExplorer.Application.Abstractions;
using CodeSceneExplorer.Domain.Shared;
using CodeSceneExplorer.Infrastructure.CodeScene;
using Xunit;

namespace CodeSceneExplorer.Tests.Infrastructure.CodeScene;

public sealed class MonthlyCodeHealthSourceTests
{
    [Fact]
    public async Task GetReadingsAsync_returns_code_health_for_each_project_in_the_requested_month()
    {
        var api = new FakeCodeSceneApi();
        var sut = new MonthlyCodeHealthSource(api);

        var result = await sut.GetReadingsAsync(
            new DateOnly(2025, 9, 1),
            new DateOnly(2025, 9, 30));

        Assert.Collection(
            result,
            reading =>
            {
                Assert.Equal("2025-09", reading.YearMonth);
                Assert.Equal(4.59m, reading.CodeHealth);
            },
            reading =>
            {
                Assert.Equal("2025-09", reading.YearMonth);
                Assert.Equal(6.12m, reading.CodeHealth);
            });
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

        public Task<string> ListAnalysesAsync(int projectId, CancellationToken cancellationToken = default)
        {
            var payload = projectId == 1
                ? """
                  {"page":1,"max_pages":1,"analyses":[{"id":101,"analysistime":"2025-09-15T08:00:00Z"},{"id":102,"analysistime":"2025-10-15T08:00:00Z"}]}
                  """
                : """
                  {"page":1,"max_pages":1,"analyses":[{"id":201,"analysistime":"2025-09-20T08:00:00Z"},{"id":202,"analysistime":"2025-10-20T08:00:00Z"}]}
                  """;

            return Task.FromResult(payload);
        }

        public Task<string> GetLatestAnalysisAsync(int projectId, CancellationToken cancellationToken = default) => Task.FromResult("{}");

        public Task<string> GetAnalysisAsync(int projectId, string analysisId, CancellationToken cancellationToken = default)
        {
            var payload = analysisId is "101"
                ? """
                  {"high_level_metrics":{"month_score":4.59}}
                  """
                : """
                  {"high_level_metrics":{"month_score":6.12}}
                  """;

            return Task.FromResult(payload);
        }

        public Task<string> ListDeltaAnalysesAsync(int projectId, CancellationToken cancellationToken = default) => Task.FromResult("[]");

        public Task<string> GetDeltaAnalysisAsync(int projectId, int deltaAnalysisId, CancellationToken cancellationToken = default) => Task.FromResult("{}");

        public Task<string> GetTechnicalDebtAsync(int projectId, string analysisId, bool refactoringTargets = false, CancellationToken cancellationToken = default) => Task.FromResult("{}");

        public Task<string> GetCommitActivityTrendAsync(int projectId, string analysisId, CancellationToken cancellationToken = default) => Task.FromResult("{}");

        public Task<string> ListAnalysisCommitsAsync(int projectId, string analysisId, int page = 1, int pageSize = 200, CancellationToken cancellationToken = default) => Task.FromResult("[]");

        public Task<string> ListAnalysisIssuesAsync(int projectId, string analysisId, int page = 1, int pageSize = 200, CancellationToken cancellationToken = default) => Task.FromResult("[]");

        public Task<string> GetAnalysesByDateAsync(int projectId, DateRange period, CancellationToken cancellationToken = default) => Task.FromResult("{}");
    }
}
