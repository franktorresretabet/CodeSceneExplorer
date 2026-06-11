namespace CodeSceneExplorer.Infrastructure.CodeScene;

public sealed class CodeSceneApiOptions
{
    public const string BaseAddressEnvironmentVariable = "CODESCENE_API_BASE_URL";
    public const string AccessTokenFileName = "CodeSceneApiToken.txt";
    public static readonly Uri DefaultBaseAddress = new("https://codescene.ekasa.local/v2/");

    public required Uri BaseAddress { get; init; }

    public string? AccessToken { get; init; }

    public static CodeSceneApiOptions FromEnvironment(string? userProfileDirectory = null) =>
        new()
        {
            BaseAddress = ResolveBaseAddress(Environment.GetEnvironmentVariable(BaseAddressEnvironmentVariable)),
            AccessToken = ResolveAccessToken(userProfileDirectory)
        };

    private static Uri ResolveBaseAddress(string? configuredBaseAddress) =>
        string.IsNullOrWhiteSpace(configuredBaseAddress)
            ? DefaultBaseAddress
            : new Uri(configuredBaseAddress, UriKind.Absolute);

    private static string? ResolveAccessToken(string? userProfileDirectory)
    {
        var userProfilePath = string.IsNullOrWhiteSpace(userProfileDirectory)
            ? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
            : userProfileDirectory;

        var tokenFilePath = Path.Combine(userProfilePath, AccessTokenFileName);

        return File.Exists(tokenFilePath)
            ? File.ReadAllText(tokenFilePath).Trim()
            : null;
    }
}
