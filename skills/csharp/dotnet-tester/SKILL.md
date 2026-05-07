---
name: dotnet-tester
description: "Write and execute .NET unit tests using xUnit, FakeItEasy and AwesomeAssertions."
tags: [csharp, testing]
targets: [copilot-cli, claude-code]
type: skill
---
# dotnet-tester

Use this skill to add or improve unit tests for C# / .NET code.

## Conventions

- Test framework: xUnit.
- Mocks: FakeItEasy (do not introduce other mocking libraries).
- Assertions: AwesomeAssertions (the `FluentAssertions` namespace).
- Arrange-Act-Assert structure with one logical assertion per test.
- Test classes mirror the SUT namespace + `Tests` suffix.

## Workflow

1. Identify the SUT and read its public contract.
2. Cover the happy path, edge cases and failure modes.
3. Prefer real collaborators when cheap; otherwise fake interfaces.
4. Run `dotnet test` and ensure all tests pass before reporting completion.
