#!/usr/bin/env bash
set -u
TEST_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SKILL_DIR="${SKILL_DIR:-$(cd "$TEST_DIR/../.." && pwd)}"
FIX="$SKILL_DIR/tests/fixtures"
SCRIPT="$SKILL_DIR/scripts/collect-diff.sh"
source "$SKILL_DIR/tests/helpers.sh"

# 1. uncommitted mode on net10 — picks up New.cs, excludes *.min.js and wwwroot/lib
out=$(bash "$SCRIPT" --repo-root "$FIX/repo-net10" --mode uncommitted 2>/dev/null); rc=$?
assert_exit "$rc" 0 "uncommitted exit code"
assert_json_eq "$out" '.files' "1" "uncommitted file count"
file_list=$(printf '%s' "$out" | jq -r '.file_list | join(",")')
[[ "$file_list" == "src/New.cs" ]] && \
  assert_eq ok ok "uncommitted file_list is src/New.cs" || \
  fail "uncommitted file_list is src/New.cs (got: $file_list)"

# 2. branch mode on empty-diff — no changes vs main
out=$(bash "$SCRIPT" --repo-root "$FIX/repo-empty-diff" --mode branch 2>/dev/null); rc=$?
assert_exit "$rc" 0 "empty-diff branch exit code"
assert_json_eq "$out" '.files' "0" "empty-diff file count"
assert_json_eq "$out" '.diff' "" "empty-diff payload empty"

# 3. not a git repo
out=$(bash "$SCRIPT" --repo-root "$FIX/repo-no-git" --mode uncommitted 2>/dev/null); rc=$?
assert_exit "$rc" 2 "no-git exit code"

# 4. branch mode but main missing — simulate by deleting branch
T=$(mktemp -d); cp -R "$FIX/repo-empty-diff"/. "$T"/
( cd "$T" && git branch -m main feature && git checkout -q -b feature2 )
out=$(bash "$SCRIPT" --repo-root "$T" --mode branch 2>/dev/null); rc=$?
assert_exit "$rc" 3 "missing-main exit code"
rm -rf "$T"

# 5. large-diff repo — counts >2000 LOC, >50 files
out=$(bash "$SCRIPT" --repo-root "$FIX/repo-large-diff" --mode uncommitted 2>/dev/null); rc=$?
assert_exit "$rc" 0 "large-diff exit code"
loc=$(printf '%s' "$out" | jq '.loc')
files=$(printf '%s' "$out" | jq '.files')
[[ "$loc" -gt 2000 ]] && assert_eq ok ok "large-diff loc>2000 (got $loc)" || fail "large-diff loc>2000 (got $loc)"
[[ "$files" -gt 50 ]] && assert_eq ok ok "large-diff files>50 (got $files)" || fail "large-diff files>50 (got $files)"

# 6. --help
out=$(bash "$SCRIPT" --help 2>&1); rc=$?
assert_exit "$rc" 0 "--help exit"

summary
