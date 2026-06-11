using System.Net;
using CodeSceneExplorer.Infrastructure.CodeScene;
using Xunit;

namespace CodeSceneExplorer.Tests.Infrastructure.CodeScene;

public sealed class CodeSceneApiClientAnalysesTests
{
    [Fact]
    public async Task ListAnalysisIssuesAsync_builds_the_expected_request()
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

        var result = await sut.ListAnalysisIssuesAsync(42, "analysis-1");

        Assert.Equal("{\"items\":[]}", result);
        Assert.Single(requests);
        Assert.Equal(HttpMethod.Get, requests[0].Method);
        Assert.Equal("https://api.codescene.io/v2/projects/42/analyses/analysis-1/issues?page=1&page_size=200", requests[0].RequestUri!.ToString());
    }

    [Fact]
    public async Task ListAnalysisCommitsAsync_builds_the_expected_request()
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

        var result = await sut.ListAnalysisCommitsAsync(42, "analysis-1");

        Assert.Equal("{\"items\":[]}", result);
        Assert.Single(requests);
        Assert.Equal(HttpMethod.Get, requests[0].Method);
        Assert.Equal("https://api.codescene.io/v2/projects/42/analyses/analysis-1/commits?page=1&page_size=200", requests[0].RequestUri!.ToString());
    }

    [Fact]
    public async Task GetCommitActivityTrendAsync_builds_the_expected_request()
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

        var result = await sut.GetCommitActivityTrendAsync(42, "analysis-1");

        Assert.Equal("{\"id\":999}", result);
        Assert.Single(requests);
        Assert.Equal(HttpMethod.Get, requests[0].Method);
        Assert.Equal("https://api.codescene.io/v2/projects/42/analyses/analysis-1/commit-activity", requests[0].RequestUri!.ToString());
    }

    [Fact]
    public async Task GetTechnicalDebtAsync_builds_the_expected_request()
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

        var result = await sut.GetTechnicalDebtAsync(42, "analysis-1", true);

        Assert.Equal("{\"id\":789}", result);
        Assert.Single(requests);
        Assert.Equal(HttpMethod.Get, requests[0].Method);
        Assert.Equal("https://api.codescene.io/v2/projects/42/analyses/analysis-1/technical-debt?refactoring_targets=True", requests[0].RequestUri!.ToString());
    }

    [Fact]
    public async Task GetDeltaAnalysisAsync_builds_the_expected_request()
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

        var result = await sut.GetDeltaAnalysisAsync(42, 7);

        Assert.Equal("{\"id\":456}", result);
        Assert.Single(requests);
        Assert.Equal(HttpMethod.Get, requests[0].Method);
        Assert.Equal("https://api.codescene.io/v2/projects/42/delta-analyses/7", requests[0].RequestUri!.ToString());
    }

    [Fact]
    public async Task ListDeltaAnalysesAsync_builds_the_expected_request()
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

        var result = await sut.ListDeltaAnalysesAsync(42);

        Assert.Equal("[]", result);
        Assert.Single(requests);
        Assert.Equal(HttpMethod.Get, requests[0].Method);
        Assert.Equal("https://api.codescene.io/v2/projects/42/delta-analyses", requests[0].RequestUri!.ToString());
    }

    [Fact]
    public async Task GetAnalysisAsync_builds_the_expected_request()
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

        var result = await sut.GetAnalysisAsync(42, "analysis-1");

        Assert.Equal("{\"id\":123}", result);
        Assert.Single(requests);
        Assert.Equal(HttpMethod.Get, requests[0].Method);
        Assert.Equal("https://api.codescene.io/v2/projects/42/analyses/analysis-1", requests[0].RequestUri!.ToString());
    }

    [Fact]
    public async Task GetLatestAnalysisAsync_builds_the_expected_request()
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

        var result = await sut.GetLatestAnalysisAsync(42);

        Assert.Equal("{\"id\":123}", result);
        Assert.Single(requests);
        Assert.Equal(HttpMethod.Get, requests[0].Method);
        Assert.Equal("https://api.codescene.io/v2/projects/42/analyses/latest", requests[0].RequestUri!.ToString());
    }

    [Fact]
    public async Task ListAnalysesAsync_builds_the_expected_request()
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

        var result = await sut.ListAnalysesAsync(42);

        Assert.Equal("[]", result);
        Assert.Single(requests);
        Assert.Equal(HttpMethod.Get, requests[0].Method);
        Assert.Equal("https://api.codescene.io/v2/projects/42/analyses", requests[0].RequestUri!.ToString());
    }

    private sealed class RecordingHandler(List<HttpRequestMessage> requests) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            requests.Add(request);
            var path = request.RequestUri!.AbsolutePath;
            var content = path.Contains("/issues", StringComparison.Ordinal)
                    ? "{\"items\":[]}"
                : path.Contains("/commits", StringComparison.Ordinal)
                    ? "{\"items\":[]}"
                : path.Contains("/commit-activity", StringComparison.Ordinal)
                    ? "{\"id\":999}"
                : path.Contains("/technical-debt", StringComparison.Ordinal)
                    ? "{\"id\":789}"
                : path.EndsWith("/analyses/latest", StringComparison.Ordinal) || path.Contains("/analyses/analysis-1", StringComparison.Ordinal)
                    ? "{\"id\":123}"
                : path.Contains("/delta-analyses/7", StringComparison.Ordinal)
                    ? "{\"id\":456}"
                : path.EndsWith("/delta-analyses", StringComparison.Ordinal)
                    ? "[]"
                    : "[]";

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(content)
            });
        }
    }
}
