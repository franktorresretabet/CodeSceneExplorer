using System.Net;
using CodeSceneExplorer.Domain.Shared;
using CodeSceneExplorer.Infrastructure.CodeScene;
using Xunit;

namespace CodeSceneExplorer.Tests.Infrastructure.CodeScene;

public sealed class CodeSceneApiClientTests
{
    [Fact]
    public async Task GetAnalysesByDateAsync_builds_the_expected_request()
    {
        var requests = new List<HttpRequestMessage>();

        using var handler = new RecordingHandler(requests);
        using var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.codescene.io/v2/")
        };

        var sut = new CodeSceneApiClient(client, new CodeSceneApiOptions
        {
            BaseAddress = new Uri("https://api.codescene.io/v2/")
        });

        var result = await sut.GetAnalysesByDateAsync(
            42,
            DateRange.Create(new DateOnly(2025, 1, 1), new DateOnly(2025, 12, 31)));

        Assert.Equal("{}", result);
        Assert.Single(requests);
        Assert.Equal(HttpMethod.Get, requests[0].Method);
        Assert.Equal("https://api.codescene.io/v2/projects/42/analyses/bydate?from=2025-01-01&to=2025-12-31", requests[0].RequestUri!.ToString());
    }

    private sealed class RecordingHandler(List<HttpRequestMessage> requests) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            requests.Add(request);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}")
            });
        }
    }
}
