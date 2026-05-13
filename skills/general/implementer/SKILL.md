---
name: implementer
description: Use when implementing a requirement, feature, bugfix, or refactor with a clear goal — "implement requirement X", "build feature Y", "umsetzen". Language-independent orchestration workflow. Skip for pure questions or trivial one-line changes.
---

# implementer

Structured way to take a requirement from "understood" to "implemented, tested, and reviewed". Language-independent — but for language-specific work (tests, reviews, SDK scaffolding, …) check first whether a dedicated skill exists and use it.

**Core principle: delegate to sub-agents wherever possible to keep the main agent's context lean.** The main agent coordinates and holds only summaries.

## When to use

- A feature, bugfix, or refactor with a clear, agreed-upon goal.
- Not for: pure questions, exploration-only tasks, trivial one-line edits.

## Sub-agent policy

Default: dispatch codebase research, implementation tasks, and review to sub-agents (Task/Agent tool — `general-purpose` or a specialized agent). They return concise results; the main agent integrates them. A research sub-agent should use the project's preferred code-exploration tooling.

Do it directly only when: the change is trivial (a small, localized edit you can finish in a couple of steps), or the step needs direct user interaction (Phase 1 questions, Phase 2 approval).

## Language-specific skills

Before doing language-specific work, look for a matching skill. In this repo (.NET):

| Task | Skill |
|------|-------|
| Tests | `dotnet-tester` |
| Code review | `dotnet-reviewer` |
| SDK / client library | `dotnet-sdk-builder` |
| XML documentation | `csharp-docs` |
| EF Core data access | `ef-core` |
| NuGet packages | `nuget-manager` |
| ASP.NET Core | `dotnet-aspnet` |
| Inspecting .NET APIs | `dotnet-inspect` |

When a row applies, invoke that skill via the Skill tool — don't just work from memory. Other languages: look for the skills available there. If none, follow that language's best practices.

## Workflow

### Phase 1 — Understand & clarify the requirement
- Read the requirement. Research existing code, patterns, and reusable utilities **via a sub-agent**; get back a short summary.
- Collect open questions, assumptions, and edge cases, then **clarify them with the user before continuing**. Do not guess.
- Output: an unambiguous understanding of the requirement.

### Phase 2 — Create a plan
- Write the implementation plan as `docs/plans/YYYY-MM-DD-short-description.md` (create the directory if missing; never overwrite an existing file — append `-2`, `-3`, … if needed).
- Include: context/rationale, affected files, existing functions/utilities to reuse (from the Phase 1 research), steps, test/verification strategy, any language-specific skills to use.
- **Get the user's approval before implementing.** Update the plan on change requests.
- Output: an approved plan file.

### Phase 3 — Implement production code with tests
- Implement per the plan; split into independent tasks and **delegate them to sub-agents**; the main agent integrates the results.
- Tests are part of the implementation (critical paths, edge cases). Use the language's test skill if one exists (e.g. `dotnet-tester`).
- Follow the project's existing conventions and code style; reuse existing utilities instead of writing new code.
- Get the build and tests green; never fake-pass tests.
- If implementation surfaces a new ambiguity, return to the user (Phase 1) or revise the plan (Phase 2) before continuing — don't guess your way past it.
- Output: implemented code + tests, build & tests green.

### Phase 4 — Review the changes (sub-agent)
- Start a **separate sub-agent** that reviews only the changes (the diff) against the requirement, the plan, and code quality. If a language-specific review skill exists (e.g. `dotnet-reviewer`), have the sub-agent use it.
- The sub-agent returns prioritized findings; the main agent addresses the relevant ones (again via sub-agent if substantial) until clean.
- Output: a cleaned-up state; remaining items documented.

### Phase 5 — Short summary
- Brief summary in chat: what changed (files), how it was tested, review outcome, any open items / follow-ups.
- Do not commit or push unless the user asks.

## Common mistakes

- Implementing before clarifying open questions.
- Skipping plan approval.
- Reviewing in the main context instead of a sub-agent — the context fills up.
- Ignoring an available language-specific skill.
- Writing new code when a utility already exists.
- Leaving out tests.

## Quick reference

| Phase | Goal | Actor | Output |
|-------|------|-------|--------|
| 1 | Understand & clarify | Main + research sub-agent + user | Clear requirement |
| 2 | Plan | Main + user approval | Approved `docs/plans/…` file |
| 3 | Implement + tests | Sub-agents, main integrates | Code + tests, build green |
| 4 | Review changes | Review sub-agent, main fixes | Clean state |
| 5 | Summarize | Main | Summary for the user |
