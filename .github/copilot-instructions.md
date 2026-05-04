# SmartHal – Copilot Instructions

## Repository Purpose

This is a **planning and design repository only** — no source code lives here. The repo is used to develop plans, specifications, feature definitions, and architectural decisions for the SmartHal platform iteratively.

## What is SmartHal?

SmartHal is an IoT platform focused on:

- **Device management** across heterogeneous device types and manufacturers (HomeMatic, ZigBee, KNX, and others)
- **Backup & restore** of device configurations
- **YAML-based configuration system** for defining devices and their relationships — enabling state restoration when replacing devices, including propagating relationship changes to related devices
- **CLI-first** interface, with a parallel Web UI solution planned

## Documentation Structure (`docs/`)

| File | Content |
|---|---|
| `docs/vision.md` | Vision, Leitplanken, Nicht-Ziele, Zielgruppen, Tech-Entscheidungen |
| `docs/anforderungen.md` | Funktionale und nicht-funktionale Anforderungen (IDs F-xx / NF-xx) |
| `docs/architektur.md` | Library-First-Architektur, Schichtenmodell, ASCII-Diagramme |
| `docs/yaml-schema.md` | YAML-Konfigurationsformat Schema v1.0 (deklarativ, ID-Konzept) |
| `docs/secrets-management.md` | `ISecretsProvider`-Interface, alle Provider (Windows/macOS/Linux/File/Env), CLI-Befehle |
| `docs/roadmap.md` | Phasen-Planung mit Meilensteinen |

## Technology Decisions

- **CLI & Backend**: .NET / C#
- **REST API**: ASP.NET Core (Phase 2)
- **Web Frontend**: Angular (Phase 2)
- **Config format**: YAML (declarative, versioned)
- **Local persistence**: Filesystem only (YAML files) in Phase 1; optional SQLite in Phase 2 for REST API backends

## Key Architecture Concepts

- **Library-First**: All business logic in reusable .NET libraries; CLI and REST API are thin shells
- **Adapter pattern**: Each protocol (HomeMatic, EVCC, MQTT, Matter) is an independent module implementing `ISmartHalAdapter`
- **Stable IDs**: SmartHal assigns its own device IDs (`smhal-*`) separate from adapter/protocol IDs — this enables device replacement without breaking relations
- **Secrets via ISecretsProvider**: Credentials are never stored in YAML files; adapter configs hold only key references (`_key` fields); the active provider (Windows Credential Manager, macOS Keychain, Encrypted File, Env Vars, etc.) is configured in `meta.yaml`

## Working in This Repo

- All work here is planning, design, and decision-making — not implementation
- Use an **iterative approach** to develop the platform concept step by step
- Documents should be written in **German** (consistent with existing content)
- Requirements use IDs in format `F-XX.Y` (functional) and `NF-XX.Y` (non-functional)
