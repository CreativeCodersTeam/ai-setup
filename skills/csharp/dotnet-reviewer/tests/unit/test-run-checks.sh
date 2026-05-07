#!/usr/bin/env bash
set -u
TEST_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SKILL_DIR="${SKILL_DIR:-$(cd "$TEST_DIR/../.." && pwd)}"
FIX="$SKILL_DIR/tests/fixtures"
SCRIPT="$SKILL_DIR/scripts/run-checks.sh"
MOCK="$SKILL_DIR/tests/unit/mock-dotnet/dotnet"
source "$SKILL_DIR/tests/helpers.sh"

run_with_mode() {
  local mode=$1; shift
  MOCK_DOTNET_MODE="$mode" DOTNET_BIN="bash $MOCK" \
    bash "$SCRIPT" --repo-root "$FIX/repo-net10" "$@"
}

# build only — happy
out=$(run_with_mode build-ok --build); rc=$?
assert_exit "$rc" 0 "build-ok exit"
assert_json_eq "$out" '.build.ok' "true" "build-ok flag"
assert_json_eq "$out" '.format' "null" "format omitted when not requested"

# build only — fail
out=$(run_with_mode build-fail --build); rc=$?
assert_exit "$rc" 0 "build-fail wrapper exit (still 0)"
assert_json_eq "$out" '.build.ok' "false" "build-fail flag"
errs=$(printf '%s' "$out" | jq '.build.errors | length')
[[ "$errs" -ge 1 ]] && assert_eq ok ok "build-fail produces error" || fail "build-fail produces error"

# format dirty
out=$(run_with_mode format-dirty --format); rc=$?
assert_exit "$rc" 0 "format-dirty wrapper exit"
assert_json_eq "$out" '.format.ok' "false" "format-dirty flag"

# test fail
out=$(run_with_mode test-fail --test); rc=$?
assert_exit "$rc" 0 "test-fail wrapper exit"
assert_json_eq "$out" '.test.ok' "false" "test-fail flag"

# combined: build + format + test, all good
out=$(MOCK_DOTNET_MODE=build-ok DOTNET_BIN="bash $MOCK" \
      bash "$SCRIPT" --repo-root "$FIX/repo-net10" --build --format --test 2>/dev/null) || true
# can't have multi-mode mock in one run; test just asserts JSON has all three keys present
assert_json_eq "$out" '.build.ok' "true" "combined build present"
assert_json_eq "$out" '(.format != null)' "true" "combined format present"
assert_json_eq "$out" '(.test != null)' "true" "combined test present"

# --help
out=$(bash "$SCRIPT" --help 2>&1); rc=$?
assert_exit "$rc" 0 "--help exit"

summary
