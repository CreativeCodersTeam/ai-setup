#!/usr/bin/env bash
# Source from unit tests. Provides: assert_eq, assert_ne, assert_exit, assert_json_eq, fail.

set -u

_ASSERT_PASSED=0
_ASSERT_FAILED=0
_FAILED_NAMES=()

_red()   { printf '\033[31m%s\033[0m' "$1"; }
_green() { printf '\033[32m%s\033[0m' "$1"; }

assert_eq() {
  local actual=$1 expected=$2 name=$3
  if [[ "$actual" == "$expected" ]]; then
    _ASSERT_PASSED=$((_ASSERT_PASSED+1))
    printf '  %s %s\n' "$(_green PASS)" "$name"
  else
    _ASSERT_FAILED=$((_ASSERT_FAILED+1))
    _FAILED_NAMES+=("$name")
    printf '  %s %s\n    expected: %q\n    actual:   %q\n' \
      "$(_red FAIL)" "$name" "$expected" "$actual"
  fi
}

assert_exit() {
  local actual=$1 expected=$2 name=$3
  assert_eq "$actual" "$expected" "$name (exit code)"
}

assert_json_eq() {
  local json=$1 jq_path=$2 expected=$3 name=$4
  local actual
  actual=$(printf '%s' "$json" | jq -r "$jq_path" 2>/dev/null || echo "<jq-error>")
  assert_eq "$actual" "$expected" "$name ($jq_path)"
}

fail() {
  _ASSERT_FAILED=$((_ASSERT_FAILED+1))
  _FAILED_NAMES+=("$1")
  printf '  %s %s\n' "$(_red FAIL)" "$1"
}

summary() {
  printf '\n%d passed, %d failed\n' "$_ASSERT_PASSED" "$_ASSERT_FAILED"
  if (( _ASSERT_FAILED > 0 )); then
    printf 'Failed:\n'
    for n in "${_FAILED_NAMES[@]}"; do printf '  - %s\n' "$n"; done
    return 1
  fi
}
