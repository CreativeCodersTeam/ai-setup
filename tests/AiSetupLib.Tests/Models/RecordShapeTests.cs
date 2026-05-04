using AiSetupLib.Models;

namespace AiSetupLib.Tests.Models;

public class RecordShapeTests
{
    [Fact]
    public void AssetDefinition_holds_metadata_and_source_path()
    {
        var asset = new AssetDefinition(
            Name: "csharp/dotnet-tester",
            Description: "Run .NET tests",
            Type: AssetType.Skill,
            Tags: ["csharp", "testing"],
            Targets: [DeployTarget.CopilotCli, DeployTarget.ClaudeCode],
            ApplyTo: "**/*.cs",
            SourcePath: "/repo/skills/csharp/dotnet-tester/SKILL.md",
            Body: "skill body");

        asset.Name.Should().Be("csharp/dotnet-tester");
        asset.Tags.Should().HaveCount(2);
    }

    [Fact]
    public void Profile_groups_asset_references_by_type()
    {
        var profile = new Profile(
            Name: "dotnet-dev",
            Description: ".NET",
            Agents: ["dotnet-developer"],
            Instructions: ["csharp/csharp.instructions"],
            Skills: ["csharp/dotnet-tester"],
            McpConfigs: ["github"]);

        profile.Skills.Should().ContainSingle();
    }

    [Fact]
    public void DeployOptions_defaults_dry_run_and_force_to_false()
    {
        var options = new DeployOptions(
            Target: DeployTarget.ClaudeCode,
            Mode: DeployMode.Local,
            DestinationPath: "/tmp/repo");

        options.DryRun.Should().BeFalse();
        options.Force.Should().BeFalse();
        options.Profile.Should().BeNull();
        options.Agents.Should().BeEmpty();
    }

    [Fact]
    public void DeployAction_carries_kind_and_target_path()
    {
        var action = new DeployAction(
            Kind: DeployActionKind.Create,
            TargetPath: "/dest/CLAUDE.md",
            SourceAssets: ["csharp/csharp.instructions"]);

        action.Kind.Should().Be(DeployActionKind.Create);
    }

    [Fact]
    public void DeployPlan_aggregates_actions()
    {
        var plan = new DeployPlan([
            new DeployAction(DeployActionKind.Create, "/a", []),
            new DeployAction(DeployActionKind.Overwrite, "/b", []),
        ]);

        plan.Actions.Should().HaveCount(2);
    }
}
