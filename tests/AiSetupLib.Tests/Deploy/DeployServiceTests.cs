using AiSetupLib;
using AiSetupLib.Deploy;
using AiSetupLib.Discovery;
using AiSetupLib.Models;
using AiSetupLib.Profiles;
using AiSetupLib.Targets;

namespace AiSetupLib.Tests.Deploy;

public class DeployServiceTests
{
    private static AssetDefinition Asset(string name, AssetType type)
        => new(name, "d", type, [], [], null, $"/src/{name}", "body");

    private (DeployService svc, IAssetDiscovery disc, IProfileResolver pr, IDeployTarget tgt)
        Make(IReadOnlyList<AssetDefinition> available)
    {
        var disc = A.Fake<IAssetDiscovery>();
        A.CallTo(() => disc.Discover(A<string>._)).Returns(available);
        var pr = A.Fake<IProfileResolver>();
        var tgt = A.Fake<IDeployTarget>();
        A.CallTo(() => tgt.Target).Returns(DeployTarget.CopilotCli);
        var registry = new TargetRegistry([tgt]);
        return (new DeployService(disc, pr, registry, new RepoRoot("/repo")), disc, pr, tgt);
    }

    [Fact]
    public void Deploy_filters_assets_by_explicit_selection_and_calls_target()
    {
        var assets = new[]
        {
            Asset("a", AssetType.Instruction),
            Asset("b", AssetType.Instruction),
        };
        var (svc, _, _, tgt) = Make(assets);
        var opts = new DeployOptions(DeployTarget.CopilotCli, DeployMode.Repo, "/dest")
        {
            Instructions = ["a"],
        };
        A.CallTo(() => tgt.Plan(A<IReadOnlyList<AssetDefinition>>._, opts))
            .Returns(new DeployPlan([]));

        svc.Deploy(opts);

        A.CallTo(() => tgt.Plan(
                A<IReadOnlyList<AssetDefinition>>.That.Matches(a => a.Count == 1 && a[0].Name == "a"),
                opts))
            .MustHaveHappened();
        A.CallTo(() => tgt.Apply(A<DeployPlan>._, A<IReadOnlyList<AssetDefinition>>._, opts))
            .MustHaveHappened();
    }

    [Fact]
    public void Deploy_does_not_apply_when_dry_run_true()
    {
        var (svc, _, _, tgt) = Make([Asset("a", AssetType.Instruction)]);
        var opts = new DeployOptions(DeployTarget.CopilotCli, DeployMode.Repo, "/dest")
        {
            Instructions = ["a"],
            DryRun = true,
        };
        A.CallTo(() => tgt.Plan(A<IReadOnlyList<AssetDefinition>>._, opts))
            .Returns(new DeployPlan([]));

        svc.Deploy(opts);

        A.CallTo(() => tgt.Apply(A<DeployPlan>._, A<IReadOnlyList<AssetDefinition>>._, opts))
            .MustNotHaveHappened();
    }

    [Fact]
    public void Deploy_resolves_profile_when_specified()
    {
        var (svc, _, pr, tgt) = Make([
            Asset("dotnet-developer", AssetType.Agent),
            Asset("csharp/dotnet-tester", AssetType.Skill),
        ]);
        A.CallTo(() => pr.Resolve("/repo", "dotnet-dev")).Returns(new Profile(
            Name: "dotnet-dev",
            Description: "",
            Agents: ["dotnet-developer"],
            Instructions: [],
            Skills: ["csharp/dotnet-tester"],
            McpConfigs: []));
        A.CallTo(() => tgt.Plan(A<IReadOnlyList<AssetDefinition>>._, A<DeployOptions>._))
            .Returns(new DeployPlan([]));

        var opts = new DeployOptions(DeployTarget.CopilotCli, DeployMode.Repo, "/dest")
        {
            Profile = "dotnet-dev",
        };
        svc.Deploy(opts);

        A.CallTo(() => tgt.Plan(
                A<IReadOnlyList<AssetDefinition>>.That.Matches(a => a.Count == 2),
                A<DeployOptions>._))
            .MustHaveHappened();
    }

    [Fact]
    public void Deploy_throws_AssetNotFoundException_with_suggestions_for_missing_names()
    {
        var (svc, _, _, _) = Make([Asset("dotnet-tester", AssetType.Skill)]);
        var opts = new DeployOptions(DeployTarget.CopilotCli, DeployMode.Repo, "/dest")
        {
            Skills = ["dotnet-test"],
        };

        var act = () => svc.Deploy(opts);

        act.Should().Throw<AssetNotFoundException>()
            .Which.MissingNames.Should().Contain("dotnet-test");
    }

    [Fact]
    public void Deploy_returns_plan_from_target()
    {
        var (svc, _, _, tgt) = Make([Asset("a", AssetType.Instruction)]);
        var expected = new DeployPlan([new DeployAction(DeployActionKind.Create, "/x", ["a"])]);
        A.CallTo(() => tgt.Plan(A<IReadOnlyList<AssetDefinition>>._, A<DeployOptions>._))
            .Returns(expected);

        var opts = new DeployOptions(DeployTarget.CopilotCli, DeployMode.Repo, "/dest")
        {
            Instructions = ["a"],
            DryRun = true,
        };

        var plan = svc.Deploy(opts);

        plan.Should().BeSameAs(expected);
    }
}
