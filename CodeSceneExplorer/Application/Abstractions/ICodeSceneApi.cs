using CodeSceneExplorer.Domain.Shared;

namespace CodeSceneExplorer.Application.Abstractions;

public interface ICodeSceneApi
{
    Task<string> GetAnalysesByDateAsync(
        int projectId,
        DateRange period,
        CancellationToken cancellationToken = default);
}
