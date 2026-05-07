---
name: csharp
description: "C# coding conventions"
tags: [csharp, dotnet]
targets: [copilot-cli, claude-code]
applyTo: "**/*.cs"
---

# C# Conventions

- Always use the latest stable C# version available in the project's target framework.
- Prefer file-scoped namespaces and single-line using directives.
- Use `nameof` instead of string literals when referring to member names.
- Document all public members with XML documentation.
- In **library code** always use `.ConfigureAwait(false)`.
