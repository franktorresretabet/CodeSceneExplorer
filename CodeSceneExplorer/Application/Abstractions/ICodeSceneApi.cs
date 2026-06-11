using CodeSceneExplorer.Domain.Shared;

namespace CodeSceneExplorer.Application.Abstractions;

public interface ICodeSceneApi
{
    Task<string> ListProjectsAsync(
        int page = 1,
        string? orderBy = null,
        string? filter = null,
        string? fields = null,
        CancellationToken cancellationToken = default);

    Task<string> GetProjectAsync(int projectId, CancellationToken cancellationToken = default);

    Task<string> ListAnalysesAsync(int projectId, CancellationToken cancellationToken = default);

    Task<string> GetLatestAnalysisAsync(int projectId, CancellationToken cancellationToken = default);

    Task<string> GetAnalysisAsync(
        int projectId,
        string analysisId,
        CancellationToken cancellationToken = default);

    Task<string> ListDeltaAnalysesAsync(int projectId, CancellationToken cancellationToken = default);

    Task<string> GetDeltaAnalysisAsync(
        int projectId,
        int deltaAnalysisId,
        CancellationToken cancellationToken = default);

    Task<string> GetTechnicalDebtAsync(
        int projectId,
        string analysisId,
        bool refactoringTargets = false,
        CancellationToken cancellationToken = default);

    Task<string> GetCommitActivityTrendAsync(
        int projectId,
        string analysisId,
        CancellationToken cancellationToken = default);

    Task<string> ListAnalysisCommitsAsync(
        int projectId,
        string analysisId,
        int page = 1,
        int pageSize = 200,
        CancellationToken cancellationToken = default);

    Task<string> ListAnalysisIssuesAsync(
        int projectId,
        string analysisId,
        int page = 1,
        int pageSize = 200,
        CancellationToken cancellationToken = default);

    Task<string> GetAnalysesByDateAsync(
        int projectId,
        DateRange period,
        CancellationToken cancellationToken = default);
}
