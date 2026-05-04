# ai-setup

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
