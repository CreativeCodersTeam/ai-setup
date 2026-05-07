# Tests

## Layout

- `helpers.sh` — assertion helpers sourced by every unit test.
- `run-tests.sh` — entry point. Builds fixtures if missing, runs every `unit/test-*.sh`.
- `fixtures/make-fixtures.sh` — generates the six fixture repos. Idempotent.
- `clean-fixtures.sh` — removes generated `repo-*/` directories. Auto-rebuilt on next `run-tests.sh`.
- `unit/test-*.sh` — one file per script under test.
- `unit/mock-dotnet/dotnet` — mock `dotnet` binary, behavior controlled via `MOCK_DOTNET_MODE`.
- `integration/test-skill-flow.md` — manual smoke checklist; not run by `run-tests.sh`.

## Running

```bash
# all unit tests (rebuilds fixtures on first run)
bash tests/run-tests.sh

# one test file
bash tests/unit/test-detect-version.sh

# rebuild fixtures from scratch
rm -rf tests/fixtures/repo-*
bash tests/fixtures/make-fixtures.sh

# clean fixtures (removes repo-*/ directories; auto-rebuilt on next run-tests)
bash tests/clean-fixtures.sh
```

## Dependencies

- `bash` 3.2+ (macOS default works)
- `git`
- `jq`
- `python3` (used by `collect-diff.sh` and `run-checks.sh` for JSON escaping)

`dotnet` SDK is **not** required to run unit tests — `run-checks.sh` is exercised against the mock binary.

## Conventions

- Each test calls `summary` last; the function exits non-zero if any assertion failed.
- Tests do not modify the fixture repos. If a test needs to mutate a repo, it copies it to `mktemp` first.
- New scripts → new `unit/test-<name>.sh` + new fixtures only if existing ones don't fit.
