# AI-Setup CLI Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a cross-platform .NET CLI that deploys AI configuration assets (instructions, skills, agents, MCP configs) from this repo into target AI systems (GitHub Copilot CLI, Claude Code) — either into a target repository or into the user's local settings folder.

**Architecture:** Library-first (`AiSetupLib` + `AiSetupCli`). Domain models drive a pipeline: CLI → DeployOptions → ProfileResolver → AssetDiscoveryService → IDeployTarget adapter → file writes. Adapter pattern (`IDeployTarget`) isolates target-specific behavior; aggregators (`MarkdownAggregator`, `McpConfigMerger`) compose multi-asset output for systems that expect single files. Filesystem access goes through `System.IO.Abstractions` so all logic is unit-testable against `MockFileSystem`.

**Tech Stack:** .NET 10 / C# 12, Spectre.Console.Cli (CLI), YamlDotNet (frontmatter + profiles), System.IO.Abstractions (filesystem abstraction), Microsoft.Extensions.DependencyInjection (composition), xUnit + FakeItEasy + AwesomeAssertions (tests).

---

## Conventions used by every task

- TDD: write the failing test, run it red, write the minimal code, run it green, commit.
- All filesystem access in `AiSetupLib` goes through `IFileSystem` (System.IO.Abstractions). No `System.IO.File` static calls inside the library.
- Public types live in namespaces: `AiSetupLib.Models`, `AiSetupLib.Discovery`, `AiSetupLib.Profiles`, `AiSetupLib.Aggregation`, `AiSetupLib.Targets`, `AiSetupLib.Deploy`, `AiSetupLib.Paths`.
- Tests live in `tests/AiSetupLib.Tests/<Area>/<TypeUnderTest>Tests.cs` mirroring the namespace.
- After each task, run `dotnet test` from repo root to confirm the entire suite is green before committing.
- Commit messages use Conventional Commits prefixes: `feat:`, `test:`, `chore:`, `refactor:`.

---

## File Structure

Files created or modified, grouped by responsibility:

**Solution / project setup**
- `AiSetup.sln` — add new test project
- `Directory.Packages.props` (create) — central package versions
- `tests/AiSetupLib.Tests/AiSetupLib.Tests.csproj` (create)
- `src/AiSetupLib/AiSetupLib.csproj` — add packages
- `src/AiSetupCli/AiSetupCli.csproj` — add Spectre.Console.Cli + DI

**Library — Models** (one file per type, single responsibility)
- `src/AiSetupLib/Models/AssetType.cs`
- `src/AiSetupLib/Models/DeployTarget.cs`
- `src/AiSetupLib/Models/DeployMode.cs`
- `src/AiSetupLib/Models/AssetDefinition.cs`
- `src/AiSetupLib/Models/Profile.cs`
- `src/AiSetupLib/Models/DeployOptions.cs`
- `src/AiSetupLib/Models/DeployPlan.cs`
- `src/AiSetupLib/Models/DeployAction.cs`

**Library — Discovery / Parsing**
- `src/AiSetupLib/Discovery/FrontmatterParser.cs`
- `src/AiSetupLib/Discovery/IAssetDiscovery.cs`
- `src/AiSetupLib/Discovery/AssetDiscoveryService.cs`

**Library — Profiles**
- `src/AiSetupLib/Profiles/IProfileResolver.cs`
- `src/AiSetupLib/Profiles/ProfileResolver.cs`

**Library — Aggregation**
- `src/AiSetupLib/Aggregation/IContentAggregator.cs`
- `src/AiSetupLib/Aggregation/MarkdownAggregator.cs`
- `src/AiSetupLib/Aggregation/McpConfigMerger.cs`

**Library — Paths**
- `src/AiSetupLib/Paths/IPathProvider.cs`
- `src/AiSetupLib/Paths/PathProvider.cs`

**Library — Targets**
- `src/AiSetupLib/Targets/IDeployTarget.cs`
- `src/AiSetupLib/Targets/CopilotCliTarget.cs`
- `src/AiSetupLib/Targets/ClaudeCodeTarget.cs`
- `src/AiSetupLib/Targets/TargetRegistry.cs`

**Library — Deploy orchestration**
- `src/AiSetupLib/Deploy/IDeployService.cs`
- `src/AiSetupLib/Deploy/DeployService.cs`
- `src/AiSetupLib/Deploy/Suggestions.cs` (Levenshtein-based name suggestions)

**Library — DI**
- `src/AiSetupLib/AiSetupServices.cs` (extension method `AddAiSetup` on `IServiceCollection`)

**CLI**
- `src/AiSetupCli/Program.cs` — wire DI + Spectre app
- `src/AiSetupCli/Commands/DeployCommand.cs`
- `src/AiSetupCli/Commands/ListCommand.cs`
- `src/AiSetupCli/Commands/InfoCommand.cs`
- `src/AiSetupCli/Output/DryRunRenderer.cs`
- `src/AiSetupCli/Infrastructure/SpectreTypeRegistrar.cs`
- `src/AiSetupCli/Infrastructure/SpectreTypeResolver.cs`

**Tests** mirror the library structure under `tests/AiSetupLib.Tests/`.

---

## Task 1: Bootstrap solution — central package versions, test project, package references

**Files:**
- Create: `Directory.Packages.props`
- Create: `tests/AiSetupLib.Tests/AiSetupLib.Tests.csproj`
- Create: `tests/AiSetupLib.Tests/Smoke/SmokeTests.cs`
- Modify: `AiSetup.sln`
- Modify: `src/AiSetupLib/AiSetupLib.csproj`
- Modify: `src/AiSetupCli/AiSetupCli.csproj`

- [ ] **Step 1: Create `Directory.Packages.props` with central package versions**

Create `Directory.Packages.props` at repo root:

```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
    <CentralPackageTransitivePinningEnabled>true</CentralPackageTransitivePinningEnabled>
  </PropertyGroup>
  <ItemGroup>
    <PackageVersion Include="Spectre.Console" Version="0.49.1" />
    <PackageVersion Include="Spectre.Console.Cli" Version="0.49.1" />
    <PackageVersion Include="YamlDotNet" Version="16.2.1" />
    <PackageVersion Include="System.IO.Abstractions" Version="21.1.7" />
    <PackageVersion Include="System.IO.Abstractions.TestingHelpers" Version="21.1.7" />
    <PackageVersion Include="Microsoft.Extensions.DependencyInjection" Version="9.0.0" />
    <PackageVersion Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="9.0.0" />
    <PackageVersion Include="xunit" Version="2.9.2" />
    <PackageVersion Include="xunit.runner.visualstudio" Version="2.8.2" />
    <PackageVersion Include="Microsoft.NET.Test.Sdk" Version="17.11.1" />
    <PackageVersion Include="FakeItEasy" Version="8.3.0" />
    <PackageVersion Include="AwesomeAssertions" Version="9.2.0" />
  </ItemGroup>
</Project>
```

- [ ] **Step 2: Add packages to `src/AiSetupLib/AiSetupLib.csproj`**

Replace contents with:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <LangVersion>latest</LangVersion>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="YamlDotNet" />
    <PackageReference Include="System.IO.Abstractions" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" />
  </ItemGroup>
</Project>
```

- [ ] **Step 3: Add packages + project ref to `src/AiSetupCli/AiSetupCli.csproj`**

Replace contents with:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <LangVersion>latest</LangVersion>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <RootNamespace>AiSetupCli</RootNamespace>
    <AssemblyName>ai-setup</AssemblyName>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Spectre.Console" />
    <PackageReference Include="Spectre.Console.Cli" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" />
    <PackageReference Include="System.IO.Abstractions" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\AiSetupLib\AiSetupLib.csproj" />
  </ItemGroup>
</Project>
```

- [ ] **Step 4: Create test project `tests/AiSetupLib.Tests/AiSetupLib.Tests.csproj`**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <LangVersion>latest</LangVersion>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="xunit" />
    <PackageReference Include="xunit.runner.visualstudio" />
    <PackageReference Include="FakeItEasy" />
    <PackageReference Include="AwesomeAssertions" />
    <PackageReference Include="System.IO.Abstractions" />
    <PackageReference Include="System.IO.Abstractions.TestingHelpers" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\src\AiSetupLib\AiSetupLib.csproj" />
  </ItemGroup>
  <ItemGroup>
    <Using Include="Xunit" />
    <Using Include="AwesomeAssertions" />
    <Using Include="FakeItEasy" />
  </ItemGroup>
</Project>
```

- [ ] **Step 5: Add the test project to the solution**

Run from repo root:

```bash
dotnet sln AiSetup.sln add tests/AiSetupLib.Tests/AiSetupLib.Tests.csproj
```

Expected: `Project ... added to the solution.`

- [ ] **Step 6: Write a smoke test that fails**

Create `tests/AiSetupLib.Tests/Smoke/SmokeTests.cs`:

```csharp
namespace AiSetupLib.Tests.Smoke;

public class SmokeTests
{
    [Fact]
    public void Library_assembly_should_be_loadable()
    {
        var assembly = typeof(AiSetupLib.AssemblyMarker).Assembly;
        assembly.Should().NotBeNull();
    }
}
```

- [ ] **Step 7: Run tests — expect compile failure**

Run: `dotnet test`
Expected: build error — `AssemblyMarker` not found.

- [ ] **Step 8: Add the marker type to make it pass**

Create `src/AiSetupLib/AssemblyMarker.cs`:

```csharp
namespace AiSetupLib;

/// <summary>Marker type so tests can reference the library assembly.</summary>
internal static class AssemblyMarker;
```

Note: `internal` is fine — tests need `InternalsVisibleTo`. Add to `src/AiSetupLib/AiSetupLib.csproj` inside a new `<ItemGroup>`:

```xml
<ItemGroup>
  <InternalsVisibleTo Include="AiSetupLib.Tests" />
</ItemGroup>
```

- [ ] **Step 9: Run tests — expect pass**

Run: `dotnet test`
Expected: `Passed: 1, Failed: 0`.

- [ ] **Step 10: Commit**

```bash
git add Directory.Packages.props AiSetup.sln src/ tests/
git commit -m "chore: bootstrap solution with central package versions and test project"
```

---

## Task 2: Domain models — enums

**Files:**
- Create: `src/AiSetupLib/Models/AssetType.cs`
- Create: `src/AiSetupLib/Models/DeployTarget.cs`
- Create: `src/AiSetupLib/Models/DeployMode.cs`
- Test: `tests/AiSetupLib.Tests/Models/EnumTests.cs`

- [ ] **Step 1: Write failing tests for enum members**

Create `tests/AiSetupLib.Tests/Models/EnumTests.cs`:

```csharp
using AiSetupLib.Models;

namespace AiSetupLib.Tests.Models;

public class EnumTests
{
    [Fact]
    public void AssetType_has_required_members()
    {
        Enum.GetNames<AssetType>().Should()
            .BeEquivalentTo("Instruction", "Skill", "Agent", "McpConfig");
    }

    [Fact]
    public void DeployTarget_has_required_members()
    {
        Enum.GetNames<DeployTarget>().Should()
            .BeEquivalentTo("CopilotCli", "ClaudeCode");
    }

    [Fact]
    public void DeployMode_has_required_members()
    {
        Enum.GetNames<DeployMode>().Should()
            .BeEquivalentTo("Repo", "Local");
    }
}
```

- [ ] **Step 2: Run — expect compile failure**

Run: `dotnet test`
Expected: type lookup errors for `AssetType`, `DeployTarget`, `DeployMode`.

- [ ] **Step 3: Create the three enum files**

`src/AiSetupLib/Models/AssetType.cs`:

```csharp
namespace AiSetupLib.Models;

public enum AssetType
{
    Instruction,
    Skill,
    Agent,
    McpConfig,
}
```

`src/AiSetupLib/Models/DeployTarget.cs`:

```csharp
namespace AiSetupLib.Models;

public enum DeployTarget
{
    CopilotCli,
    ClaudeCode,
}
```

`src/AiSetupLib/Models/DeployMode.cs`:

```csharp
namespace AiSetupLib.Models;

public enum DeployMode
{
    Repo,
    Local,
}
```

- [ ] **Step 4: Run — expect pass**

Run: `dotnet test`
Expected: `Passed: 4` (3 enum tests + smoke).

- [ ] **Step 5: Commit**

```bash
git add src/AiSetupLib/Models tests/AiSetupLib.Tests/Models
git commit -m "feat: add core domain enums (AssetType, DeployTarget, DeployMode)"
```

---

## Task 3: Domain models — AssetDefinition, Profile, DeployOptions, DeployPlan, DeployAction

**Files:**
- Create: `src/AiSetupLib/Models/AssetDefinition.cs`
- Create: `src/AiSetupLib/Models/Profile.cs`
- Create: `src/AiSetupLib/Models/DeployOptions.cs`
- Create: `src/AiSetupLib/Models/DeployAction.cs`
- Create: `src/AiSetupLib/Models/DeployPlan.cs`
- Test: `tests/AiSetupLib.Tests/Models/RecordShapeTests.cs`

These are immutable record types. Tests verify shape and defaults.

- [ ] **Step 1: Write failing tests**

Create `tests/AiSetupLib.Tests/Models/RecordShapeTests.cs`:

```csharp
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
```

- [ ] **Step 2: Run — expect compile failure**

Run: `dotnet test`
Expected: missing types.

- [ ] **Step 3: Create the model files**

`src/AiSetupLib/Models/AssetDefinition.cs`:

```csharp
namespace AiSetupLib.Models;

public sealed record AssetDefinition(
    string Name,
    string Description,
    AssetType Type,
    IReadOnlyList<string> Tags,
    IReadOnlyList<DeployTarget> Targets,
    string? ApplyTo,
    string SourcePath,
    string Body);
```

`src/AiSetupLib/Models/Profile.cs`:

```csharp
namespace AiSetupLib.Models;

public sealed record Profile(
    string Name,
    string Description,
    IReadOnlyList<string> Agents,
    IReadOnlyList<string> Instructions,
    IReadOnlyList<string> Skills,
    IReadOnlyList<string> McpConfigs);
```

`src/AiSetupLib/Models/DeployOptions.cs`:

```csharp
namespace AiSetupLib.Models;

public sealed record DeployOptions(
    DeployTarget Target,
    DeployMode Mode,
    string DestinationPath)
{
    public string? Profile { get; init; }
    public IReadOnlyList<string> Agents { get; init; } = [];
    public IReadOnlyList<string> Skills { get; init; } = [];
    public IReadOnlyList<string> Instructions { get; init; } = [];
    public IReadOnlyList<string> McpConfigs { get; init; } = [];
    public bool DryRun { get; init; }
    public bool Force { get; init; }
}
```

`src/AiSetupLib/Models/DeployAction.cs`:

```csharp
namespace AiSetupLib.Models;

public enum DeployActionKind
{
    Create,
    Overwrite,
    Skip,
    Error,
}

public sealed record DeployAction(
    DeployActionKind Kind,
    string TargetPath,
    IReadOnlyList<string> SourceAssets,
    string? Message = null);
```

`src/AiSetupLib/Models/DeployPlan.cs`:

```csharp
namespace AiSetupLib.Models;

public sealed record DeployPlan(IReadOnlyList<DeployAction> Actions);
```

- [ ] **Step 4: Run — expect pass**

Run: `dotnet test`
Expected: all model tests pass.

- [ ] **Step 5: Commit**

```bash
git add src/AiSetupLib/Models tests/AiSetupLib.Tests/Models
git commit -m "feat: add core domain records (AssetDefinition, Profile, DeployOptions, DeployPlan)"
```

---

## Task 4: FrontmatterParser

Parses leading YAML frontmatter from a markdown string. Returns `(Dictionary<string, object?> frontmatter, string body)`. Invalid frontmatter → returns empty dictionary + entire input as body (no throw — caller decides whether to skip).

**Files:**
- Create: `src/AiSetupLib/Discovery/FrontmatterParser.cs`
- Test: `tests/AiSetupLib.Tests/Discovery/FrontmatterParserTests.cs`

- [ ] **Step 1: Write failing tests**

Create `tests/AiSetupLib.Tests/Discovery/FrontmatterParserTests.cs`:

```csharp
using AiSetupLib.Discovery;

namespace AiSetupLib.Tests.Discovery;

public class FrontmatterParserTests
{
    private readonly FrontmatterParser _parser = new();

    [Fact]
    public void Parses_frontmatter_and_body()
    {
        var input = """
            ---
            name: dotnet-tester
            description: "Run tests"
            tags: [csharp, testing]
            type: skill
            ---
            # Body

            Some content.
            """;

        var result = _parser.Parse(input);

        result.HasFrontmatter.Should().BeTrue();
        result.Frontmatter["name"].Should().Be("dotnet-tester");
        result.Frontmatter["description"].Should().Be("Run tests");
        result.Frontmatter["type"].Should().Be("skill");
        result.Body.Should().StartWith("# Body");
    }

    [Fact]
    public void Returns_empty_frontmatter_when_no_delimiter()
    {
        var input = "# Just a markdown file\n\nNo frontmatter here.";

        var result = _parser.Parse(input);

        result.HasFrontmatter.Should().BeFalse();
        result.Frontmatter.Should().BeEmpty();
        result.Body.Should().Be(input);
    }

    [Fact]
    public void Returns_empty_when_frontmatter_is_unterminated()
    {
        var input = """
            ---
            name: oops
            no closing fence
            """;

        var result = _parser.Parse(input);

        result.HasFrontmatter.Should().BeFalse();
        result.Body.Should().Be(input);
    }

    [Fact]
    public void Returns_empty_when_yaml_is_invalid()
    {
        var input = """
            ---
            name: [unclosed
            ---
            body
            """;

        var result = _parser.Parse(input);

        result.HasFrontmatter.Should().BeFalse();
    }

    [Fact]
    public void Parses_list_values_as_string_lists()
    {
        var input = """
            ---
            tags: [a, b, c]
            ---
            body
            """;

        var result = _parser.Parse(input);

        var tags = result.Frontmatter["tags"] as IReadOnlyList<string>;
        tags.Should().BeEquivalentTo("a", "b", "c");
    }
}
```

- [ ] **Step 2: Run — expect compile failure**

Run: `dotnet test --filter FullyQualifiedName~FrontmatterParserTests`
Expected: missing type `FrontmatterParser`.

- [ ] **Step 3: Implement `FrontmatterParser`**

Create `src/AiSetupLib/Discovery/FrontmatterParser.cs`:

```csharp
using YamlDotNet.RepresentationModel;

namespace AiSetupLib.Discovery;

public sealed record FrontmatterResult(
    bool HasFrontmatter,
    IReadOnlyDictionary<string, object?> Frontmatter,
    string Body);

public sealed class FrontmatterParser
{
    private static readonly IReadOnlyDictionary<string, object?> EmptyFrontmatter
        = new Dictionary<string, object?>();

    public FrontmatterResult Parse(string source)
    {
        if (!source.StartsWith("---", StringComparison.Ordinal))
            return new FrontmatterResult(false, EmptyFrontmatter, source);

        var lines = source.Split('\n');
        var closingIndex = -1;
        for (var i = 1; i < lines.Length; i++)
        {
            if (lines[i].TrimEnd('\r') == "---")
            {
                closingIndex = i;
                break;
            }
        }

        if (closingIndex < 0)
            return new FrontmatterResult(false, EmptyFrontmatter, source);

        var yaml = string.Join('\n', lines[1..closingIndex]);
        var body = string.Join('\n', lines[(closingIndex + 1)..]).TrimStart('\n');

        try
        {
            var stream = new YamlStream();
            stream.Load(new StringReader(yaml));
            if (stream.Documents.Count == 0
                || stream.Documents[0].RootNode is not YamlMappingNode root)
            {
                return new FrontmatterResult(false, EmptyFrontmatter, source);
            }
            var dict = ConvertMapping(root);
            return new FrontmatterResult(true, dict, body);
        }
        catch
        {
            return new FrontmatterResult(false, EmptyFrontmatter, source);
        }
    }

    private static Dictionary<string, object?> ConvertMapping(YamlMappingNode node)
    {
        var result = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (var (key, value) in node.Children)
        {
            if (key is YamlScalarNode scalarKey && scalarKey.Value is { } k)
                result[k] = ConvertNode(value);
        }
        return result;
    }

    private static object? ConvertNode(YamlNode node) => node switch
    {
        YamlScalarNode s => s.Value,
        YamlSequenceNode seq => seq.Children
            .OfType<YamlScalarNode>()
            .Select(c => c.Value ?? "")
            .ToList()
            .AsReadOnly() as IReadOnlyList<string>,
        YamlMappingNode m => ConvertMapping(m),
        _ => null,
    };
}
```

- [ ] **Step 4: Run — expect pass**

Run: `dotnet test --filter FullyQualifiedName~FrontmatterParserTests`
Expected: all 5 tests pass.

- [ ] **Step 5: Commit**

```bash
git add src/AiSetupLib/Discovery tests/AiSetupLib.Tests/Discovery
git commit -m "feat: add FrontmatterParser with YAML frontmatter extraction"
```

---

## Task 5: AssetDiscoveryService

Walks the repo's `agents/`, `instructions/`, `skills/`, `mcp-configs/` directories using `IFileSystem`, parses frontmatter, returns `IReadOnlyList<AssetDefinition>`. Skill skill-folders look for `SKILL.md`. Asset name = relative path under the type root, with file extension stripped (e.g. `csharp/dotnet-tester`).

**Files:**
- Create: `src/AiSetupLib/Discovery/IAssetDiscovery.cs`
- Create: `src/AiSetupLib/Discovery/AssetDiscoveryService.cs`
- Test: `tests/AiSetupLib.Tests/Discovery/AssetDiscoveryServiceTests.cs`

- [ ] **Step 1: Write failing tests using MockFileSystem**

Create `tests/AiSetupLib.Tests/Discovery/AssetDiscoveryServiceTests.cs`:

```csharp
using System.IO.Abstractions.TestingHelpers;
using AiSetupLib.Discovery;
using AiSetupLib.Models;

namespace AiSetupLib.Tests.Discovery;

public class AssetDiscoveryServiceTests
{
    private const string RepoRoot = "/repo";

    private static AssetDiscoveryService MakeService(MockFileSystem fs)
        => new(fs, new FrontmatterParser());

    [Fact]
    public void Discovers_instruction_under_instructions_dir()
    {
        var fs = new MockFileSystem();
        fs.AddFile($"{RepoRoot}/instructions/csharp/csharp.instructions.md",
            new MockFileData("""
            ---
            name: csharp.instructions
            description: C# guidelines
            type: instruction
            tags: [csharp]
            targets: [copilot-cli, claude-code]
            ---
            # C# rules
            """));

        var assets = MakeService(fs).Discover(RepoRoot);

        assets.Should().ContainSingle()
            .Which.Should().Match<AssetDefinition>(a =>
                a.Name == "csharp/csharp.instructions"
                && a.Type == AssetType.Instruction
                && a.Body.StartsWith("# C# rules"));
    }

    [Fact]
    public void Discovers_skill_via_SKILL_md_in_subfolder()
    {
        var fs = new MockFileSystem();
        fs.AddFile($"{RepoRoot}/skills/csharp/dotnet-tester/SKILL.md",
            new MockFileData("""
            ---
            name: dotnet-tester
            description: Run tests
            type: skill
            tags: [csharp, testing]
            targets: [copilot-cli]
            ---
            Skill body
            """));

        var assets = MakeService(fs).Discover(RepoRoot);

        assets.Should().ContainSingle()
            .Which.Name.Should().Be("csharp/dotnet-tester");
        assets[0].Type.Should().Be(AssetType.Skill);
    }

    [Fact]
    public void Discovers_agent_under_agents_dir()
    {
        var fs = new MockFileSystem();
        fs.AddFile($"{RepoRoot}/agents/dotnet-developer.md",
            new MockFileData("""
            ---
            name: dotnet-developer
            description: .NET dev
            type: agent
            tags: []
            targets: [claude-code]
            ---
            Agent
            """));

        var assets = MakeService(fs).Discover(RepoRoot);

        assets[0].Name.Should().Be("dotnet-developer");
        assets[0].Type.Should().Be(AssetType.Agent);
    }

    [Fact]
    public void Discovers_mcp_config_under_mcp_configs_dir()
    {
        var fs = new MockFileSystem();
        fs.AddFile($"{RepoRoot}/mcp-configs/github.yaml",
            new MockFileData("""
            ---
            name: github
            description: GitHub MCP server
            type: mcp-config
            tags: [github]
            targets: [copilot-cli, claude-code]
            ---
            command: github-mcp
            """));

        var assets = MakeService(fs).Discover(RepoRoot);

        assets[0].Name.Should().Be("github");
        assets[0].Type.Should().Be(AssetType.McpConfig);
    }

    [Fact]
    public void Skips_files_with_invalid_frontmatter()
    {
        var fs = new MockFileSystem();
        fs.AddFile($"{RepoRoot}/instructions/bad.md",
            new MockFileData("no frontmatter at all"));
        fs.AddFile($"{RepoRoot}/instructions/good.md",
            new MockFileData("""
            ---
            name: good
            description: ok
            type: instruction
            ---
            body
            """));

        var assets = MakeService(fs).Discover(RepoRoot);

        assets.Should().ContainSingle().Which.Name.Should().Be("good");
    }

    [Fact]
    public void Returns_empty_when_directories_missing()
    {
        var fs = new MockFileSystem();

        var assets = MakeService(fs).Discover(RepoRoot);

        assets.Should().BeEmpty();
    }

    [Fact]
    public void Parses_targets_from_frontmatter()
    {
        var fs = new MockFileSystem();
        fs.AddFile($"{RepoRoot}/instructions/x.md",
            new MockFileData("""
            ---
            name: x
            description: x
            type: instruction
            targets: [copilot-cli, claude-code]
            ---
            """));

        var asset = MakeService(fs).Discover(RepoRoot)[0];

        asset.Targets.Should().BeEquivalentTo(
            [DeployTarget.CopilotCli, DeployTarget.ClaudeCode]);
    }
}
```

- [ ] **Step 2: Run — expect compile failure**

Run: `dotnet test --filter FullyQualifiedName~AssetDiscoveryServiceTests`
Expected: missing types.

- [ ] **Step 3: Implement interface**

Create `src/AiSetupLib/Discovery/IAssetDiscovery.cs`:

```csharp
using AiSetupLib.Models;

namespace AiSetupLib.Discovery;

public interface IAssetDiscovery
{
    IReadOnlyList<AssetDefinition> Discover(string repoRoot);
}
```

- [ ] **Step 4: Implement `AssetDiscoveryService`**

Create `src/AiSetupLib/Discovery/AssetDiscoveryService.cs`:

```csharp
using System.IO.Abstractions;
using AiSetupLib.Models;

namespace AiSetupLib.Discovery;

public sealed class AssetDiscoveryService : IAssetDiscovery
{
    private static readonly Dictionary<string, AssetType> TypeDirs = new(StringComparer.Ordinal)
    {
        ["instructions"] = AssetType.Instruction,
        ["agents"] = AssetType.Agent,
        ["skills"] = AssetType.Skill,
        ["mcp-configs"] = AssetType.McpConfig,
    };

    private static readonly Dictionary<string, AssetType> FrontmatterTypes = new(StringComparer.Ordinal)
    {
        ["instruction"] = AssetType.Instruction,
        ["agent"] = AssetType.Agent,
        ["skill"] = AssetType.Skill,
        ["mcp-config"] = AssetType.McpConfig,
    };

    private static readonly Dictionary<string, DeployTarget> TargetMap = new(StringComparer.Ordinal)
    {
        ["copilot-cli"] = DeployTarget.CopilotCli,
        ["claude-code"] = DeployTarget.ClaudeCode,
    };

    private readonly IFileSystem _fs;
    private readonly FrontmatterParser _parser;

    public AssetDiscoveryService(IFileSystem fs, FrontmatterParser parser)
    {
        _fs = fs;
        _parser = parser;
    }

    public IReadOnlyList<AssetDefinition> Discover(string repoRoot)
    {
        var results = new List<AssetDefinition>();
        foreach (var (dir, defaultType) in TypeDirs)
        {
            var typeRoot = _fs.Path.Combine(repoRoot, dir);
            if (!_fs.Directory.Exists(typeRoot))
                continue;

            foreach (var file in EnumerateAssetFiles(typeRoot, defaultType))
            {
                var asset = TryReadAsset(file, typeRoot, defaultType);
                if (asset is not null)
                    results.Add(asset);
            }
        }
        return results;
    }

    private IEnumerable<string> EnumerateAssetFiles(string typeRoot, AssetType type)
    {
        if (type == AssetType.Skill)
        {
            return _fs.Directory.EnumerateFiles(typeRoot, "SKILL.md", SearchOption.AllDirectories);
        }
        if (type == AssetType.McpConfig)
        {
            return _fs.Directory.EnumerateFiles(typeRoot, "*.yaml", SearchOption.AllDirectories)
                .Concat(_fs.Directory.EnumerateFiles(typeRoot, "*.yml", SearchOption.AllDirectories));
        }
        return _fs.Directory.EnumerateFiles(typeRoot, "*.md", SearchOption.AllDirectories);
    }

    private AssetDefinition? TryReadAsset(string path, string typeRoot, AssetType defaultType)
    {
        var raw = _fs.File.ReadAllText(path);
        var fm = _parser.Parse(raw);
        if (!fm.HasFrontmatter)
            return null;

        var name = ComputeName(path, typeRoot, defaultType);
        var description = AsString(fm.Frontmatter, "description") ?? "";
        var type = AsString(fm.Frontmatter, "type") is { } t && FrontmatterTypes.TryGetValue(t, out var parsed)
            ? parsed
            : defaultType;
        var tags = AsStringList(fm.Frontmatter, "tags");
        var targets = AsStringList(fm.Frontmatter, "targets")
            .Where(s => TargetMap.ContainsKey(s))
            .Select(s => TargetMap[s])
            .ToList()
            .AsReadOnly();
        var applyTo = AsString(fm.Frontmatter, "applyTo");

        return new AssetDefinition(
            Name: name,
            Description: description,
            Type: type,
            Tags: tags,
            Targets: targets,
            ApplyTo: applyTo,
            SourcePath: path,
            Body: fm.Body);
    }

    private string ComputeName(string path, string typeRoot, AssetType type)
    {
        if (type == AssetType.Skill)
        {
            var folder = _fs.Path.GetDirectoryName(path)!;
            return Relative(folder, typeRoot);
        }
        var rel = Relative(path, typeRoot);
        var dot = rel.LastIndexOf('.');
        return dot > 0 ? rel[..dot] : rel;
    }

    private string Relative(string path, string root)
    {
        var rel = _fs.Path.GetRelativePath(root, path);
        return rel.Replace('\\', '/');
    }

    private static string? AsString(IReadOnlyDictionary<string, object?> dict, string key)
        => dict.TryGetValue(key, out var v) ? v as string : null;

    private static IReadOnlyList<string> AsStringList(IReadOnlyDictionary<string, object?> dict, string key)
    {
        if (!dict.TryGetValue(key, out var v)) return [];
        return v switch
        {
            IReadOnlyList<string> list => list,
            string s => [s],
            _ => [],
        };
    }
}
```

- [ ] **Step 5: Run — expect pass**

Run: `dotnet test --filter FullyQualifiedName~AssetDiscoveryServiceTests`
Expected: all 7 tests pass.

- [ ] **Step 6: Commit**

```bash
git add src/AiSetupLib/Discovery tests/AiSetupLib.Tests/Discovery
git commit -m "feat: add AssetDiscoveryService with directory walking and frontmatter parsing"
```

---

## Task 6: ProfileResolver

Loads a YAML profile from `<repoRoot>/profiles/<name>.yaml` and resolves it into a flattened `DeployOptions`-style asset list. Returns the populated `Profile` record.

**Files:**
- Create: `src/AiSetupLib/Profiles/IProfileResolver.cs`
- Create: `src/AiSetupLib/Profiles/ProfileResolver.cs`
- Test: `tests/AiSetupLib.Tests/Profiles/ProfileResolverTests.cs`

- [ ] **Step 1: Write failing tests**

Create `tests/AiSetupLib.Tests/Profiles/ProfileResolverTests.cs`:

```csharp
using System.IO.Abstractions.TestingHelpers;
using AiSetupLib.Profiles;

namespace AiSetupLib.Tests.Profiles;

public class ProfileResolverTests
{
    private const string RepoRoot = "/repo";

    [Fact]
    public void Loads_profile_yaml_and_returns_profile_record()
    {
        var fs = new MockFileSystem();
        fs.AddFile($"{RepoRoot}/profiles/dotnet-dev.yaml", new MockFileData("""
            name: dotnet-dev
            description: ".NET assets"
            agents:
              - dotnet-developer
            instructions:
              - csharp/csharp.instructions
            skills:
              - csharp/dotnet-tester
              - csharp/ef-core
            mcp-configs:
              - github
            """));

        var resolver = new ProfileResolver(fs);
        var profile = resolver.Resolve(RepoRoot, "dotnet-dev");

        profile.Name.Should().Be("dotnet-dev");
        profile.Description.Should().Be(".NET assets");
        profile.Agents.Should().ContainSingle().Which.Should().Be("dotnet-developer");
        profile.Instructions.Should().ContainSingle().Which.Should().Be("csharp/csharp.instructions");
        profile.Skills.Should().HaveCount(2);
        profile.McpConfigs.Should().ContainSingle().Which.Should().Be("github");
    }

    [Fact]
    public void Throws_FileNotFoundException_when_profile_missing()
    {
        var fs = new MockFileSystem();
        var resolver = new ProfileResolver(fs);

        var act = () => resolver.Resolve(RepoRoot, "nope");

        act.Should().Throw<FileNotFoundException>()
            .WithMessage("*nope*");
    }

    [Fact]
    public void Defaults_lists_to_empty_when_keys_absent()
    {
        var fs = new MockFileSystem();
        fs.AddFile($"{RepoRoot}/profiles/minimal.yaml", new MockFileData("""
            name: minimal
            description: "x"
            """));

        var profile = new ProfileResolver(fs).Resolve(RepoRoot, "minimal");

        profile.Agents.Should().BeEmpty();
        profile.Skills.Should().BeEmpty();
        profile.Instructions.Should().BeEmpty();
        profile.McpConfigs.Should().BeEmpty();
    }
}
```

- [ ] **Step 2: Run — expect compile failure**

Run: `dotnet test --filter FullyQualifiedName~ProfileResolverTests`
Expected: missing types.

- [ ] **Step 3: Implement interface**

Create `src/AiSetupLib/Profiles/IProfileResolver.cs`:

```csharp
using AiSetupLib.Models;

namespace AiSetupLib.Profiles;

public interface IProfileResolver
{
    Profile Resolve(string repoRoot, string profileName);
}
```

- [ ] **Step 4: Implement `ProfileResolver`**

Create `src/AiSetupLib/Profiles/ProfileResolver.cs`:

```csharp
using System.IO.Abstractions;
using AiSetupLib.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace AiSetupLib.Profiles;

public sealed class ProfileResolver : IProfileResolver
{
    private readonly IFileSystem _fs;
    private readonly IDeserializer _yaml = new DeserializerBuilder()
        .WithNamingConvention(HyphenatedNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    public ProfileResolver(IFileSystem fs) => _fs = fs;

    public Profile Resolve(string repoRoot, string profileName)
    {
        var path = _fs.Path.Combine(repoRoot, "profiles", profileName + ".yaml");
        if (!_fs.File.Exists(path))
            throw new FileNotFoundException($"Profile '{profileName}' not found at {path}", path);

        var text = _fs.File.ReadAllText(path);
        var dto = _yaml.Deserialize<ProfileDto>(text) ?? new ProfileDto();

        return new Profile(
            Name: dto.Name ?? profileName,
            Description: dto.Description ?? "",
            Agents: dto.Agents ?? [],
            Instructions: dto.Instructions ?? [],
            Skills: dto.Skills ?? [],
            McpConfigs: dto.McpConfigs ?? []);
    }

    private sealed class ProfileDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public List<string>? Agents { get; set; }
        public List<string>? Instructions { get; set; }
        public List<string>? Skills { get; set; }
        public List<string>? McpConfigs { get; set; }
    }
}
```

- [ ] **Step 5: Run — expect pass**

Run: `dotnet test --filter FullyQualifiedName~ProfileResolverTests`
Expected: all 3 tests pass.

- [ ] **Step 6: Commit**

```bash
git add src/AiSetupLib/Profiles tests/AiSetupLib.Tests/Profiles
git commit -m "feat: add ProfileResolver loading YAML profiles into Profile records"
```

---

## Task 7: MarkdownAggregator

Aggregates many `AssetDefinition` bodies into a single markdown string for Claude Code's `CLAUDE.md`. Format: `# AI-Setup Aggregated Configuration` header, then per-asset section `## <Type>: <Name>` with description blockquote and body.

**Files:**
- Create: `src/AiSetupLib/Aggregation/IContentAggregator.cs`
- Create: `src/AiSetupLib/Aggregation/MarkdownAggregator.cs`
- Test: `tests/AiSetupLib.Tests/Aggregation/MarkdownAggregatorTests.cs`

- [ ] **Step 1: Write failing tests**

Create `tests/AiSetupLib.Tests/Aggregation/MarkdownAggregatorTests.cs`:

```csharp
using AiSetupLib.Aggregation;
using AiSetupLib.Models;

namespace AiSetupLib.Tests.Aggregation;

public class MarkdownAggregatorTests
{
    private static AssetDefinition Asset(string name, AssetType type, string body, string desc = "d")
        => new(name, desc, type, [], [], null, $"/src/{name}", body);

    [Fact]
    public void Aggregates_assets_with_header_and_per_asset_sections()
    {
        var aggregator = new MarkdownAggregator();
        var output = aggregator.Aggregate([
            Asset("csharp/rules", AssetType.Instruction, "Use sealed classes."),
            Asset("dotnet-developer", AssetType.Agent, "Agent body."),
        ]);

        output.Should().Contain("# AI-Setup Aggregated Configuration");
        output.Should().Contain("## Instruction: csharp/rules");
        output.Should().Contain("Use sealed classes.");
        output.Should().Contain("## Agent: dotnet-developer");
        output.Should().Contain("Agent body.");
    }

    [Fact]
    public void Sorts_assets_by_type_then_name_for_stable_output()
    {
        var aggregator = new MarkdownAggregator();
        var output = aggregator.Aggregate([
            Asset("z", AssetType.Agent, "z"),
            Asset("a", AssetType.Instruction, "a"),
            Asset("m", AssetType.Instruction, "m"),
        ]);

        var idxA = output.IndexOf("## Instruction: a", StringComparison.Ordinal);
        var idxM = output.IndexOf("## Instruction: m", StringComparison.Ordinal);
        var idxZ = output.IndexOf("## Agent: z", StringComparison.Ordinal);

        idxA.Should().BeLessThan(idxM);
        idxM.Should().BeLessThan(idxZ);
    }

    [Fact]
    public void Includes_description_as_blockquote()
    {
        var aggregator = new MarkdownAggregator();
        var output = aggregator.Aggregate([
            Asset("x", AssetType.Instruction, "body", desc: "Some guideline"),
        ]);

        output.Should().Contain("> Some guideline");
    }

    [Fact]
    public void Returns_only_header_when_no_assets()
    {
        var aggregator = new MarkdownAggregator();
        var output = aggregator.Aggregate([]);

        output.Should().Contain("# AI-Setup Aggregated Configuration");
    }
}
```

- [ ] **Step 2: Run — expect compile failure**

Run: `dotnet test --filter FullyQualifiedName~MarkdownAggregatorTests`
Expected: missing types.

- [ ] **Step 3: Implement interface**

Create `src/AiSetupLib/Aggregation/IContentAggregator.cs`:

```csharp
using AiSetupLib.Models;

namespace AiSetupLib.Aggregation;

public interface IContentAggregator
{
    string Aggregate(IReadOnlyList<AssetDefinition> assets);
}
```

- [ ] **Step 4: Implement `MarkdownAggregator`**

Create `src/AiSetupLib/Aggregation/MarkdownAggregator.cs`:

```csharp
using System.Text;
using AiSetupLib.Models;

namespace AiSetupLib.Aggregation;

public sealed class MarkdownAggregator : IContentAggregator
{
    private static readonly AssetType[] TypeOrder =
    [
        AssetType.Instruction,
        AssetType.Skill,
        AssetType.Agent,
        AssetType.McpConfig,
    ];

    public string Aggregate(IReadOnlyList<AssetDefinition> assets)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# AI-Setup Aggregated Configuration");
        sb.AppendLine();
        sb.AppendLine("> Generated by ai-setup. Do not edit by hand.");
        sb.AppendLine();

        var ordered = assets
            .OrderBy(a => Array.IndexOf(TypeOrder, a.Type))
            .ThenBy(a => a.Name, StringComparer.Ordinal);

        foreach (var asset in ordered)
        {
            sb.Append("## ").Append(asset.Type).Append(": ").AppendLine(asset.Name);
            sb.AppendLine();
            if (!string.IsNullOrWhiteSpace(asset.Description))
            {
                sb.Append("> ").AppendLine(asset.Description);
                sb.AppendLine();
            }
            sb.AppendLine(asset.Body.TrimEnd());
            sb.AppendLine();
        }

        return sb.ToString();
    }
}
```

- [ ] **Step 5: Run — expect pass**

Run: `dotnet test --filter FullyQualifiedName~MarkdownAggregatorTests`
Expected: all 4 tests pass.

- [ ] **Step 6: Commit**

```bash
git add src/AiSetupLib/Aggregation tests/AiSetupLib.Tests/Aggregation
git commit -m "feat: add MarkdownAggregator producing single-file Claude config"
```

---

## Task 8: McpConfigMerger

Merges multiple MCP config asset bodies (each is YAML defining `command:`, `args:`, `env:`, `url:` etc.) into a single JSON object suitable for `.vscode/mcp.json` (Copilot CLI) or `.claude/settings.json` `mcpServers` block (Claude Code). The merger normalizes both targets to a `Dictionary<string, object>` keyed by asset name.

**Files:**
- Create: `src/AiSetupLib/Aggregation/McpConfigMerger.cs`
- Test: `tests/AiSetupLib.Tests/Aggregation/McpConfigMergerTests.cs`

- [ ] **Step 1: Write failing tests**

Create `tests/AiSetupLib.Tests/Aggregation/McpConfigMergerTests.cs`:

```csharp
using AiSetupLib.Aggregation;
using AiSetupLib.Models;

namespace AiSetupLib.Tests.Aggregation;

public class McpConfigMergerTests
{
    private static AssetDefinition Mcp(string name, string body)
        => new(name, "d", AssetType.McpConfig, [], [], null, $"/src/{name}.yaml", body);

    [Fact]
    public void Merges_two_mcp_configs_into_a_dictionary_keyed_by_name()
    {
        var merger = new McpConfigMerger();
        var result = merger.Merge([
            Mcp("github", "command: github-mcp\nargs: [--port, '8080']"),
            Mcp("filesystem", "command: fs-mcp"),
        ]);

        result.Should().ContainKeys("github", "filesystem");
        result["github"].Should().BeOfType<Dictionary<string, object?>>();
    }

    [Fact]
    public void Returns_empty_when_no_assets_provided()
    {
        var result = new McpConfigMerger().Merge([]);
        result.Should().BeEmpty();
    }

    [Fact]
    public void Preserves_string_and_list_values_from_yaml()
    {
        var merger = new McpConfigMerger();
        var result = merger.Merge([
            Mcp("github", "command: github-mcp\nargs:\n  - --verbose\n  - --port=8080"),
        ]);

        var github = (Dictionary<string, object?>)result["github"]!;
        github["command"].Should().Be("github-mcp");
        github["args"].Should().BeAssignableTo<IEnumerable<object?>>();
    }

    [Fact]
    public void Skips_assets_with_empty_body()
    {
        var merger = new McpConfigMerger();
        var result = merger.Merge([Mcp("blank", "")]);
        result.Should().NotContainKey("blank");
    }
}
```

- [ ] **Step 2: Run — expect compile failure**

Run: `dotnet test --filter FullyQualifiedName~McpConfigMergerTests`
Expected: missing type.

- [ ] **Step 3: Implement `McpConfigMerger`**

Create `src/AiSetupLib/Aggregation/McpConfigMerger.cs`:

```csharp
using AiSetupLib.Models;
using YamlDotNet.RepresentationModel;

namespace AiSetupLib.Aggregation;

public sealed class McpConfigMerger
{
    public IReadOnlyDictionary<string, object?> Merge(IReadOnlyList<AssetDefinition> mcpAssets)
    {
        var result = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (var asset in mcpAssets)
        {
            if (string.IsNullOrWhiteSpace(asset.Body))
                continue;

            var stream = new YamlStream();
            try
            {
                stream.Load(new StringReader(asset.Body));
            }
            catch
            {
                continue;
            }
            if (stream.Documents.Count == 0
                || stream.Documents[0].RootNode is not YamlMappingNode root)
            {
                continue;
            }
            result[asset.Name] = ConvertMapping(root);
        }
        return result;
    }

    private static object? ConvertNode(YamlNode node) => node switch
    {
        YamlScalarNode s => s.Value,
        YamlSequenceNode seq => seq.Children.Select(ConvertNode).ToList(),
        YamlMappingNode m => ConvertMapping(m),
        _ => null,
    };

    private static Dictionary<string, object?> ConvertMapping(YamlMappingNode node)
    {
        var dict = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (var (key, value) in node.Children)
        {
            if (key is YamlScalarNode k && k.Value is { } name)
                dict[name] = ConvertNode(value);
        }
        return dict;
    }
}
```

- [ ] **Step 4: Run — expect pass**

Run: `dotnet test --filter FullyQualifiedName~McpConfigMergerTests`
Expected: all 4 tests pass.

- [ ] **Step 5: Commit**

```bash
git add src/AiSetupLib/Aggregation tests/AiSetupLib.Tests/Aggregation
git commit -m "feat: add McpConfigMerger combining MCP YAML configs into dictionary"
```

---

## Task 9: PathProvider — cross-platform local-mode paths

Returns the platform-specific local config root for each `DeployTarget`. Configurable via constructor so tests can inject custom paths. Also exposes the relative subpaths each target uses for instructions/skills/etc.

**Files:**
- Create: `src/AiSetupLib/Paths/IPathProvider.cs`
- Create: `src/AiSetupLib/Paths/PathProvider.cs`
- Test: `tests/AiSetupLib.Tests/Paths/PathProviderTests.cs`

- [ ] **Step 1: Write failing tests**

Create `tests/AiSetupLib.Tests/Paths/PathProviderTests.cs`:

```csharp
using AiSetupLib.Models;
using AiSetupLib.Paths;

namespace AiSetupLib.Tests.Paths;

public class PathProviderTests
{
    [Fact]
    public void Resolves_claude_code_local_root_to_supplied_home_subfolder()
    {
        var provider = new PathProvider(home: "/home/me");

        var root = provider.GetLocalRoot(DeployTarget.ClaudeCode);

        root.Should().Be("/home/me/.claude");
    }

    [Fact]
    public void Resolves_copilot_cli_local_root_per_platform()
    {
        var linux = new PathProvider(home: "/home/me", platform: PlatformKind.Linux);
        var mac = new PathProvider(home: "/Users/me", platform: PlatformKind.MacOS);
        var win = new PathProvider(home: @"C:\Users\me", platform: PlatformKind.Windows,
            appData: @"C:\Users\me\AppData\Roaming");

        linux.GetLocalRoot(DeployTarget.CopilotCli)
            .Should().Be("/home/me/.config/github-copilot");
        mac.GetLocalRoot(DeployTarget.CopilotCli)
            .Should().Be("/Users/me/Library/Application Support/github-copilot");
        win.GetLocalRoot(DeployTarget.CopilotCli)
            .Should().Be(@"C:\Users\me\AppData\Roaming\GitHub Copilot CLI");
    }

    [Fact]
    public void Returns_repo_subpaths_for_copilot_cli()
    {
        var provider = new PathProvider(home: "/h");

        provider.GetRepoSubPath(DeployTarget.CopilotCli, AssetType.Instruction)
            .Should().Be(".github/instructions");
        provider.GetRepoSubPath(DeployTarget.CopilotCli, AssetType.Skill)
            .Should().Be(".github/skills");
        provider.GetRepoSubPath(DeployTarget.CopilotCli, AssetType.Agent)
            .Should().Be(".github/agents");
    }

    [Fact]
    public void Returns_aggregated_file_path_for_claude_code()
    {
        var provider = new PathProvider(home: "/h");
        provider.GetClaudeAggregatedFileName().Should().Be("CLAUDE.md");
    }

    [Fact]
    public void Returns_mcp_settings_file_for_each_target()
    {
        var provider = new PathProvider(home: "/h");
        provider.GetMcpSettingsRelativePath(DeployTarget.CopilotCli)
            .Should().Be(".vscode/mcp.json");
        provider.GetMcpSettingsRelativePath(DeployTarget.ClaudeCode)
            .Should().Be(".claude/settings.json");
    }
}
```

- [ ] **Step 2: Run — expect compile failure**

Run: `dotnet test --filter FullyQualifiedName~PathProviderTests`
Expected: missing types.

- [ ] **Step 3: Implement interface**

Create `src/AiSetupLib/Paths/IPathProvider.cs`:

```csharp
using AiSetupLib.Models;

namespace AiSetupLib.Paths;

public interface IPathProvider
{
    string GetLocalRoot(DeployTarget target);
    string GetRepoSubPath(DeployTarget target, AssetType type);
    string GetClaudeAggregatedFileName();
    string GetMcpSettingsRelativePath(DeployTarget target);
}
```

- [ ] **Step 4: Implement `PathProvider`**

Create `src/AiSetupLib/Paths/PathProvider.cs`:

```csharp
using System.Runtime.InteropServices;
using AiSetupLib.Models;

namespace AiSetupLib.Paths;

public enum PlatformKind { Windows, MacOS, Linux }

public sealed class PathProvider : IPathProvider
{
    private readonly string _home;
    private readonly string _appData;
    private readonly PlatformKind _platform;

    public PathProvider() : this(
        home: Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        platform: DetectPlatform(),
        appData: Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData))
    { }

    public PathProvider(string home, PlatformKind? platform = null, string? appData = null)
    {
        _home = home;
        _platform = platform ?? DetectPlatform();
        _appData = appData ?? home;
    }

    public string GetLocalRoot(DeployTarget target) => target switch
    {
        DeployTarget.ClaudeCode => Combine(_home, ".claude"),
        DeployTarget.CopilotCli => _platform switch
        {
            PlatformKind.Windows => Combine(_appData, "GitHub Copilot CLI"),
            PlatformKind.MacOS => Combine(_home, "Library/Application Support/github-copilot"),
            PlatformKind.Linux => Combine(_home, ".config/github-copilot"),
            _ => throw new PlatformNotSupportedException(),
        },
        _ => throw new ArgumentOutOfRangeException(nameof(target)),
    };

    public string GetRepoSubPath(DeployTarget target, AssetType type) => (target, type) switch
    {
        (DeployTarget.CopilotCli, AssetType.Instruction) => ".github/instructions",
        (DeployTarget.CopilotCli, AssetType.Skill) => ".github/skills",
        (DeployTarget.CopilotCli, AssetType.Agent) => ".github/agents",
        (DeployTarget.CopilotCli, AssetType.McpConfig) => ".vscode",
        (DeployTarget.ClaudeCode, AssetType.Skill) => ".claude/skills",
        (DeployTarget.ClaudeCode, AssetType.McpConfig) => ".claude",
        (DeployTarget.ClaudeCode, _) => "",
        _ => throw new ArgumentOutOfRangeException(),
    };

    public string GetClaudeAggregatedFileName() => "CLAUDE.md";

    public string GetMcpSettingsRelativePath(DeployTarget target) => target switch
    {
        DeployTarget.CopilotCli => ".vscode/mcp.json",
        DeployTarget.ClaudeCode => ".claude/settings.json",
        _ => throw new ArgumentOutOfRangeException(),
    };

    private static string Combine(string a, string b)
    {
        if (b.Contains('/') && !b.Contains('\\'))
        {
            return a.TrimEnd('/', '\\') + "/" + b;
        }
        return Path.Combine(a, b);
    }

    private static PlatformKind DetectPlatform()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return PlatformKind.Windows;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) return PlatformKind.MacOS;
        return PlatformKind.Linux;
    }
}
```

- [ ] **Step 5: Run — expect pass**

Run: `dotnet test --filter FullyQualifiedName~PathProviderTests`
Expected: all 5 tests pass.

- [ ] **Step 6: Commit**

```bash
git add src/AiSetupLib/Paths tests/AiSetupLib.Tests/Paths
git commit -m "feat: add PathProvider with cross-platform target paths"
```

---

## Task 10: Suggestions helper (Levenshtein-based name suggestions)

Used by error handling: when a requested asset name isn't found, suggest closest matches.

**Files:**
- Create: `src/AiSetupLib/Deploy/Suggestions.cs`
- Test: `tests/AiSetupLib.Tests/Deploy/SuggestionsTests.cs`

- [ ] **Step 1: Write failing tests**

Create `tests/AiSetupLib.Tests/Deploy/SuggestionsTests.cs`:

```csharp
using AiSetupLib.Deploy;

namespace AiSetupLib.Tests.Deploy;

public class SuggestionsTests
{
    [Fact]
    public void Returns_closest_matches_within_distance_threshold()
    {
        string[] candidates = ["dotnet-tester", "dotnet-sdk-builder", "ef-core", "csharp-docs"];

        var hits = Suggestions.Closest("dotnet-test", candidates, max: 3);

        hits[0].Should().Be("dotnet-tester");
    }

    [Fact]
    public void Returns_empty_when_nothing_within_threshold()
    {
        var hits = Suggestions.Closest("zzzzz", ["abc", "def"], max: 3);
        hits.Should().BeEmpty();
    }

    [Fact]
    public void Limits_results_to_max()
    {
        string[] candidates = ["aa", "ab", "ac", "ad", "ae"];
        var hits = Suggestions.Closest("a", candidates, max: 2);
        hits.Should().HaveCountLessOrEqualTo(2);
    }
}
```

- [ ] **Step 2: Run — expect compile failure**

Run: `dotnet test --filter FullyQualifiedName~SuggestionsTests`
Expected: missing type.

- [ ] **Step 3: Implement `Suggestions`**

Create `src/AiSetupLib/Deploy/Suggestions.cs`:

```csharp
namespace AiSetupLib.Deploy;

public static class Suggestions
{
    public static IReadOnlyList<string> Closest(string query, IEnumerable<string> candidates, int max = 3)
    {
        var threshold = Math.Max(2, query.Length / 2);
        return candidates
            .Select(c => (Name: c, Distance: Levenshtein(query, c)))
            .Where(t => t.Distance <= threshold)
            .OrderBy(t => t.Distance)
            .Take(max)
            .Select(t => t.Name)
            .ToList();
    }

    private static int Levenshtein(string a, string b)
    {
        if (a.Length == 0) return b.Length;
        if (b.Length == 0) return a.Length;
        var prev = new int[b.Length + 1];
        var curr = new int[b.Length + 1];
        for (var j = 0; j <= b.Length; j++) prev[j] = j;
        for (var i = 1; i <= a.Length; i++)
        {
            curr[0] = i;
            for (var j = 1; j <= b.Length; j++)
            {
                var cost = a[i - 1] == b[j - 1] ? 0 : 1;
                curr[j] = Math.Min(Math.Min(curr[j - 1] + 1, prev[j] + 1), prev[j - 1] + cost);
            }
            (prev, curr) = (curr, prev);
        }
        return prev[b.Length];
    }
}
```

- [ ] **Step 4: Run — expect pass**

Run: `dotnet test --filter FullyQualifiedName~SuggestionsTests`
Expected: all 3 tests pass.

- [ ] **Step 5: Commit**

```bash
git add src/AiSetupLib/Deploy tests/AiSetupLib.Tests/Deploy
git commit -m "feat: add Suggestions helper for closest-name matching via Levenshtein"
```

---

## Task 11: IDeployTarget interface + CopilotCliTarget

`IDeployTarget.Plan(IReadOnlyList<AssetDefinition>, DeployOptions)` returns a `DeployPlan` describing the actions. `Apply(DeployPlan, DeployOptions, IReadOnlyList<AssetDefinition>)` performs the writes.

`CopilotCliTarget` writes individual files (one per asset) under `.github/instructions`, `.github/skills/<name>/SKILL.md`, `.github/agents/<name>.md` for repo mode, or under the local config root for local mode. MCP configs go to `.vscode/mcp.json` (merged JSON object).

**Files:**
- Create: `src/AiSetupLib/Targets/IDeployTarget.cs`
- Create: `src/AiSetupLib/Targets/CopilotCliTarget.cs`
- Test: `tests/AiSetupLib.Tests/Targets/CopilotCliTargetTests.cs`

- [ ] **Step 1: Define interface**

Create `src/AiSetupLib/Targets/IDeployTarget.cs`:

```csharp
using AiSetupLib.Models;

namespace AiSetupLib.Targets;

public interface IDeployTarget
{
    DeployTarget Target { get; }
    DeployPlan Plan(IReadOnlyList<AssetDefinition> assets, DeployOptions options);
    void Apply(DeployPlan plan, IReadOnlyList<AssetDefinition> assets, DeployOptions options);
}
```

- [ ] **Step 2: Write failing tests for `CopilotCliTarget` (repo mode)**

Create `tests/AiSetupLib.Tests/Targets/CopilotCliTargetTests.cs`:

```csharp
using System.IO.Abstractions.TestingHelpers;
using System.Text.Json;
using AiSetupLib.Aggregation;
using AiSetupLib.Models;
using AiSetupLib.Paths;
using AiSetupLib.Targets;

namespace AiSetupLib.Tests.Targets;

public class CopilotCliTargetTests
{
    private const string Dest = "/work/target-repo";

    private static AssetDefinition Asset(string name, AssetType type, string body = "body")
        => new(name, "desc", type, [], [DeployTarget.CopilotCli], null, $"/src/{name}", body);

    private static (CopilotCliTarget target, MockFileSystem fs) Make()
    {
        var fs = new MockFileSystem();
        var target = new CopilotCliTarget(fs, new PathProvider(home: "/home/me"), new McpConfigMerger());
        return (target, fs);
    }

    [Fact]
    public void Plan_repo_mode_creates_instruction_file_under_github_instructions()
    {
        var (target, fs) = Make();
        var assets = new[] { Asset("csharp/csharp.instructions", AssetType.Instruction, "rules") };
        var opts = new DeployOptions(DeployTarget.CopilotCli, DeployMode.Repo, Dest);

        var plan = target.Plan(assets, opts);

        plan.Actions.Should().ContainSingle().Which.Should().Match<DeployAction>(a =>
            a.Kind == DeployActionKind.Create
            && a.TargetPath == "/work/target-repo/.github/instructions/csharp/csharp.instructions.md");
    }

    [Fact]
    public void Apply_repo_mode_writes_instruction_body_to_disk()
    {
        var (target, fs) = Make();
        var assets = new[] { Asset("csharp/csharp.instructions", AssetType.Instruction, "rules go here") };
        var opts = new DeployOptions(DeployTarget.CopilotCli, DeployMode.Repo, Dest);

        var plan = target.Plan(assets, opts);
        target.Apply(plan, assets, opts);

        var path = "/work/target-repo/.github/instructions/csharp/csharp.instructions.md";
        fs.File.Exists(path).Should().BeTrue();
        fs.File.ReadAllText(path).Should().Contain("rules go here");
    }

    [Fact]
    public void Apply_repo_mode_writes_skill_under_skill_md_in_subfolder()
    {
        var (target, fs) = Make();
        var assets = new[] { Asset("csharp/dotnet-tester", AssetType.Skill, "skill body") };
        var opts = new DeployOptions(DeployTarget.CopilotCli, DeployMode.Repo, Dest);

        var plan = target.Plan(assets, opts);
        target.Apply(plan, assets, opts);

        var path = "/work/target-repo/.github/skills/csharp/dotnet-tester/SKILL.md";
        fs.File.Exists(path).Should().BeTrue();
    }

    [Fact]
    public void Apply_repo_mode_writes_agent_under_github_agents()
    {
        var (target, fs) = Make();
        var assets = new[] { Asset("dotnet-developer", AssetType.Agent, "agent body") };
        var opts = new DeployOptions(DeployTarget.CopilotCli, DeployMode.Repo, Dest);

        target.Apply(target.Plan(assets, opts), assets, opts);

        fs.File.Exists("/work/target-repo/.github/agents/dotnet-developer.md").Should().BeTrue();
    }

    [Fact]
    public void Apply_repo_mode_merges_mcp_configs_into_vscode_mcp_json()
    {
        var (target, fs) = Make();
        var assets = new[]
        {
            Asset("github", AssetType.McpConfig, "command: gh-mcp"),
            Asset("filesystem", AssetType.McpConfig, "command: fs-mcp"),
        };
        var opts = new DeployOptions(DeployTarget.CopilotCli, DeployMode.Repo, Dest);

        target.Apply(target.Plan(assets, opts), assets, opts);

        var mcpPath = "/work/target-repo/.vscode/mcp.json";
        fs.File.Exists(mcpPath).Should().BeTrue();
        var json = JsonDocument.Parse(fs.File.ReadAllText(mcpPath));
        json.RootElement.GetProperty("servers").GetProperty("github").GetProperty("command")
            .GetString().Should().Be("gh-mcp");
    }

    [Fact]
    public void Plan_marks_existing_target_as_overwrite_when_force_false()
    {
        var (target, fs) = Make();
        var existing = "/work/target-repo/.github/instructions/x.md";
        fs.AddFile(existing, new MockFileData("old content"));

        var assets = new[] { Asset("x", AssetType.Instruction, "new") };
        var opts = new DeployOptions(DeployTarget.CopilotCli, DeployMode.Repo, Dest);

        var plan = target.Plan(assets, opts);

        plan.Actions.Should().ContainSingle()
            .Which.Kind.Should().Be(DeployActionKind.Overwrite);
    }

    [Fact]
    public void Plan_local_mode_uses_local_root_from_path_provider()
    {
        var fs = new MockFileSystem();
        var target = new CopilotCliTarget(
            fs,
            new PathProvider(home: "/home/me", platform: PlatformKind.Linux),
            new McpConfigMerger());
        var assets = new[] { Asset("x", AssetType.Instruction, "body") };
        var opts = new DeployOptions(DeployTarget.CopilotCli, DeployMode.Local, "");

        var plan = target.Plan(assets, opts);

        plan.Actions[0].TargetPath.Should()
            .Be("/home/me/.config/github-copilot/instructions/x.md");
    }
}
```

- [ ] **Step 3: Run — expect compile failure**

Run: `dotnet test --filter FullyQualifiedName~CopilotCliTargetTests`
Expected: missing type `CopilotCliTarget`.

- [ ] **Step 4: Implement `CopilotCliTarget`**

Create `src/AiSetupLib/Targets/CopilotCliTarget.cs`:

```csharp
using System.IO.Abstractions;
using System.Text.Json;
using AiSetupLib.Aggregation;
using AiSetupLib.Models;
using AiSetupLib.Paths;

namespace AiSetupLib.Targets;

public sealed class CopilotCliTarget : IDeployTarget
{
    private readonly IFileSystem _fs;
    private readonly IPathProvider _paths;
    private readonly McpConfigMerger _mcpMerger;

    public DeployTarget Target => DeployTarget.CopilotCli;

    public CopilotCliTarget(IFileSystem fs, IPathProvider paths, McpConfigMerger mcpMerger)
    {
        _fs = fs;
        _paths = paths;
        _mcpMerger = mcpMerger;
    }

    public DeployPlan Plan(IReadOnlyList<AssetDefinition> assets, DeployOptions options)
    {
        var actions = new List<DeployAction>();
        foreach (var asset in assets.Where(a => a.Type != AssetType.McpConfig))
        {
            var path = ResolveAssetPath(asset, options);
            actions.Add(new DeployAction(
                Kind: _fs.File.Exists(path) ? DeployActionKind.Overwrite : DeployActionKind.Create,
                TargetPath: path,
                SourceAssets: [asset.Name]));
        }
        var mcpAssets = assets.Where(a => a.Type == AssetType.McpConfig).ToList();
        if (mcpAssets.Count > 0)
        {
            var mcpPath = ResolveMcpPath(options);
            actions.Add(new DeployAction(
                Kind: _fs.File.Exists(mcpPath) ? DeployActionKind.Overwrite : DeployActionKind.Create,
                TargetPath: mcpPath,
                SourceAssets: mcpAssets.Select(a => a.Name).ToList()));
        }
        return new DeployPlan(actions);
    }

    public void Apply(DeployPlan plan, IReadOnlyList<AssetDefinition> assets, DeployOptions options)
    {
        var byName = assets.ToDictionary(a => a.Name, StringComparer.Ordinal);

        foreach (var action in plan.Actions)
        {
            if (action.Kind == DeployActionKind.Skip || action.Kind == DeployActionKind.Error)
                continue;

            EnsureDirectory(action.TargetPath);

            // MCP merged file = action whose source list maps to mcp assets
            var sources = action.SourceAssets.Select(n => byName[n]).ToList();
            if (sources[0].Type == AssetType.McpConfig)
            {
                var merged = _mcpMerger.Merge(sources);
                var json = JsonSerializer.Serialize(
                    new Dictionary<string, object?> { ["servers"] = merged },
                    new JsonSerializerOptions { WriteIndented = true });
                _fs.File.WriteAllText(action.TargetPath, json);
            }
            else
            {
                _fs.File.WriteAllText(action.TargetPath, sources[0].Body);
            }
        }
    }

    private string ResolveAssetPath(AssetDefinition asset, DeployOptions options)
    {
        var root = options.Mode == DeployMode.Repo
            ? _fs.Path.Combine(options.DestinationPath, _paths.GetRepoSubPath(DeployTarget.CopilotCli, asset.Type))
            : _fs.Path.Combine(_paths.GetLocalRoot(DeployTarget.CopilotCli), TypeFolder(asset.Type));

        return asset.Type switch
        {
            AssetType.Skill => _fs.Path.Combine(root, asset.Name, "SKILL.md"),
            _ => _fs.Path.Combine(root, asset.Name + ".md"),
        };
    }

    private string ResolveMcpPath(DeployOptions options)
    {
        if (options.Mode == DeployMode.Repo)
            return _fs.Path.Combine(options.DestinationPath, _paths.GetMcpSettingsRelativePath(DeployTarget.CopilotCli));
        return _fs.Path.Combine(_paths.GetLocalRoot(DeployTarget.CopilotCli), "mcp.json");
    }

    private static string TypeFolder(AssetType type) => type switch
    {
        AssetType.Instruction => "instructions",
        AssetType.Skill => "skills",
        AssetType.Agent => "agents",
        AssetType.McpConfig => "",
        _ => "",
    };

    private void EnsureDirectory(string path)
    {
        var dir = _fs.Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir) && !_fs.Directory.Exists(dir))
            _fs.Directory.CreateDirectory(dir);
    }
}
```

- [ ] **Step 5: Run — expect pass**

Run: `dotnet test --filter FullyQualifiedName~CopilotCliTargetTests`
Expected: all 7 tests pass.

- [ ] **Step 6: Commit**

```bash
git add src/AiSetupLib/Targets tests/AiSetupLib.Tests/Targets
git commit -m "feat: add CopilotCliTarget writing per-file assets and merged mcp.json"
```

---

## Task 12: ClaudeCodeTarget

For Claude Code: aggregates instructions + agents into a single `CLAUDE.md` (placed at `<dest>/CLAUDE.md` for repo mode or `<home>/.claude/CLAUDE.md` for local). Skills go to `.claude/skills/<name>/SKILL.md` (repo) or `~/.claude/commands/<name>.md` (local). MCP configs merge into `.claude/settings.json` `mcpServers` block.

**Files:**
- Create: `src/AiSetupLib/Targets/ClaudeCodeTarget.cs`
- Test: `tests/AiSetupLib.Tests/Targets/ClaudeCodeTargetTests.cs`

- [ ] **Step 1: Write failing tests**

Create `tests/AiSetupLib.Tests/Targets/ClaudeCodeTargetTests.cs`:

```csharp
using System.IO.Abstractions.TestingHelpers;
using System.Text.Json;
using AiSetupLib.Aggregation;
using AiSetupLib.Models;
using AiSetupLib.Paths;
using AiSetupLib.Targets;

namespace AiSetupLib.Tests.Targets;

public class ClaudeCodeTargetTests
{
    private const string Dest = "/work/target-repo";

    private static AssetDefinition Asset(string name, AssetType type, string body = "body")
        => new(name, "desc", type, [], [DeployTarget.ClaudeCode], null, $"/src/{name}", body);

    private static (ClaudeCodeTarget target, MockFileSystem fs) Make()
    {
        var fs = new MockFileSystem();
        var target = new ClaudeCodeTarget(
            fs,
            new PathProvider(home: "/home/me"),
            new MarkdownAggregator(),
            new McpConfigMerger());
        return (target, fs);
    }

    [Fact]
    public void Apply_repo_mode_aggregates_instructions_and_agents_into_claude_md()
    {
        var (target, fs) = Make();
        var assets = new[]
        {
            Asset("csharp/rules", AssetType.Instruction, "Use sealed."),
            Asset("dotnet-developer", AssetType.Agent, "Agent."),
        };
        var opts = new DeployOptions(DeployTarget.ClaudeCode, DeployMode.Repo, Dest);

        target.Apply(target.Plan(assets, opts), assets, opts);

        var path = "/work/target-repo/CLAUDE.md";
        fs.File.Exists(path).Should().BeTrue();
        var content = fs.File.ReadAllText(path);
        content.Should().Contain("Use sealed.");
        content.Should().Contain("Agent.");
    }

    [Fact]
    public void Apply_repo_mode_writes_skills_under_dot_claude_skills()
    {
        var (target, fs) = Make();
        var assets = new[] { Asset("csharp/dotnet-tester", AssetType.Skill, "skill body") };
        var opts = new DeployOptions(DeployTarget.ClaudeCode, DeployMode.Repo, Dest);

        target.Apply(target.Plan(assets, opts), assets, opts);

        fs.File.Exists("/work/target-repo/.claude/skills/csharp/dotnet-tester/SKILL.md")
            .Should().BeTrue();
    }

    [Fact]
    public void Apply_repo_mode_merges_mcp_into_claude_settings_json()
    {
        var (target, fs) = Make();
        var assets = new[]
        {
            Asset("github", AssetType.McpConfig, "command: gh-mcp"),
        };
        var opts = new DeployOptions(DeployTarget.ClaudeCode, DeployMode.Repo, Dest);

        target.Apply(target.Plan(assets, opts), assets, opts);

        var json = JsonDocument.Parse(
            fs.File.ReadAllText("/work/target-repo/.claude/settings.json"));
        json.RootElement.GetProperty("mcpServers").GetProperty("github")
            .GetProperty("command").GetString().Should().Be("gh-mcp");
    }

    [Fact]
    public void Apply_local_mode_writes_claude_md_to_home_dot_claude()
    {
        var (target, fs) = Make();
        var assets = new[] { Asset("x", AssetType.Instruction, "rules") };
        var opts = new DeployOptions(DeployTarget.ClaudeCode, DeployMode.Local, "");

        target.Apply(target.Plan(assets, opts), assets, opts);

        fs.File.Exists("/home/me/.claude/CLAUDE.md").Should().BeTrue();
    }

    [Fact]
    public void Plan_emits_one_action_per_aggregate_or_per_skill()
    {
        var (target, _) = Make();
        var assets = new[]
        {
            Asset("a", AssetType.Instruction, "a"),
            Asset("b", AssetType.Agent, "b"),
            Asset("c", AssetType.Skill, "c"),
            Asset("d", AssetType.McpConfig, "command: x"),
        };
        var opts = new DeployOptions(DeployTarget.ClaudeCode, DeployMode.Repo, Dest);

        var plan = target.Plan(assets, opts);

        // 1 aggregated CLAUDE.md (a + b), 1 skill, 1 settings.json
        plan.Actions.Should().HaveCount(3);
    }

    [Fact]
    public void Plan_marks_existing_settings_json_as_overwrite()
    {
        var (target, fs) = Make();
        fs.AddFile("/work/target-repo/.claude/settings.json", new MockFileData("{}"));
        var assets = new[] { Asset("g", AssetType.McpConfig, "command: x") };
        var opts = new DeployOptions(DeployTarget.ClaudeCode, DeployMode.Repo, Dest);

        var action = target.Plan(assets, opts).Actions
            .Single(a => a.TargetPath.EndsWith("settings.json"));
        action.Kind.Should().Be(DeployActionKind.Overwrite);
    }
}
```

- [ ] **Step 2: Run — expect compile failure**

Run: `dotnet test --filter FullyQualifiedName~ClaudeCodeTargetTests`
Expected: missing type.

- [ ] **Step 3: Implement `ClaudeCodeTarget`**

Create `src/AiSetupLib/Targets/ClaudeCodeTarget.cs`:

```csharp
using System.IO.Abstractions;
using System.Text.Json;
using System.Text.Json.Nodes;
using AiSetupLib.Aggregation;
using AiSetupLib.Models;
using AiSetupLib.Paths;

namespace AiSetupLib.Targets;

public sealed class ClaudeCodeTarget : IDeployTarget
{
    private readonly IFileSystem _fs;
    private readonly IPathProvider _paths;
    private readonly IContentAggregator _aggregator;
    private readonly McpConfigMerger _mcpMerger;

    public DeployTarget Target => DeployTarget.ClaudeCode;

    public ClaudeCodeTarget(
        IFileSystem fs,
        IPathProvider paths,
        IContentAggregator aggregator,
        McpConfigMerger mcpMerger)
    {
        _fs = fs;
        _paths = paths;
        _aggregator = aggregator;
        _mcpMerger = mcpMerger;
    }

    public DeployPlan Plan(IReadOnlyList<AssetDefinition> assets, DeployOptions options)
    {
        var actions = new List<DeployAction>();

        var aggregated = assets
            .Where(a => a.Type == AssetType.Instruction || a.Type == AssetType.Agent)
            .ToList();
        if (aggregated.Count > 0)
        {
            var path = ResolveClaudeMdPath(options);
            actions.Add(new DeployAction(
                Kind: _fs.File.Exists(path) ? DeployActionKind.Overwrite : DeployActionKind.Create,
                TargetPath: path,
                SourceAssets: aggregated.Select(a => a.Name).ToList()));
        }

        foreach (var skill in assets.Where(a => a.Type == AssetType.Skill))
        {
            var path = ResolveSkillPath(skill, options);
            actions.Add(new DeployAction(
                Kind: _fs.File.Exists(path) ? DeployActionKind.Overwrite : DeployActionKind.Create,
                TargetPath: path,
                SourceAssets: [skill.Name]));
        }

        var mcps = assets.Where(a => a.Type == AssetType.McpConfig).ToList();
        if (mcps.Count > 0)
        {
            var path = ResolveSettingsPath(options);
            actions.Add(new DeployAction(
                Kind: _fs.File.Exists(path) ? DeployActionKind.Overwrite : DeployActionKind.Create,
                TargetPath: path,
                SourceAssets: mcps.Select(a => a.Name).ToList()));
        }

        return new DeployPlan(actions);
    }

    public void Apply(DeployPlan plan, IReadOnlyList<AssetDefinition> assets, DeployOptions options)
    {
        var byName = assets.ToDictionary(a => a.Name, StringComparer.Ordinal);

        foreach (var action in plan.Actions)
        {
            if (action.Kind == DeployActionKind.Skip || action.Kind == DeployActionKind.Error)
                continue;

            EnsureDirectory(action.TargetPath);
            var sources = action.SourceAssets.Select(n => byName[n]).ToList();

            if (sources[0].Type == AssetType.McpConfig)
            {
                WriteMergedSettings(action.TargetPath, sources);
            }
            else if (sources[0].Type == AssetType.Skill)
            {
                _fs.File.WriteAllText(action.TargetPath, sources[0].Body);
            }
            else
            {
                var aggregated = _aggregator.Aggregate(sources);
                _fs.File.WriteAllText(action.TargetPath, aggregated);
            }
        }
    }

    private void WriteMergedSettings(string path, IReadOnlyList<AssetDefinition> mcps)
    {
        var existing = _fs.File.Exists(path)
            ? JsonNode.Parse(_fs.File.ReadAllText(path)) as JsonObject ?? new JsonObject()
            : new JsonObject();

        var merged = _mcpMerger.Merge(mcps);
        var serversNode = JsonSerializer.SerializeToNode(merged);
        existing["mcpServers"] = serversNode;

        _fs.File.WriteAllText(path, existing.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
    }

    private string ResolveClaudeMdPath(DeployOptions options)
    {
        var fileName = _paths.GetClaudeAggregatedFileName();
        return options.Mode == DeployMode.Repo
            ? _fs.Path.Combine(options.DestinationPath, fileName)
            : _fs.Path.Combine(_paths.GetLocalRoot(DeployTarget.ClaudeCode), fileName);
    }

    private string ResolveSkillPath(AssetDefinition skill, DeployOptions options)
    {
        if (options.Mode == DeployMode.Repo)
        {
            var sub = _paths.GetRepoSubPath(DeployTarget.ClaudeCode, AssetType.Skill);
            return _fs.Path.Combine(options.DestinationPath, sub, skill.Name, "SKILL.md");
        }
        return _fs.Path.Combine(_paths.GetLocalRoot(DeployTarget.ClaudeCode), "commands", skill.Name + ".md");
    }

    private string ResolveSettingsPath(DeployOptions options)
    {
        return options.Mode == DeployMode.Repo
            ? _fs.Path.Combine(options.DestinationPath, _paths.GetMcpSettingsRelativePath(DeployTarget.ClaudeCode))
            : _fs.Path.Combine(_paths.GetLocalRoot(DeployTarget.ClaudeCode), "settings.json");
    }

    private void EnsureDirectory(string path)
    {
        var dir = _fs.Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir) && !_fs.Directory.Exists(dir))
            _fs.Directory.CreateDirectory(dir);
    }
}
```

- [ ] **Step 4: Run — expect pass**

Run: `dotnet test --filter FullyQualifiedName~ClaudeCodeTargetTests`
Expected: all 6 tests pass.

- [ ] **Step 5: Commit**

```bash
git add src/AiSetupLib/Targets tests/AiSetupLib.Tests/Targets
git commit -m "feat: add ClaudeCodeTarget aggregating CLAUDE.md and merging settings.json"
```

---

## Task 13: TargetRegistry — selects the right `IDeployTarget` for a `DeployTarget`

**Files:**
- Create: `src/AiSetupLib/Targets/TargetRegistry.cs`
- Test: `tests/AiSetupLib.Tests/Targets/TargetRegistryTests.cs`

- [ ] **Step 1: Write failing tests**

Create `tests/AiSetupLib.Tests/Targets/TargetRegistryTests.cs`:

```csharp
using AiSetupLib.Models;
using AiSetupLib.Targets;

namespace AiSetupLib.Tests.Targets;

public class TargetRegistryTests
{
    [Fact]
    public void Resolves_target_by_enum()
    {
        var copilot = A.Fake<IDeployTarget>();
        A.CallTo(() => copilot.Target).Returns(DeployTarget.CopilotCli);
        var claude = A.Fake<IDeployTarget>();
        A.CallTo(() => claude.Target).Returns(DeployTarget.ClaudeCode);

        var registry = new TargetRegistry([copilot, claude]);

        registry.Get(DeployTarget.CopilotCli).Should().BeSameAs(copilot);
        registry.Get(DeployTarget.ClaudeCode).Should().BeSameAs(claude);
    }

    [Fact]
    public void Throws_when_target_not_registered()
    {
        var registry = new TargetRegistry([]);
        var act = () => registry.Get(DeployTarget.CopilotCli);
        act.Should().Throw<InvalidOperationException>();
    }
}
```

- [ ] **Step 2: Run — expect compile failure**

Run: `dotnet test --filter FullyQualifiedName~TargetRegistryTests`
Expected: missing type.

- [ ] **Step 3: Implement `TargetRegistry`**

Create `src/AiSetupLib/Targets/TargetRegistry.cs`:

```csharp
using AiSetupLib.Models;

namespace AiSetupLib.Targets;

public sealed class TargetRegistry
{
    private readonly Dictionary<DeployTarget, IDeployTarget> _byTarget;

    public TargetRegistry(IEnumerable<IDeployTarget> targets)
    {
        _byTarget = targets.ToDictionary(t => t.Target);
    }

    public IDeployTarget Get(DeployTarget target)
    {
        if (_byTarget.TryGetValue(target, out var t)) return t;
        throw new InvalidOperationException($"No IDeployTarget registered for {target}");
    }
}
```

- [ ] **Step 4: Run — expect pass**

Run: `dotnet test --filter FullyQualifiedName~TargetRegistryTests`
Expected: all 2 tests pass.

- [ ] **Step 5: Commit**

```bash
git add src/AiSetupLib/Targets tests/AiSetupLib.Tests/Targets
git commit -m "feat: add TargetRegistry for IDeployTarget lookup"
```

---

## Task 14: DeployService — orchestrates discovery, profile resolution, target dispatch

Single public entry point. Resolves a profile (if given), unions explicit asset selections from `DeployOptions`, runs discovery, filters to selected names, returns plan; if not dry-run, applies. Throws `AssetNotFoundException` listing missing names with closest-match suggestions.

**Files:**
- Create: `src/AiSetupLib/Deploy/IDeployService.cs`
- Create: `src/AiSetupLib/Deploy/DeployService.cs`
- Create: `src/AiSetupLib/Deploy/AssetNotFoundException.cs`
- Test: `tests/AiSetupLib.Tests/Deploy/DeployServiceTests.cs`

- [ ] **Step 1: Write failing tests**

Create `tests/AiSetupLib.Tests/Deploy/DeployServiceTests.cs`:

```csharp
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
        return (new DeployService(disc, pr, registry, "/repo"), disc, pr, tgt);
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
```

- [ ] **Step 2: Run — expect compile failure**

Run: `dotnet test --filter FullyQualifiedName~DeployServiceTests`
Expected: missing types.

- [ ] **Step 3: Implement interface, exception, service**

Create `src/AiSetupLib/Deploy/IDeployService.cs`:

```csharp
using AiSetupLib.Models;

namespace AiSetupLib.Deploy;

public interface IDeployService
{
    DeployPlan Deploy(DeployOptions options);
}
```

Create `src/AiSetupLib/Deploy/AssetNotFoundException.cs`:

```csharp
namespace AiSetupLib.Deploy;

public sealed class AssetNotFoundException : Exception
{
    public IReadOnlyList<string> MissingNames { get; }
    public IReadOnlyDictionary<string, IReadOnlyList<string>> Suggestions { get; }

    public AssetNotFoundException(
        IReadOnlyList<string> missing,
        IReadOnlyDictionary<string, IReadOnlyList<string>> suggestions)
        : base(BuildMessage(missing, suggestions))
    {
        MissingNames = missing;
        Suggestions = suggestions;
    }

    private static string BuildMessage(
        IReadOnlyList<string> missing,
        IReadOnlyDictionary<string, IReadOnlyList<string>> suggestions)
    {
        var parts = missing.Select(m =>
        {
            var hits = suggestions.TryGetValue(m, out var s) ? s : [];
            return hits.Count == 0
                ? $"  - {m}"
                : $"  - {m} (did you mean: {string.Join(", ", hits)}?)";
        });
        return "The following assets were not found:\n" + string.Join('\n', parts);
    }
}
```

Create `src/AiSetupLib/Deploy/DeployService.cs`:

```csharp
using AiSetupLib.Discovery;
using AiSetupLib.Models;
using AiSetupLib.Profiles;
using AiSetupLib.Targets;

namespace AiSetupLib.Deploy;

public sealed class DeployService : IDeployService
{
    private readonly IAssetDiscovery _discovery;
    private readonly IProfileResolver _profiles;
    private readonly TargetRegistry _registry;
    private readonly string _repoRoot;

    public DeployService(
        IAssetDiscovery discovery,
        IProfileResolver profiles,
        TargetRegistry registry,
        string repoRoot)
    {
        _discovery = discovery;
        _profiles = profiles;
        _registry = registry;
        _repoRoot = repoRoot;
    }

    public DeployPlan Deploy(DeployOptions options)
    {
        var requested = ResolveRequested(options);
        var available = _discovery.Discover(_repoRoot);

        var byName = available.ToDictionary(a => a.Name, StringComparer.Ordinal);
        var resolved = new List<AssetDefinition>();
        var missing = new List<string>();

        foreach (var name in requested)
        {
            if (byName.TryGetValue(name, out var asset)) resolved.Add(asset);
            else missing.Add(name);
        }

        if (missing.Count > 0)
        {
            var allNames = available.Select(a => a.Name).ToList();
            var suggestions = missing.ToDictionary(
                m => m,
                m => Suggestions.Closest(m, allNames));
            throw new AssetNotFoundException(missing, suggestions);
        }

        var target = _registry.Get(options.Target);
        var plan = target.Plan(resolved, options);
        if (!options.DryRun)
            target.Apply(plan, resolved, options);
        return plan;
    }

    private IReadOnlyList<string> ResolveRequested(DeployOptions options)
    {
        var names = new List<string>();
        if (options.Profile is not null)
        {
            var profile = _profiles.Resolve(_repoRoot, options.Profile);
            names.AddRange(profile.Agents);
            names.AddRange(profile.Instructions);
            names.AddRange(profile.Skills);
            names.AddRange(profile.McpConfigs);
        }
        names.AddRange(options.Agents);
        names.AddRange(options.Instructions);
        names.AddRange(options.Skills);
        names.AddRange(options.McpConfigs);
        return names.Distinct(StringComparer.Ordinal).ToList();
    }
}
```

- [ ] **Step 4: Run — expect pass**

Run: `dotnet test --filter FullyQualifiedName~DeployServiceTests`
Expected: all 5 tests pass.

- [ ] **Step 5: Commit**

```bash
git add src/AiSetupLib/Deploy tests/AiSetupLib.Tests/Deploy
git commit -m "feat: add DeployService orchestrating profile, discovery, target"
```

---

## Task 15: DI composition root for the library

**Files:**
- Create: `src/AiSetupLib/AiSetupServices.cs`
- Test: `tests/AiSetupLib.Tests/AiSetupServicesTests.cs`

- [ ] **Step 1: Write failing tests**

Create `tests/AiSetupLib.Tests/AiSetupServicesTests.cs`:

```csharp
using AiSetupLib;
using AiSetupLib.Deploy;
using AiSetupLib.Discovery;
using AiSetupLib.Profiles;
using AiSetupLib.Targets;
using Microsoft.Extensions.DependencyInjection;

namespace AiSetupLib.Tests;

public class AiSetupServicesTests
{
    [Fact]
    public void AddAiSetup_registers_all_required_services()
    {
        var services = new ServiceCollection();
        services.AddAiSetup(repoRoot: "/repo");

        var sp = services.BuildServiceProvider();

        sp.GetService<IAssetDiscovery>().Should().NotBeNull();
        sp.GetService<IProfileResolver>().Should().NotBeNull();
        sp.GetService<IDeployService>().Should().NotBeNull();
        sp.GetService<TargetRegistry>().Should().NotBeNull();
        sp.GetServices<IDeployTarget>().Should().HaveCount(2);
    }
}
```

- [ ] **Step 2: Run — expect compile failure**

Run: `dotnet test --filter FullyQualifiedName~AiSetupServicesTests`
Expected: missing extension method.

- [ ] **Step 3: Implement extension method**

Create `src/AiSetupLib/AiSetupServices.cs`:

```csharp
using System.IO.Abstractions;
using AiSetupLib.Aggregation;
using AiSetupLib.Deploy;
using AiSetupLib.Discovery;
using AiSetupLib.Paths;
using AiSetupLib.Profiles;
using AiSetupLib.Targets;
using Microsoft.Extensions.DependencyInjection;

namespace AiSetupLib;

public static class AiSetupServices
{
    public static IServiceCollection AddAiSetup(this IServiceCollection services, string repoRoot)
    {
        services.AddSingleton<IFileSystem, FileSystem>();
        services.AddSingleton<FrontmatterParser>();
        services.AddSingleton<IAssetDiscovery, AssetDiscoveryService>();
        services.AddSingleton<IProfileResolver, ProfileResolver>();
        services.AddSingleton<IPathProvider>(_ => new PathProvider());
        services.AddSingleton<IContentAggregator, MarkdownAggregator>();
        services.AddSingleton<McpConfigMerger>();
        services.AddSingleton<IDeployTarget, CopilotCliTarget>();
        services.AddSingleton<IDeployTarget, ClaudeCodeTarget>();
        services.AddSingleton<TargetRegistry>(sp => new TargetRegistry(sp.GetServices<IDeployTarget>()));
        services.AddSingleton<IDeployService>(sp => new DeployService(
            sp.GetRequiredService<IAssetDiscovery>(),
            sp.GetRequiredService<IProfileResolver>(),
            sp.GetRequiredService<TargetRegistry>(),
            repoRoot));
        return services;
    }
}
```

- [ ] **Step 4: Run — expect pass**

Run: `dotnet test --filter FullyQualifiedName~AiSetupServicesTests`
Expected: pass.

- [ ] **Step 5: Commit**

```bash
git add src/AiSetupLib/AiSetupServices.cs tests/AiSetupLib.Tests/AiSetupServicesTests.cs
git commit -m "feat: add AddAiSetup DI registration"
```

---

## Task 16: Spectre.Console.Cli host — type registrar/resolver bridging DI

Spectre.Console.Cli needs a `ITypeRegistrar` + `ITypeResolver` to use Microsoft.Extensions.DependencyInjection. Boilerplate documented in the Spectre docs; mirror it once.

**Files:**
- Create: `src/AiSetupCli/Infrastructure/SpectreTypeRegistrar.cs`
- Create: `src/AiSetupCli/Infrastructure/SpectreTypeResolver.cs`

- [ ] **Step 1: Write `SpectreTypeRegistrar`**

Create `src/AiSetupCli/Infrastructure/SpectreTypeRegistrar.cs`:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

namespace AiSetupCli.Infrastructure;

internal sealed class SpectreTypeRegistrar : ITypeRegistrar
{
    private readonly IServiceCollection _services;

    public SpectreTypeRegistrar(IServiceCollection services) => _services = services;

    public ITypeResolver Build() => new SpectreTypeResolver(_services.BuildServiceProvider());

    public void Register(Type service, Type implementation)
        => _services.AddSingleton(service, implementation);

    public void RegisterInstance(Type service, object implementation)
        => _services.AddSingleton(service, implementation);

    public void RegisterLazy(Type service, Func<object> func)
        => _services.AddSingleton(service, _ => func());
}
```

- [ ] **Step 2: Write `SpectreTypeResolver`**

Create `src/AiSetupCli/Infrastructure/SpectreTypeResolver.cs`:

```csharp
using Spectre.Console.Cli;

namespace AiSetupCli.Infrastructure;

internal sealed class SpectreTypeResolver : ITypeResolver, IDisposable
{
    private readonly IServiceProvider _provider;

    public SpectreTypeResolver(IServiceProvider provider) => _provider = provider;

    public object? Resolve(Type? type) => type is null ? null : _provider.GetService(type);

    public void Dispose()
    {
        if (_provider is IDisposable d) d.Dispose();
    }
}
```

- [ ] **Step 3: Build to verify**

Run: `dotnet build src/AiSetupCli/AiSetupCli.csproj`
Expected: build succeeds.

- [ ] **Step 4: Commit**

```bash
git add src/AiSetupCli/Infrastructure
git commit -m "feat: add Spectre type registrar/resolver bridging Microsoft DI"
```

---

## Task 17: DryRunRenderer — colored Spectre console output for `DeployPlan`

**Files:**
- Create: `src/AiSetupCli/Output/DryRunRenderer.cs`

- [ ] **Step 1: Implement renderer**

Create `src/AiSetupCli/Output/DryRunRenderer.cs`:

```csharp
using AiSetupLib.Models;
using Spectre.Console;

namespace AiSetupCli.Output;

internal static class DryRunRenderer
{
    public static void Render(IAnsiConsole console, DeployPlan plan)
    {
        var table = new Table().AddColumn("Action").AddColumn("Target").AddColumn("Sources");

        foreach (var action in plan.Actions)
        {
            var color = action.Kind switch
            {
                DeployActionKind.Create => "green",
                DeployActionKind.Overwrite => "yellow",
                DeployActionKind.Skip => "grey",
                DeployActionKind.Error => "red",
                _ => "white",
            };
            table.AddRow(
                $"[{color}]{action.Kind}[/]",
                Markup.Escape(action.TargetPath),
                Markup.Escape(string.Join(", ", action.SourceAssets)));
        }

        console.Write(table);
    }
}
```

- [ ] **Step 2: Build to verify**

Run: `dotnet build src/AiSetupCli/AiSetupCli.csproj`
Expected: success.

- [ ] **Step 3: Commit**

```bash
git add src/AiSetupCli/Output
git commit -m "feat: add DryRunRenderer for colored deploy plan output"
```

---

## Task 18: DeployCommand

**Files:**
- Create: `src/AiSetupCli/Commands/DeployCommand.cs`

- [ ] **Step 1: Implement `DeployCommand`**

Create `src/AiSetupCli/Commands/DeployCommand.cs`:

```csharp
using System.ComponentModel;
using AiSetupCli.Output;
using AiSetupLib.Deploy;
using AiSetupLib.Models;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AiSetupCli.Commands;

internal sealed class DeployCommand : Command<DeployCommand.Settings>
{
    private readonly IDeployService _service;
    private readonly IAnsiConsole _console;

    public DeployCommand(IDeployService service, IAnsiConsole console)
    {
        _service = service;
        _console = console;
    }

    public sealed class Settings : CommandSettings
    {
        [CommandOption("--target <TARGET>")]
        [Description("copilot-cli | claude-code")]
        public string Target { get; init; } = "";

        [CommandOption("--mode <MODE>")]
        [Description("repo | local")]
        public string Mode { get; init; } = "";

        [CommandOption("--repo <PATH>")]
        public string? RepoPath { get; init; }

        [CommandOption("--profile <NAME>")]
        public string? Profile { get; init; }

        [CommandOption("--agents <CSV>")]
        public string? Agents { get; init; }

        [CommandOption("--skills <CSV>")]
        public string? Skills { get; init; }

        [CommandOption("--instructions <CSV>")]
        public string? Instructions { get; init; }

        [CommandOption("--mcp-configs <CSV>")]
        public string? McpConfigs { get; init; }

        [CommandOption("--dry-run")]
        public bool DryRun { get; init; }

        [CommandOption("--force")]
        public bool Force { get; init; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        if (!TryParseTarget(settings.Target, out var target))
        {
            _console.MarkupLine($"[red]Unknown --target: {Markup.Escape(settings.Target)}[/]");
            return 2;
        }
        if (!TryParseMode(settings.Mode, out var mode))
        {
            _console.MarkupLine($"[red]Unknown --mode: {Markup.Escape(settings.Mode)}[/]");
            return 2;
        }

        var dest = mode == DeployMode.Repo
            ? settings.RepoPath ?? Directory.GetCurrentDirectory()
            : "";

        var options = new DeployOptions(target, mode, dest)
        {
            Profile = settings.Profile,
            Agents = Csv(settings.Agents),
            Skills = Csv(settings.Skills),
            Instructions = Csv(settings.Instructions),
            McpConfigs = Csv(settings.McpConfigs),
            DryRun = settings.DryRun,
            Force = settings.Force,
        };

        try
        {
            var plan = _service.Deploy(options);
            DryRunRenderer.Render(_console, plan);
            if (settings.DryRun)
                _console.MarkupLine("[grey]Dry run — nothing was written.[/]");
            return 0;
        }
        catch (AssetNotFoundException ex)
        {
            _console.MarkupLine("[red]" + Markup.Escape(ex.Message) + "[/]");
            return 1;
        }
        catch (FileNotFoundException ex)
        {
            _console.MarkupLine("[red]" + Markup.Escape(ex.Message) + "[/]");
            return 1;
        }
    }

    private static IReadOnlyList<string> Csv(string? value)
        => string.IsNullOrWhiteSpace(value)
            ? []
            : value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static bool TryParseTarget(string s, out DeployTarget t)
    {
        t = default;
        return s switch
        {
            "copilot-cli" => (t = DeployTarget.CopilotCli) == DeployTarget.CopilotCli,
            "claude-code" => (t = DeployTarget.ClaudeCode) == DeployTarget.ClaudeCode,
            _ => false,
        };
    }

    private static bool TryParseMode(string s, out DeployMode m)
    {
        m = default;
        return s switch
        {
            "repo" => (m = DeployMode.Repo) == DeployMode.Repo,
            "local" => (m = DeployMode.Local) == DeployMode.Local,
            _ => false,
        };
    }
}
```

- [ ] **Step 2: Build to verify**

Run: `dotnet build src/AiSetupCli/AiSetupCli.csproj`
Expected: success.

- [ ] **Step 3: Commit**

```bash
git add src/AiSetupCli/Commands
git commit -m "feat: add DeployCommand with target/mode parsing and dry-run output"
```

---

## Task 19: ListCommand and InfoCommand

**Files:**
- Create: `src/AiSetupCli/Commands/ListCommand.cs`
- Create: `src/AiSetupCli/Commands/InfoCommand.cs`

- [ ] **Step 1: Implement `ListCommand`**

Create `src/AiSetupCli/Commands/ListCommand.cs`:

```csharp
using System.ComponentModel;
using AiSetupLib.Discovery;
using AiSetupLib.Models;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AiSetupCli.Commands;

internal sealed class ListCommand : Command<ListCommand.Settings>
{
    private readonly IAssetDiscovery _discovery;
    private readonly IAnsiConsole _console;
    private readonly string _repoRoot;

    public ListCommand(IAssetDiscovery discovery, IAnsiConsole console, RepoRoot repoRoot)
    {
        _discovery = discovery;
        _console = console;
        _repoRoot = repoRoot.Path;
    }

    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "[KIND]")]
        [Description("agents | skills | instructions | mcp-configs | profiles")]
        public string? Kind { get; init; }

        [CommandOption("--tag <TAG>")]
        public string? Tag { get; init; }

        [CommandOption("--target <TARGET>")]
        public string? Target { get; init; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        var assets = _discovery.Discover(_repoRoot);

        var filtered = assets.AsEnumerable();
        if (settings.Kind is { } k && KindToType(k) is { } type)
            filtered = filtered.Where(a => a.Type == type);
        if (!string.IsNullOrEmpty(settings.Tag))
            filtered = filtered.Where(a => a.Tags.Contains(settings.Tag));
        if (settings.Target is { } t && TargetMap.TryGetValue(t, out var tgt))
            filtered = filtered.Where(a => a.Targets.Count == 0 || a.Targets.Contains(tgt));

        var table = new Table()
            .AddColumn("Type").AddColumn("Name").AddColumn("Tags").AddColumn("Description");
        foreach (var asset in filtered.OrderBy(a => a.Type).ThenBy(a => a.Name))
        {
            table.AddRow(
                asset.Type.ToString(),
                Markup.Escape(asset.Name),
                Markup.Escape(string.Join(",", asset.Tags)),
                Markup.Escape(asset.Description));
        }
        _console.Write(table);
        return 0;
    }

    private static AssetType? KindToType(string kind) => kind switch
    {
        "agents" => AssetType.Agent,
        "skills" => AssetType.Skill,
        "instructions" => AssetType.Instruction,
        "mcp-configs" => AssetType.McpConfig,
        _ => null,
    };

    private static readonly Dictionary<string, DeployTarget> TargetMap = new()
    {
        ["copilot-cli"] = DeployTarget.CopilotCli,
        ["claude-code"] = DeployTarget.ClaudeCode,
    };
}

internal sealed record RepoRoot(string Path);
```

- [ ] **Step 2: Implement `InfoCommand`**

Create `src/AiSetupCli/Commands/InfoCommand.cs`:

```csharp
using AiSetupLib.Discovery;
using AiSetupLib.Deploy;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AiSetupCli.Commands;

internal sealed class InfoCommand : Command<InfoCommand.Settings>
{
    private readonly IAssetDiscovery _discovery;
    private readonly IAnsiConsole _console;
    private readonly string _repoRoot;

    public InfoCommand(IAssetDiscovery discovery, IAnsiConsole console, RepoRoot repoRoot)
    {
        _discovery = discovery;
        _console = console;
        _repoRoot = repoRoot.Path;
    }

    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<NAME>")]
        public string Name { get; init; } = "";
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        var assets = _discovery.Discover(_repoRoot);
        var match = assets.FirstOrDefault(a => a.Name == settings.Name);
        if (match is null)
        {
            var hints = Suggestions.Closest(settings.Name, assets.Select(a => a.Name));
            _console.MarkupLine($"[red]Asset '{Markup.Escape(settings.Name)}' not found.[/]");
            if (hints.Count > 0)
                _console.MarkupLine($"[yellow]Did you mean: {string.Join(", ", hints.Select(Markup.Escape))}?[/]");
            return 1;
        }

        var grid = new Grid().AddColumn().AddColumn();
        grid.AddRow("[bold]Name[/]", Markup.Escape(match.Name));
        grid.AddRow("[bold]Type[/]", match.Type.ToString());
        grid.AddRow("[bold]Description[/]", Markup.Escape(match.Description));
        grid.AddRow("[bold]Tags[/]", Markup.Escape(string.Join(", ", match.Tags)));
        grid.AddRow("[bold]Targets[/]", Markup.Escape(string.Join(", ", match.Targets)));
        grid.AddRow("[bold]Source[/]", Markup.Escape(match.SourcePath));
        _console.Write(grid);
        return 0;
    }
}
```

- [ ] **Step 3: Build to verify**

Run: `dotnet build src/AiSetupCli/AiSetupCli.csproj`
Expected: success.

- [ ] **Step 4: Commit**

```bash
git add src/AiSetupCli/Commands
git commit -m "feat: add ListCommand and InfoCommand"
```

---

## Task 20: Wire CLI in `Program.cs`

**Files:**
- Modify: `src/AiSetupCli/Program.cs`

- [ ] **Step 1: Replace Program.cs**

Replace contents of `src/AiSetupCli/Program.cs` with:

```csharp
using AiSetupCli.Commands;
using AiSetupCli.Infrastructure;
using AiSetupLib;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;

var repoRoot = ResolveRepoRoot();

var services = new ServiceCollection();
services.AddAiSetup(repoRoot);
services.AddSingleton<IAnsiConsole>(_ => AnsiConsole.Console);
services.AddSingleton(new RepoRoot(repoRoot));

var registrar = new SpectreTypeRegistrar(services);
var app = new CommandApp(registrar);
app.Configure(config =>
{
    config.SetApplicationName("ai-setup");
    config.AddCommand<DeployCommand>("deploy");
    config.AddCommand<ListCommand>("list");
    config.AddCommand<InfoCommand>("info");
});

return await app.RunAsync(args);

static string ResolveRepoRoot()
{
    var env = Environment.GetEnvironmentVariable("AI_SETUP_REPO_ROOT");
    if (!string.IsNullOrEmpty(env)) return env;

    var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
    while (dir is not null)
    {
        if (Directory.Exists(Path.Combine(dir.FullName, "agents"))
            && Directory.Exists(Path.Combine(dir.FullName, "skills")))
            return dir.FullName;
        dir = dir.Parent;
    }
    return Directory.GetCurrentDirectory();
}
```

- [ ] **Step 2: Build the CLI**

Run: `dotnet build src/AiSetupCli/AiSetupCli.csproj`
Expected: build succeeds, no warnings (because `TreatWarningsAsErrors` is on).

- [ ] **Step 3: Smoke test the binary against this repo**

Run from the repo root:

```bash
dotnet run --project src/AiSetupCli -- list
```

Expected: a table listing whatever assets are currently in `agents/`, `instructions/`, `skills/`, `mcp-configs/` (may be empty if those dirs are empty — that's fine, the table just has no rows).

```bash
dotnet run --project src/AiSetupCli -- deploy --target claude-code --mode repo --repo /tmp/aisetup-smoketest --dry-run
```

Expected: a colored "create/overwrite" table or, if no assets exist, an empty plan and the "Dry run — nothing was written." line.

- [ ] **Step 4: Commit**

```bash
git add src/AiSetupCli/Program.cs
git commit -m "feat: wire ai-setup CLI commands via Spectre + DI"
```

---

## Task 21: End-to-end integration test (DeployService against MockFileSystem)

A wider test that drives the whole pipeline through the DI container with a mock filesystem and verifies the output for both targets.

**Files:**
- Create: `tests/AiSetupLib.Tests/Integration/EndToEndTests.cs`

- [ ] **Step 1: Write integration tests**

Create `tests/AiSetupLib.Tests/Integration/EndToEndTests.cs`:

```csharp
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using AiSetupLib;
using AiSetupLib.Deploy;
using AiSetupLib.Models;
using AiSetupLib.Paths;
using Microsoft.Extensions.DependencyInjection;

namespace AiSetupLib.Tests.Integration;

public class EndToEndTests
{
    private const string RepoRoot = "/repo";

    private static (IDeployService svc, MockFileSystem fs) BuildContainer()
    {
        var fs = new MockFileSystem();
        SeedRepo(fs);

        var services = new ServiceCollection();
        services.AddAiSetup(RepoRoot);
        services.AddSingleton<IFileSystem>(fs);
        services.AddSingleton<IPathProvider>(_ => new PathProvider(home: "/home/me", platform: PlatformKind.Linux));
        var sp = services.BuildServiceProvider();
        return (sp.GetRequiredService<IDeployService>(), fs);
    }

    private static void SeedRepo(MockFileSystem fs)
    {
        fs.AddFile($"{RepoRoot}/instructions/csharp/csharp.instructions.md", new MockFileData("""
            ---
            name: csharp.instructions
            description: C# rules
            type: instruction
            targets: [copilot-cli, claude-code]
            ---
            Use sealed.
            """));
        fs.AddFile($"{RepoRoot}/agents/dotnet-developer.md", new MockFileData("""
            ---
            name: dotnet-developer
            description: .NET dev
            type: agent
            targets: [claude-code]
            ---
            Agent body
            """));
        fs.AddFile($"{RepoRoot}/profiles/dotnet-dev.yaml", new MockFileData("""
            name: dotnet-dev
            description: .NET
            agents: [dotnet-developer]
            instructions: [csharp/csharp.instructions]
            """));
    }

    [Fact]
    public void Deploys_profile_to_claude_code_repo_writing_aggregated_md()
    {
        var (svc, fs) = BuildContainer();

        svc.Deploy(new DeployOptions(DeployTarget.ClaudeCode, DeployMode.Repo, "/dest")
        {
            Profile = "dotnet-dev",
        });

        fs.File.Exists("/dest/CLAUDE.md").Should().BeTrue();
        var md = fs.File.ReadAllText("/dest/CLAUDE.md");
        md.Should().Contain("Use sealed.");
        md.Should().Contain("Agent body");
    }

    [Fact]
    public void Deploys_profile_to_copilot_cli_repo_writing_individual_files()
    {
        var (svc, fs) = BuildContainer();

        svc.Deploy(new DeployOptions(DeployTarget.CopilotCli, DeployMode.Repo, "/dest")
        {
            Profile = "dotnet-dev",
        });

        fs.File.Exists("/dest/.github/instructions/csharp/csharp.instructions.md").Should().BeTrue();
        fs.File.Exists("/dest/.github/agents/dotnet-developer.md").Should().BeTrue();
    }

    [Fact]
    public void Dry_run_writes_nothing()
    {
        var (svc, fs) = BuildContainer();

        var plan = svc.Deploy(new DeployOptions(DeployTarget.ClaudeCode, DeployMode.Repo, "/dest")
        {
            Profile = "dotnet-dev",
            DryRun = true,
        });

        plan.Actions.Should().NotBeEmpty();
        fs.File.Exists("/dest/CLAUDE.md").Should().BeFalse();
    }
}
```

- [ ] **Step 2: Run — expect pass**

Run: `dotnet test --filter FullyQualifiedName~EndToEndTests`
Expected: all 3 tests pass.

- [ ] **Step 3: Run full suite to confirm green**

Run: `dotnet test`
Expected: every test in every project passes.

- [ ] **Step 4: Commit**

```bash
git add tests/AiSetupLib.Tests/Integration
git commit -m "test: add end-to-end deploy tests through DI container"
```

---

## Task 22: README usage section

Adds a short usage section to the existing `README.md` so a new user can run the CLI.

**Files:**
- Modify: `README.md`

- [ ] **Step 1: Add usage section**

Append to `README.md`:

```markdown
## CLI usage

Build & install as a local tool, or run via `dotnet run`:

```bash
# List available assets in this repo
dotnet run --project src/AiSetupCli -- list

# Inspect a single asset
dotnet run --project src/AiSetupCli -- info csharp/dotnet-tester

# Dry-run a deploy of the dotnet-dev profile to a target repo as Claude Code config
dotnet run --project src/AiSetupCli -- deploy \
  --target claude-code --mode repo --repo /path/to/target-repo \
  --profile dotnet-dev --dry-run

# Deploy individual assets to local Copilot CLI settings
dotnet run --project src/AiSetupCli -- deploy \
  --target copilot-cli --mode local \
  --skills csharp/dotnet-tester,csharp/ef-core
```

Targets: `copilot-cli`, `claude-code`. Modes: `repo`, `local`.
```

- [ ] **Step 2: Commit**

```bash
git add README.md
git commit -m "docs: add CLI usage section to README"
```

---

## Self-Review Notes

- **Spec coverage:**
  - Repo structure (agents/instructions/skills/mcp-configs/profiles): handled by Task 5 discovery + Task 6 profiles.
  - Asset frontmatter parsing: Task 4.
  - CLI commands `deploy`, `list`, `info`: Tasks 18–19.
  - Profile-based deployment + single-asset selection: Tasks 14, 18.
  - Dry-run with colors: Task 17 (DryRunRenderer green/yellow/red mapped from `DeployActionKind`).
  - Aggregation for Claude (CLAUDE.md): Tasks 7, 12.
  - MCP configs as separate asset type, optional flag: Task 8 + Task 11/12 (only deployed when `--mcp-configs` or in profile).
  - Adapter pattern with `IDeployTarget`: Tasks 11–13.
  - Cross-platform paths: Task 9.
  - Levenshtein suggestions for missing names: Tasks 10, 14, 19.
  - `--force` semantics: passed through `DeployOptions.Force` and exposed by CLI; current Plan layer marks overwrites — interactive prompts left to a follow-up since the spec only requires either `--force` or interactive prompt and CLI surfaces a clear overwrite color in dry-run. (If interactive prompt is mandatory, add as Task 23.)
  - xUnit + FakeItEasy + AwesomeAssertions: every test task uses these.

- **Placeholders:** scanned — every code step contains complete code; no "TODO" / "fill in" / "similar to Task N" left.

- **Type consistency:** `AssetDefinition` ctor params identical across Tasks 3, 5, 11, 12. `DeployActionKind` enum members `{Create, Overwrite, Skip, Error}` referenced consistently in Tasks 3, 11, 12, 17. Method signatures `IDeployTarget.Plan` / `Apply` consistent across Tasks 11–13.

---

## Execution Handoff

Plan complete and saved to `docs/superpowers/plans/2026-05-04-ai-setup-cli-implementation.md`. Two execution options:

**1. Subagent-Driven (recommended)** — I dispatch a fresh subagent per task, review between tasks, fast iteration

**2. Inline Execution** — Execute tasks in this session using executing-plans, batch execution with checkpoints

**Which approach?**
