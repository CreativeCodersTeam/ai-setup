# ai-setup

Centralised management and deployment of AI assets — agents, skills, instructions
and MCP server configurations — for **GitHub Copilot CLI** and **Anthropic Claude Code**.

The repository is both:

- the **asset library** (under `agents/`, `instructions/`, `skills/`, `mcp-configs/`,
  `profiles/`), and
- a **.NET 10 CLI tool** (`ai-setup`) that deploys those assets — always selected through a
  profile (`--profile`). Each asset in the profile is deployed either into a target
  repository or into the user's local config folder, depending on the `@repo` / `@local`
  suffix on the profile entry (bare entries default to `repo`).

## Repository layout

```
ai-setup/
├── agents/              # Agent definitions (Markdown + YAML frontmatter)
├── instructions/        # Coding guidelines, grouped by language/topic
├── skills/              # Folder-structured workflows (SKILL.md + references/)
├── mcp-configs/         # MCP server configurations (one YAML per server)
├── profiles/            # Predefined bundles of assets
├── src/
│   ├── AiSetupLib/      # Discovery, profiles, aggregation, targets, deploy service
│   └── AiSetupCli/      # Spectre.Console.Cli front-end
└── tests/
    ├── AiSetupLib.Tests/
    └── AiSetupCli.Tests/
```

## Build & test

```sh
dotnet build ai-setup.sln -c Release
dotnet test  ai-setup.sln -c Release
```

## Use the CLI

From this repository root:

```sh
# Show available profiles
dotnet run --project src/AiSetupCli -- list profiles

# Show available instructions
dotnet run --project src/AiSetupCli -- list instructions

# Inspect one asset
dotnet run --project src/AiSetupCli -- info csharp/csharp.instructions

# Dry-run deploy of the dotnet-dev profile (--repo is required when the
# profile contains a 'repo'-mode asset)
dotnet run --project src/AiSetupCli -- deploy \
    --target claude-code \
    --repo /path/to/your/repo --profile dotnet-dev --dry-run

# Real deploy (overwrites existing files with --force)
dotnet run --project src/AiSetupCli -- deploy \
    --target claude-code \
    --repo /path/to/your/repo --profile dotnet-dev --force
```

After packing, the same commands work via the `dotnet tool`:

```sh
dotnet pack src/AiSetupCli -c Release -o ./artifacts
dotnet tool install --global --add-source ./artifacts AiSetupCli
ai-setup deploy --target claude-code --repo /path/to/your/repo --profile dotnet-dev --force
```

## Authoring assets

Each Markdown asset starts with a YAML frontmatter block:

```yaml
---
name: dotnet-tester
description: "Write and execute .NET unit tests"
tags: [csharp, testing]
targets: [copilot-cli, claude-code]   # empty list = all targets
applyTo: "**/*.cs"                    # optional, instruction-style glob
---
```

| Asset type   | Location               | Identifier example                |
|--------------|------------------------|-----------------------------------|
| Instruction  | `instructions/<group>/<name>.instructions.md` | `csharp/csharp.instructions` |
| Agent        | `agents/<name>.md`     | `dotnet-developer`                 |
| Skill        | `skills/<group>/<name>/SKILL.md` | `csharp/dotnet-tester` |
| MCP config   | `mcp-configs/<name>.yaml`        | `github`              |

Profiles bundle multiple IDs. Each entry may carry an optional `@repo` / `@local` suffix
that selects where that asset is deployed; a bare entry defaults to `@repo`. A single
profile may mix modes — aggregated outputs (`CLAUDE.md`, `settings.json`, `mcp.json`) are
then produced once per mode-group (e.g. a repo `CLAUDE.md` and a separate local one).

```yaml
# profiles/dotnet-dev.yaml
name: dotnet-dev
description: ".NET / C# development bundle"
agents:
  - dotnet-developer            # -> repo (default)
instructions:
  - general/general.instructions@local
  - csharp/csharp.instructions  # -> repo
skills:
  - csharp/dotnet-tester
mcp-configs:
  - github
```

`--repo <PATH>` is required only when the resolved profile contains at least one
`repo`-mode asset.

## Deploy behaviour per target

| Aspect       | Copilot CLI (repo)                | Copilot CLI (local)                            | Claude Code (repo)                | Claude Code (local)                          |
|--------------|-----------------------------------|------------------------------------------------|-----------------------------------|----------------------------------------------|
| Instructions | `.github/instructions/<id>.md`    | `<localRoot>/instructions/<id>.md`             | aggregated `CLAUDE.md`            | `~/.claude/CLAUDE.md` (with `.bak`)          |
| Agents       | `.github/agents/<id>.md`          | `<localRoot>/agents/<id>.md`                   | `.claude/agents/<id>.md`          | `~/.claude/agents/<id>.md`                   |
| Skills       | `.github/skills/<id>/`            | `<localRoot>/skills/<id>/`                     | `.claude/skills/<id>/`            | `~/.claude/skills/<id>/`                     |
| MCP servers  | `.vscode/mcp.json` (`servers`)    | `<localRoot>/mcp.json`                         | `.claude/settings.json` (`mcpServers`) | `~/.claude/settings.json`               |

`<localRoot>` follows OS conventions (e.g. `~/Library/Application Support/github-copilot/`
on macOS, `~/.config/github-copilot/` on Linux, `%APPDATA%\GitHub Copilot CLI\` on Windows).

## License

See [LICENSE](./LICENSE).
