using AiSetup.Discovery;
using AiSetup.Exceptions;
using AiSetup.Models;
using AiSetup.Profiles;

namespace AiSetup.Tests.Profiles;

public sealed class ProfileResolverTests
{
    [Fact]
    public void Resolve_WithProfileAndOverrides_MergesUniqueAssets()
    {
        // Arrange
        var profile = new Profile("dotnet-dev", null,
            Agents: ["dotnet-developer"],
            Instructions: ["csharp/csharp.instructions"],
            Skills: ["csharp/dotnet-tester"],
            McpConfigs: []);
        var profiles = A.Fake<IProfileRepository>();
        A.CallTo(() => profiles.Find("dotnet-dev")).Returns(profile);

        var assets = A.Fake<IAssetRepository>();
        A.CallTo(() => assets.Find(AssetType.Agent, "dotnet-developer"))
            .Returns(NewAsset(AssetType.Agent, "dotnet-developer"));
        A.CallTo(() => assets.Find(AssetType.Instruction, "csharp/csharp.instructions"))
            .Returns(NewAsset(AssetType.Instruction, "csharp/csharp.instructions"));
        A.CallTo(() => assets.Find(AssetType.Skill, "csharp/dotnet-tester"))
            .Returns(NewAsset(AssetType.Skill, "csharp/dotnet-tester"));
        A.CallTo(() => assets.Find(AssetType.Skill, "general/create-readme"))
            .Returns(NewAsset(AssetType.Skill, "general/create-readme"));

        var sut = new ProfileResolver(assets, profiles);

        // Act
        var result = sut.Resolve(new DeployOptions
        {
            Target = DeployTarget.ClaudeCode,
            Mode = DeployMode.Repo,
            SourceRepoPath = "/src",
            ProfileName = "dotnet-dev",
            Skills = ["general/create-readme", "csharp/dotnet-tester"]
        });

        // Assert
        result.Skills.Select(a => a.Id).Should()
            .ContainInOrder("csharp/dotnet-tester", "general/create-readme");
        result.Agents.Should().HaveCount(1);
        result.Instructions.Should().HaveCount(1);
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
        Action act = () => sut.Resolve(new DeployOptions
        {
            Target = DeployTarget.ClaudeCode,
            Mode = DeployMode.Repo,
            SourceRepoPath = "/src",
            ProfileName = "dotnet-de"
        });

        // Assert
        act.Should().Throw<MissingProfileException>()
            .Which.Suggestions.Should().Contain("dotnet-dev");
    }

    [Fact]
    public void Resolve_WithMissingAsset_ThrowsMissingAssetExceptionWithSuggestions()
    {
        // Arrange
        var assets = A.Fake<IAssetRepository>();
        A.CallTo(() => assets.Find(AssetType.Skill, "csharp/dotnet-testr"))
            .Returns((AssetDefinition?)null);
        A.CallTo(() => assets.All(AssetType.Skill))
            .Returns([NewAsset(AssetType.Skill, "csharp/dotnet-tester")]);

        var profiles = A.Fake<IProfileRepository>();
        var sut = new ProfileResolver(assets, profiles);

        // Act
        Action act = () => sut.Resolve(new DeployOptions
        {
            Target = DeployTarget.ClaudeCode,
            Mode = DeployMode.Repo,
            SourceRepoPath = "/src",
            Skills = ["csharp/dotnet-testr"]
        });

        // Assert
        act.Should().Throw<MissingAssetException>()
            .Which.Suggestions.Should().Contain("csharp/dotnet-tester");
    }

    [Fact]
    public void Resolve_WithoutProfileAndOnlyOverrides_ResolvesOverrides()
    {
        // Arrange
        var assets = A.Fake<IAssetRepository>();
        A.CallTo(() => assets.Find(AssetType.Skill, "csharp/dotnet-tester"))
            .Returns(NewAsset(AssetType.Skill, "csharp/dotnet-tester"));

        var sut = new ProfileResolver(assets, A.Fake<IProfileRepository>());

        // Act
        var result = sut.Resolve(new DeployOptions
        {
            Target = DeployTarget.ClaudeCode,
            Mode = DeployMode.Repo,
            SourceRepoPath = "/src",
            Skills = ["csharp/dotnet-tester"]
        });

        // Assert
        result.Skills.Should().ContainSingle().Which.Id.Should().Be("csharp/dotnet-tester");
        result.Agents.Should().BeEmpty();
        result.Instructions.Should().BeEmpty();
        result.McpConfigs.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_WithWhitespaceProfileName_TreatsAsNoProfile()
    {
        // Arrange
        var profiles = A.Fake<IProfileRepository>();
        var sut = new ProfileResolver(A.Fake<IAssetRepository>(), profiles);

        // Act
        var result = sut.Resolve(new DeployOptions
        {
            Target = DeployTarget.ClaudeCode,
            Mode = DeployMode.Repo,
            SourceRepoPath = "/src",
            ProfileName = "   "
        });

        // Assert
        result.Should().BeEquivalentTo(ResolvedAssets.Empty);
        A.CallTo(() => profiles.Find(A<string>._)).MustNotHaveHappened();
    }

    [Fact]
    public void Resolve_WithProfileAssetsAndOverlappingOverrides_CombinesWithoutDuplicates()
    {
        // Arrange
        var profile = new Profile("p", null,
            Agents: ["agent-a"],
            Instructions: [],
            Skills: [],
            McpConfigs: []);
        var profiles = A.Fake<IProfileRepository>();
        A.CallTo(() => profiles.Find("p")).Returns(profile);

        var assets = A.Fake<IAssetRepository>();
        A.CallTo(() => assets.Find(AssetType.Agent, "agent-a"))
            .Returns(NewAsset(AssetType.Agent, "agent-a"));

        var sut = new ProfileResolver(assets, profiles);

        // Act
        var result = sut.Resolve(new DeployOptions
        {
            Target = DeployTarget.ClaudeCode,
            Mode = DeployMode.Repo,
            SourceRepoPath = "/src",
            ProfileName = "p",
            Agents = ["agent-a"]
        });

        // Assert
        result.Agents.Should().ContainSingle();
    }

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
