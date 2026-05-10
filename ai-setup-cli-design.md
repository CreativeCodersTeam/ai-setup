# AI-Setup CLI — Design Spec

## Überblick

Ein CLI-Tool zur zentralen Verwaltung und zum Deployment von AI-Konfigurationen (Agents, Skills, Instructions, MCP Server Configs) für verschiedene AI-Systeme. Das Tool liest Definitionen aus diesem Repo und deployt sie zielgerichtet — entweder in ein Repository oder in lokale Settings-Ordner des jeweiligen AI-Systems.

## Zielsysteme

- **GitHub Copilot CLI** — Unterstützt Einzeldateien, Ordnerstrukturen
- **Claude Code** — Erwartet aggregierte Dateien (CLAUDE.md), JSON-Settings

## Anforderungen

### Funktional

- Zentrale Verwaltung aller AI-Definitionen in einem Repo
- Gruppierung nach Sprache/Technologie und Themenbereich (verschachtelte Ordnerstruktur)
- Deploy per CLI in Ziel-Repos oder lokale Settings
- Deployment ausschließlich profil-basiert (vordefinierte Asset-Bündel) — keine Einzelauswahl per CLI-Flags
- Intelligente Aggregation je nach Zielsystem
- MCP Server Configs als eigener Asset-Typ (über das Profil ausgewählt)
- Dry-Run Modus
- Cross-Platform: Windows, macOS, Linux

### Nicht-Funktional

- .NET / C# mit Spectre.Console.Cli
- Library-First-Architektur (AiSetupLib + AiSetupCli)
- Adapter-Pattern für Zielsysteme
- Testbar mit xUnit + FakeItEasy + AwesomeAssertions

---

## Repo-Struktur

```
ai-setup/
├── agents/                     # Agent-Definitionen
│   └── dotnet-developer.md
├── instructions/               # Instruction-Dateien (nach Sprache/Thema)
│   ├── csharp/
│   │   └── csharp.instructions.md
│   ├── python/
│   │   └── python.instructions.md
│   └── general/
│       └── code-review.instructions.md
├── skills/                     # Skill-Definitionen (verschachtelt)
│   ├── csharp/
│   │   ├── dotnet-tester/
│   │   ├── dotnet-sdk-builder/
│   │   ├── ef-core/
│   │   ├── nuget-manager/
│   │   └── csharp-docs/
│   └── general/
│       └── create-readme/
├── mcp-configs/                # MCP Server Konfigurationen
│   ├── github.yaml
│   ├── filesystem.yaml
│   └── ...
├── profiles/                   # Vordefinierte Profile
│   ├── dotnet-dev.yaml
│   ├── python-dev.yaml
│   └── fullstack.yaml
├── src/
│   ├── AiSetupCli/            # CLI (Spectre.Console.Cli)
│   └── AiSetupLib/            # Business-Logik + Adapter
└── .github/skills/            # Superpowers (Meta-Skills für dieses Repo)
```

---

## Asset-Typen & Metadaten

Jede Definition hat YAML-Frontmatter:

```yaml
---
name: dotnet-tester
description: "Write and execute .NET unit tests"
tags: [csharp, testing]
targets: [copilot-cli, claude-code]
type: skill
applyTo: "**/*.cs"
---
```

**Asset-Typen:**

| Typ | Beschreibung |
|-----|-------------|
| `instruction` | Coding-Guidelines, werden in Instructions-Dateien des Ziels geschrieben |
| `skill` | Komplexere Workflows mit Ordnerstruktur (SKILL.md + references/) |
| `agent` | Agent-Definitionen mit Skill-Referenzen |
| `mcp-config` | MCP Server Konfiguration (Verbindungsdaten, Endpoints) |

**Optionale `manifest.yaml` pro Ordner:**

```yaml
group: csharp
description: "C# und .NET Entwicklung"
default-targets: [copilot-cli, claude-code]
```

---

## CLI Commands

```
ai-setup deploy --target <copilot-cli|claude-code>
                --mode <repo|local>
                --profile <name>           # Pflicht: das zu verteilende Profil
                --repo <path>              # Bei mode=repo: Ziel-Repository
                [--dry-run]                # Zeigt was passieren würde
                [--force]                  # Überschreiben ohne Nachfrage
                [--mcp-on-conflict <fail|overwrite|skip>]

ai-setup list [agents|skills|instructions|mcp-configs|profiles]
              [--tag <tag>]
              [--target <system>]

ai-setup info <asset-name>                # Details zu einem Asset
```

---

## Deploy-Verhalten je nach Zielsystem

| Aspekt | Copilot CLI (repo) | Copilot CLI (local) | Claude Code (repo) | Claude Code (local) |
|--------|-------------------|--------------------|--------------------|---------------------|
| Instructions | `.github/instructions/*.md` (Einzeldateien) | `~/.config/github-copilot/instructions/` | Aggregiert in `CLAUDE.md` | `~/.claude/CLAUDE.md` |
| Skills | `.github/skills/<name>/` (Ordner kopieren) | `~/.config/github-copilot/skills/` | In CLAUDE.md eingebettet oder `.claude/commands/` | `~/.claude/commands/` |
| Agents | `.github/agents/*.md` | `~/.config/github-copilot/agents/` | Als Teil von CLAUDE.md | `~/.claude/CLAUDE.md` |
| MCP Configs | `.vscode/mcp.json` | VS Code User Settings | `.claude/settings.json` | `~/.claude/settings.json` |

---

## Architektur

```
AiSetupLib/
├── Models/
│   ├── AssetDefinition.cs         # Basis: Name, Description, Tags, Targets, Type
│   ├── AssetType.cs               # Enum: Instruction, Skill, Agent, McpConfig
│   ├── Profile.cs                 # Profil mit Asset-Referenzen
│   ├── DeployTarget.cs            # Enum: CopilotCli, ClaudeCode
│   ├── DeployMode.cs              # Enum: Repo, Local
│   └── DeployOptions.cs           # Ziel, Modus, Profilname, Flags
├── Discovery/
│   ├── IAssetDiscovery.cs         # Interface: Assets im Repo finden
│   ├── AssetDiscoveryService.cs   # Scannt Ordner, liest Frontmatter
│   └── FrontmatterParser.cs       # YAML-Frontmatter parsen
├── Targets/
│   ├── IDeployTarget.cs           # Interface für Zielsysteme
│   ├── CopilotCliTarget.cs        # Copilot CLI: Einzeldateien, Ordner kopieren
│   ├── ClaudeCodeTarget.cs        # Claude: Aggregation, settings.json
│   └── TargetRegistry.cs          # Registriert verfügbare Targets
├── Aggregation/
│   ├── IContentAggregator.cs      # Interface: Inhalte zusammenführen
│   ├── MarkdownAggregator.cs      # Mehrere MD-Dateien → eine Datei
│   └── McpConfigMerger.cs         # MCP-Configs zusammenführen
├── Deploy/
│   ├── IDeployService.cs          # Orchestriert den Deploy-Vorgang
│   └── DeployService.cs           # Resolve Profile → Discover → Deploy
└── Profiles/
    ├── IProfileResolver.cs        # Profile laden und auflösen
    └── ProfileResolver.cs         # YAML-Profile parsen
```

### Ablauf

1. CLI parst Argumente → `DeployOptions` (mit Pflicht-Profilname)
2. `ProfileResolver` lädt das Profil und löst es in eine Asset-Liste auf
3. `AssetDiscoveryService` findet die gewünschten Assets im Repo
4. `DeployService` übergibt Assets an den richtigen `IDeployTarget`
5. Target schreibt/kopiert/aggregiert je nach Modus (repo/local)

---

## Profil-Format

```yaml
# profiles/dotnet-dev.yaml
name: dotnet-dev
description: ".NET/C# Entwicklung - alle relevanten Assets"
agents:
  - dotnet-developer
instructions:
  - csharp/csharp.instructions
skills:
  - csharp/dotnet-tester
  - csharp/dotnet-sdk-builder
  - csharp/ef-core
  - csharp/nuget-manager
  - csharp/csharp-docs
mcp-configs:
  - github
```

---

## Cross-Platform

Das CLI nutzt `Environment.SpecialFolder` und `RuntimeInformation.IsOSPlatform()` für plattformspezifische Pfade:

| Pfad | Windows | macOS | Linux |
|------|---------|-------|-------|
| Copilot CLI (local) | `%APPDATA%\GitHub Copilot CLI\` | `~/Library/Application Support/github-copilot/` | `~/.config/github-copilot/` |
| Claude Code (local) | `%USERPROFILE%\.claude\` | `~/.claude/` | `~/.claude/` |

**Distribution:** .NET Global Tool (`dotnet tool install`) oder Self-Contained Executable.

> **Hinweis:** Die exakten lokalen Pfade für GitHub Copilot CLI werden bei der Implementierung gegen die aktuelle Copilot CLI Dokumentation verifiziert. Die oben genannten Pfade basieren auf dem aktuellen Stand (Mai 2026) und werden als Konfiguration im Code hinterlegt, sodass sie bei Änderungen leicht anpassbar sind.

---

## Error Handling

- **Fehlende Assets** → Klare Fehlermeldung mit Vorschlägen (ähnliche Namen via Levenshtein-Distanz)
- **Ungültiges Frontmatter** → Warnung + Asset überspringen, Deployment nicht abbrechen
- **Ziel existiert bereits** → `--force` zum Überschreiben, sonst interaktive Nachfrage
- **Dry-Run** → Zeigt alle geplanten Aktionen farbig an (grün=neu, gelb=überschreiben, rot=fehler)

---

## Testing

- **Unit Tests**: `FrontmatterParser`, `ProfileResolver`, `MarkdownAggregator`, `McpConfigMerger`
- **Integration Tests**: `DeployService` mit temporärem Filesystem
- **Target Tests**: Jeder `IDeployTarget` mit erwartetem Output-Layout
- **Framework**: xUnit + FakeItEasy + AwesomeAssertions

---

## Erweiterbarkeit

- Neues Zielsystem → Neuer `IDeployTarget`-Adapter + DI-Registration
- Neuer Asset-Typ → `AssetType`-Enum erweitern + Handling in Targets
- Neues Profil → YAML-Datei in `profiles/` ablegen
