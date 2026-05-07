#!/usr/bin/env bash
# Runs all unit tests. Builds fixtures first if missing.

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SKILL_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"

if ! command -v jq >/dev/null 2>&1; then
  echo "ERROR: jq is required for tests" >&2
  exit 1
fi

if [[ ! -d "$SCRIPT_DIR/fixtures/repo-net10/.git" ]]; then
  echo "Building fixtures..."
  bash "$SCRIPT_DIR/fixtures/make-fixtures.sh"
fi

EXIT=0
for t in "$SCRIPT_DIR/unit"/test-*.sh; do
  [[ -f "$t" ]] || continue
  echo
  echo "=== $(basename "$t") ==="
  if ! SKILL_DIR="$SKILL_DIR" bash "$t"; then
    EXIT=1
  fi
done

exit "$EXIT"
