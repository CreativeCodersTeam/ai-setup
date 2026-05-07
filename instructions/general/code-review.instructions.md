---
name: code-review
description: "General code review checklist"
tags: [review, quality]
targets: [copilot-cli, claude-code]
---

# Code Review Checklist

- Read the description: does the change match the intent?
- Are public APIs documented?
- Are tests present for new behaviour and edge cases?
- Are error paths tested or at least covered by integration tests?
- Are security-sensitive boundaries (input validation, auth) handled?
