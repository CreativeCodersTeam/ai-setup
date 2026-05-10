using AiSetup.Discovery;
using AiSetup.Exceptions;
using AiSetup.Models;
using AiSetup.Profiles;

namespace AiSetup.Tests.Profiles;

public sealed class ProfileResolverTests
{
    [Fact]
    public void Resolve_WithProfile_ResolvesAllAssetTypes()
    {
        // Arrange
        var profile = new Profile("dotnet-dev", null,
            Agents: [Ref("dotnet-developer")],
            Instructions: [Ref("csharp/csharp.instructions")],
            Skills: [Ref("csharp/dotnet-tester")],
            McpConfigs: [Ref("github")]);
        var profiles = A.Fake<IProfileRepository>();
        A.CallTo(() => profiles.Find("dotnet-dev")).Returns(profile);

        var assets = A.Fake<IAssetRepository>();
        A.CallTo(() => assets.Find(AssetType.Agent, "dotnet-developer"))
            .Returns(NewAsset(AssetType.Agent, "dotnet-developer"));
        A.CallTo(() => assets.Find(AssetType.Instruction, "csharp/csharp.instructions"))
            .Returns(NewAsset(AssetType.Instruction, "csharp/csharp.instructions"));
        A.CallTo(() => assets.Find(AssetType.Skill, "csharp/dotnet-tester"))
            .Returns(NewAsset(AssetType.Skill, "csharp/dotnet-tester"));
        A.CallTo(() => assets.Find(AssetType.McpConfig, "github"))
            .Returns(NewAsset(AssetType.McpConfig, "github"));

        var sut = new ProfileResolver(assets, profiles);

        // Act
        var result = sut.Resolve(NewOptions("dotnet-dev"));

        // Assert
        result.Agents.Select(a => a.Definition.Id).Should().Equal("dotnet-developer");
        result.Instructions.Select(a => a.Definition.Id).Should().Equal("csharp/csharp.instructions");
        result.Skills.Select(a => a.Definition.Id).Should().Equal("csharp/dotnet-tester");
        result.McpConfigs.Select(a => a.Definition.Id).Should().Equal("github");
    }

    [Fact]
    public void Resolve_PropagatesPerAssetDeployMode()
    {
        // Arrange
        var profile = new Profile("p", null,
            Agents: [new ProfileAssetRef("repo-agent", DeployMode.Repo), new ProfileAssetRef("local-agent", DeployMode.Local)],
            Instructions: [], Skills: [], McpConfigs: []);
        var profiles = A.Fake<IProfileRepository>();
        A.CallTo(() => profiles.Find("p")).Returns(profile);

        var assets = A.Fake<IAssetRepository>();
        A.CallTo(() => assets.Find(AssetType.Agent, "repo-agent")).Returns(NewAsset(AssetType.Agent, "repo-agent"));
        A.CallTo(() => assets.Find(AssetType.Agent, "local-agent")).Returns(NewAsset(AssetType.Agent, "local-agent"));

        var sut = new ProfileResolver(assets, profiles);

        // Act
        var result = sut.Resolve(NewOptions("p"));

        // Assert
        result.Agents.Should().SatisfyRespectively(
            a => { a.Definition.Id.Should().Be("repo-agent"); a.Mode.Should().Be(DeployMode.Repo); },
            a => { a.Definition.Id.Should().Be("local-agent"); a.Mode.Should().Be(DeployMode.Local); });
    }

    [Fact]
    public void Resolve_WithEmptyProfile_ReturnsEmptyAssets()
    {
        // Arrange
        var profiles = A.Fake<IProfileRepository>();
        A.CallTo(() => profiles.Find("empty")).Returns(new Profile("empty", null, [], [], [], []));

        var sut = new ProfileResolver(A.Fake<IAssetRepository>(), profiles);

        // Act
        var result = sut.Resolve(NewOptions("empty"));

        // Assert
        result.Should().BeEquivalentTo(ResolvedAssets.Empty);
    }

    [Fact]
    public void Resolve_WithMissingProfile_ThrowsMissingProfileExceptionWithSuggestions()
    {
        // Arrange
        var profiles = A.Fake<IProfileRepository>();
        A.CallTo(() => profiles.Find(A<string>._)).Returns((Profile?)null);
        A.CallTo(() => profiles.All()).Returns([new Profile("dotnet-dev", null, [], [], [], [])]);

        var sut = new ProfileResolver(A.Fake<IAssetRepository>(), profiles);

        // Act
        Action act = () => sut.Resolve(NewOptions("dotnet-de"));

        // Assert
        act.Should().Throw<MissingProfileException>()
            .Which.Suggestions.Should().Contain("dotnet-dev");
    }

    [Fact]
    public void Resolve_WhenProfileReferencesMissingAsset_ThrowsMissingAssetExceptionWithSuggestions()
    {
        // Arrange
        var profiles = A.Fake<IProfileRepository>();
        A.CallTo(() => profiles.Find("p"))
            .Returns(new Profile("p", null, [], [], Skills: [Ref("csharp/dotnet-testr")], []));

        var assets = A.Fake<IAssetRepository>();
        A.CallTo(() => assets.Find(AssetType.Skill, "csharp/dotnet-testr"))
            .Returns((AssetDefinition?)null);
        A.CallTo(() => assets.All(AssetType.Skill))
            .Returns([NewAsset(AssetType.Skill, "csharp/dotnet-tester")]);

        var sut = new ProfileResolver(assets, profiles);

        // Act
        Action act = () => sut.Resolve(NewOptions("p"));

        // Assert
        act.Should().Throw<MissingAssetException>()
            .Which.Suggestions.Should().Contain("csharp/dotnet-tester");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Resolve_WithBlankProfileName_ThrowsArgumentException(string profileName)
    {
        // Arrange
        var profiles = A.Fake<IProfileRepository>();
        var sut = new ProfileResolver(A.Fake<IAssetRepository>(), profiles);

        // Act
        Action act = () => sut.Resolve(NewOptions(profileName));

        // Assert
        act.Should().Throw<ArgumentException>();
        A.CallTo(() => profiles.Find(A<string>._)).MustNotHaveHappened();
    }

    [Fact]
    public void Resolve_WithNullOptions_ThrowsArgumentNullException()
    {
        // Arrange
        var sut = new ProfileResolver(A.Fake<IAssetRepository>(), A.Fake<IProfileRepository>());

        // Act
        Action act = () => sut.Resolve(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    private static ProfileAssetRef Ref(string id) => new(id, DeployMode.Repo);

    private static DeployOptions NewOptions(string profileName) => new()
    {
        Target = DeployTarget.ClaudeCode,
        SourceRepoPath = "/src",
        ProfileName = profileName
    };

    private static AssetDefinition NewAsset(AssetType type, string id) => new(
        Id: id,
        Type: type,
        Name: id,
        Description: string.Empty,
        Tags: [],
        Targets: [],
        SourcePath: "/" + id,
        ApplyTo: null,
        Frontmatter: new Dictionary<string, object?>(),
        Body: string.Empty);
}
