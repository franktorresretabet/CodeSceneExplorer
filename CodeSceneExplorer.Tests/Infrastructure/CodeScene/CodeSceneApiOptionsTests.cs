using CodeSceneExplorer.Infrastructure.CodeScene;
using Xunit;

namespace CodeSceneExplorer.Tests.Infrastructure.CodeScene;

public sealed class CodeSceneApiOptionsTests
{
    [Fact]
    public void FromEnvironment_uses_the_default_base_address_when_none_is_configured()
    {
        var previous = Environment.GetEnvironmentVariable(CodeSceneApiOptions.BaseAddressEnvironmentVariable);

        try
        {
            Environment.SetEnvironmentVariable(CodeSceneApiOptions.BaseAddressEnvironmentVariable, null);

            var options = CodeSceneApiOptions.FromEnvironment();

            Assert.Equal(CodeSceneApiOptions.DefaultBaseAddress, options.BaseAddress);
        }
        finally
        {
            Environment.SetEnvironmentVariable(CodeSceneApiOptions.BaseAddressEnvironmentVariable, previous);
        }
    }

    [Fact]
    public void FromEnvironment_uses_the_configured_base_address_when_present()
    {
        var previous = Environment.GetEnvironmentVariable(CodeSceneApiOptions.BaseAddressEnvironmentVariable);

        try
        {
            Environment.SetEnvironmentVariable(CodeSceneApiOptions.BaseAddressEnvironmentVariable, "https://codescene.example.test/v2/");

            var options = CodeSceneApiOptions.FromEnvironment();

            Assert.Equal(new Uri("https://codescene.example.test/v2/"), options.BaseAddress);
        }
        finally
        {
            Environment.SetEnvironmentVariable(CodeSceneApiOptions.BaseAddressEnvironmentVariable, previous);
        }
    }

    [Fact]
    public void FromEnvironment_reads_the_access_token_from_the_user_profile_file()
    {
        var tempUserProfile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempUserProfile);

        try
        {
            File.WriteAllText(Path.Combine(tempUserProfile, CodeSceneApiOptions.AccessTokenFileName), "  test-token  ");

            var options = CodeSceneApiOptions.FromEnvironment(tempUserProfile);

            Assert.Equal("test-token", options.AccessToken);
        }
        finally
        {
            Directory.Delete(tempUserProfile, recursive: true);
        }
    }

    [Fact]
    public void FromEnvironment_leaves_the_access_token_null_when_the_file_is_missing()
    {
        var tempUserProfile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempUserProfile);

        try
        {
            var options = CodeSceneApiOptions.FromEnvironment(tempUserProfile);

            Assert.Null(options.AccessToken);
        }
        finally
        {
            Directory.Delete(tempUserProfile, recursive: true);
        }
    }
}
