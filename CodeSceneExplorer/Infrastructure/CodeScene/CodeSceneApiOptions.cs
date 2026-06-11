namespace CodeSceneExplorer.Infrastructure.CodeScene;

public sealed class CodeSceneApiOptions
{
    public required Uri BaseAddress { get; init; }

    public string? AccessToken { get; init; }
}
