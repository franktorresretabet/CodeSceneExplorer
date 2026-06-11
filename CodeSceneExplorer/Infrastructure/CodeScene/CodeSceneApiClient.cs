using System.Net.Http.Headers;
using CodeSceneExplorer.Application.Abstractions;
using CodeSceneExplorer.Domain.Shared;

namespace CodeSceneExplorer.Infrastructure.CodeScene;

public sealed class CodeSceneApiClient(HttpClient httpClient, CodeSceneApiOptions options) : ICodeSceneApi
{
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

        using var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
    }
}
