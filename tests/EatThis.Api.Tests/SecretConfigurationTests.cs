using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EatThis.Api.Tests;

[TestClass]
public sealed class SecretConfigurationTests
{
    [TestMethod]
    public void Public_configuration_example_contains_no_provider_credential()
    {
        var repositoryRoot = FindRepositoryRoot();
        var examplePath = Path.Combine(
            repositoryRoot,
            "src",
            "EatThis.Api",
            "appsettings.example.json");

        Assert.IsTrue(File.Exists(examplePath));
        var content = File.ReadAllText(examplePath);
        StringAssert.Contains(content, "\"ApiKey\": \"\"");
        Assert.IsFalse(content.Contains("AIza", StringComparison.Ordinal));
    }

    [TestMethod]
    public void Secret_file_patterns_are_ignored()
    {
        var content = File.ReadAllText(Path.Combine(FindRepositoryRoot(), ".gitignore"));

        StringAssert.Contains(content, "appsettings.Development.json");
        StringAssert.Contains(content, ".env");
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null &&
               !File.Exists(Path.Combine(directory.FullName, "EatThis.slnx")))
        {
            directory = directory.Parent;
        }

        Assert.IsNotNull(directory);
        return directory!.FullName;
    }
}
