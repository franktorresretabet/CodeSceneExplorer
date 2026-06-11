using System.Net;
using CodeSceneExplorer.Infrastructure.CodeScene;
using Xunit;

namespace CodeSceneExplorer.Tests.Infrastructure.CodeScene;

public sealed class CodeSceneApiClientProjectsTests
{
    [Fact]
    public async Task GetProjectAsync_builds_the_expected_request()
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

        var result = await sut.GetProjectAsync(42);

        Assert.Equal("{}", result);
        Assert.Single(requests);
        Assert.Equal(HttpMethod.Get, requests[0].Method);
        Assert.Equal("https://api.codescene.io/v2/projects/42", requests[0].RequestUri!.ToString());
    }

    [Fact]
    public async Task ListProjectsAsync_builds_the_expected_request()
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

        var result = await sut.ListProjectsAsync();

        Assert.Equal("[]", result);
        Assert.Single(requests);
        Assert.Equal(HttpMethod.Get, requests[0].Method);
        Assert.Equal("https://api.codescene.io/v2/projects?page=1", requests[0].RequestUri!.ToString());
    }

    private sealed class RecordingHandler(List<HttpRequestMessage> requests) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            requests.Add(request);
            var content = request.RequestUri!.AbsolutePath.EndsWith("/projects/42", StringComparison.Ordinal)
                ? "{}"
                : "[]";

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(content)
            });
        }
    }
}
