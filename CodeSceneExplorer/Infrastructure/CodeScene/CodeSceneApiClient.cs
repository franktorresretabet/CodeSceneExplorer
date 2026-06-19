using System.Net.Http.Headers;
using CodeSceneExplorer.Application.Abstractions;
using CodeSceneExplorer.Domain.Shared;

namespace CodeSceneExplorer.Infrastructure.CodeScene;

public sealed class CodeSceneApiClient(HttpClient httpClient, CodeSceneApiOptions options) : ICodeSceneApi
{
    public async Task<string> ListProjectsAsync(
        int page = 1,
        string? orderBy = null,
        string? filter = null,
        string? fields = null,
        CancellationToken cancellationToken = default)
    {
        var requestUri = BuildUri(
            "projects",
            ("page", page.ToString()),
            ("order_by", orderBy),
            ("filter", filter),
            ("fields", fields));

        return await SendAsync(requestUri, cancellationToken).ConfigureAwait(false);
    }

    public async Task<string> GetProjectAsync(int projectId, CancellationToken cancellationToken = default)
    {
        return await SendAsync($"projects/{projectId}", cancellationToken).ConfigureAwait(false);
    }

    public async Task<string> ListAnalysesAsync(int projectId, int page = 1, CancellationToken cancellationToken = default)
    {
        return await SendAsync($"projects/{projectId}/analyses?page={page}", cancellationToken).ConfigureAwait(false);
    }

    public async Task<string> GetLatestAnalysisAsync(int projectId, CancellationToken cancellationToken = default)
    {
        return await SendAsync($"projects/{projectId}/analyses/latest", cancellationToken).ConfigureAwait(false);
    }

    public async Task<string> GetAnalysisAsync(
        int projectId,
        string analysisId,
        CancellationToken cancellationToken = default)
    {
        return await SendAsync($"projects/{projectId}/analyses/{analysisId}", cancellationToken).ConfigureAwait(false);
    }

    public async Task<string> ListDeltaAnalysesAsync(int projectId, CancellationToken cancellationToken = default)
    {
        return await SendAsync($"projects/{projectId}/delta-analyses", cancellationToken).ConfigureAwait(false);
    }

    public async Task<string> GetDeltaAnalysisAsync(
        int projectId,
        int deltaAnalysisId,
        CancellationToken cancellationToken = default)
    {
        return await SendAsync($"projects/{projectId}/delta-analyses/{deltaAnalysisId}", cancellationToken).ConfigureAwait(false);
    }

    public async Task<string> GetTechnicalDebtAsync(
        int projectId,
        string analysisId,
        bool refactoringTargets = false,
        CancellationToken cancellationToken = default)
    {
        var requestUri = BuildUri(
            $"projects/{projectId}/analyses/{analysisId}/technical-debt",
            ("refactoring_targets", refactoringTargets ? "True" : null));

        return await SendAsync(requestUri, cancellationToken).ConfigureAwait(false);
    }

    public async Task<string> GetCommitActivityTrendAsync(
        int projectId,
        string analysisId,
        CancellationToken cancellationToken = default)
    {
        return await SendAsync($"projects/{projectId}/analyses/{analysisId}/commit-activity", cancellationToken).ConfigureAwait(false);
    }

    public async Task<string> ListAnalysisCommitsAsync(
        int projectId,
        string analysisId,
        int page = 1,
        int pageSize = 200,
        CancellationToken cancellationToken = default)
    {
        var requestUri = BuildUri(
            $"projects/{projectId}/analyses/{analysisId}/commits",
            ("page", page.ToString()),
            ("page_size", pageSize.ToString()));

        return await SendAsync(requestUri, cancellationToken).ConfigureAwait(false);
    }

    public async Task<string> ListAnalysisIssuesAsync(
        int projectId,
        string analysisId,
        int page = 1,
        int pageSize = 200,
        CancellationToken cancellationToken = default)
    {
        var requestUri = BuildUri(
            $"projects/{projectId}/analyses/{analysisId}/issues",
            ("page", page.ToString()),
            ("page_size", pageSize.ToString()));

        return await SendAsync(requestUri, cancellationToken).ConfigureAwait(false);
    }

    public async Task<string> GetKpiTrendCodeHealthAverageAsync(
        int projectId,
        DateOnly? start = null,
        DateOnly? end = null,
        CancellationToken cancellationToken = default)
    {
        var requestUri = BuildUri(
            $"projects/{projectId}/kpi-trend/code-health/average",
            ("start", start?.ToString("yyyy-MM-dd")),
            ("end", end?.ToString("yyyy-MM-dd")));

        return await SendAsync(requestUri, cancellationToken).ConfigureAwait(false);
    }

    public async Task<string> GetKpiTrendHotspotCodeHealthAsync(
        int projectId,
        DateOnly? start = null,
        DateOnly? end = null,
        CancellationToken cancellationToken = default)
    {
        var requestUri = BuildUri(
            $"projects/{projectId}/kpi-trend/code-health/hotspots",
            ("start", start?.ToString("yyyy-MM-dd")),
            ("end", end?.ToString("yyyy-MM-dd")));

        return await SendAsync(requestUri, cancellationToken).ConfigureAwait(false);
    }

    public async Task<string> GetAnalysesByDateAsync(
        int projectId,
        DateRange period,
        CancellationToken cancellationToken = default)
    {
        var requestUri =
            $"projects/{projectId}/analyses/bydate?from={period.From:yyyy-MM-dd}&to={period.To:yyyy-MM-dd}";

        using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

        if (!string.IsNullOrWhiteSpace(options.AccessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.AccessToken);
        }

        return await SendAsync(request, cancellationToken).ConfigureAwait(false);
    }

    private async Task<string> SendAsync(string requestUri, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        return await SendAsync(request, cancellationToken).ConfigureAwait(false);
    }

    private async Task<string> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(options.AccessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.AccessToken);
        }

        using var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
    }

    private static string BuildUri(string path, params (string Name, string? Value)[] queryParameters)
    {
        var query = string.Join(
            "&",
            queryParameters
                .Where(parameter => !string.IsNullOrWhiteSpace(parameter.Value))
                .Select(parameter => $"{parameter.Name}={Uri.EscapeDataString(parameter.Value!)}"));

        return string.IsNullOrWhiteSpace(query)
            ? path
            : $"{path}?{query}";
    }
}
