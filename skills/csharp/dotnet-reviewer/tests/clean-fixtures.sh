#!/usr/bin/env bash
# Remove all generated fixture repos. They will be regenerated on the
# next `bash tests/run-tests.sh` invocation via make-fixtures.sh.
set -euo pipefail
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
rm -rf "$SCRIPT_DIR"/fixtures/repo-*/
echo "Removed fixture repos under $SCRIPT_DIR/fixtures/"
