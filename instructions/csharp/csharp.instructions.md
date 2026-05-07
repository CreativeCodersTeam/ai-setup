---
name: csharp
description: "C# coding guidelines (formatting, async, nullability, guards)."
tags: [csharp]
targets: [copilot-cli, claude-code]
type: instruction
applyTo: "**/*.cs"
---
# C# Guidelines

- Use the latest stable C# version available in the target framework.
- Prefer file-scoped namespace declarations and single-line `using` directives.
- Insert a blank line before opening curly braces of code blocks.
- In library code use `.ConfigureAwait(false)` on awaits; not in tests.
- Use `is null` / `is not null` instead of equality operators against `null`.
- Use `nameof` instead of string literals when referring to member names.
- Guard required arguments of public methods with `Ensure.NotNull(...)` /
  `Ensure.IsNotNullOrWhitespace(...)` from `CreativeCoders.Core`.
