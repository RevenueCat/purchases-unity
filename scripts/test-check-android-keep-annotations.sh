#!/usr/bin/env bash
# Fixture suite for check-android-keep-annotations.sh.
#
# Runs the checker against the committed fixtures under scripts/fixtures/keep-check/ and asserts
# its behavior, so the scanner's edge-case handling can't silently regress. Add new cases by
# dropping files in fixtures/keep-check/{pass,fail}/ and, for fail cases, listing the expected
# flagged type name(s) in EXPECTED_FLAGGED below.

set -uo pipefail

cd "$(dirname "$0")/.."

CHECKER="scripts/check-android-keep-annotations.sh"
FIX="scripts/fixtures/keep-check"

# Type names that the checker must flag when scanning fixtures/keep-check/fail/ (sorted, unique).
EXPECTED_FLAGGED=$(printf '%s\n' \
    AfterInlineBlock \
    CtorVisibility \
    JavaPublicMissingKeep \
    NestedNoKeep \
    PublicData \
    PublicEnum \
    PublicObj \
    | sort -u)

FAILED=0

pass() { printf 'PASS: %s\n' "$1"; }
fail() { printf 'FAIL: %s\n' "$1"; FAILED=1; }

# Extract the flagged type names from checker output, sorted and unique.
flagged_names() { printf '%s\n' "$1" | sed -n "s/.*public type '\([^']*\)'.*/\1/p" | sort -u; }

# --- pass/ : every public type annotated or exempt -> zero findings, exit 0 ---
out=$(bash "$CHECKER" "$FIX/pass" 2>&1); rc=$?
if [ "$rc" -eq 0 ]; then
    pass "pass/ produces no findings"
else
    fail "pass/ should produce no findings (exit $rc)"
    printf '%s\n' "$out"
fi

# --- fail/ : public types missing @Keep -> exit 1 and exactly the expected flagged set ---
out=$(bash "$CHECKER" "$FIX/fail" 2>&1); rc=$?
got=$(flagged_names "$out")
if [ "$rc" -ne 0 ]; then
    pass "fail/ exits non-zero"
else
    fail "fail/ should exit non-zero"
fi
if [ "$got" = "$EXPECTED_FLAGGED" ]; then
    pass "fail/ flags exactly the expected types"
else
    fail "fail/ flagged set mismatch"
    printf -- '--- expected ---\n%s\n--- actual ---\n%s\n' "$EXPECTED_FLAGGED" "$got"
fi

# --- empty/ : no scannable files -> exit 1 with the no-files guard message ---
out=$(bash "$CHECKER" "$FIX/empty" 2>&1); rc=$?
if [ "$rc" -ne 0 ] && printf '%s' "$out" | grep -q "no .java/.kt files"; then
    pass "empty/ triggers the no-files guard"
else
    fail "empty/ should trigger the no-files guard (exit $rc)"
    printf '%s\n' "$out"
fi

# --- missing root -> exit 1 with the does-not-exist guard message ---
out=$(bash "$CHECKER" "$FIX/does-not-exist" 2>&1); rc=$?
if [ "$rc" -ne 0 ] && printf '%s' "$out" | grep -q "does not exist"; then
    pass "missing root triggers the does-not-exist guard"
else
    fail "missing root should trigger the does-not-exist guard (exit $rc)"
    printf '%s\n' "$out"
fi

echo ""
if [ "$FAILED" -ne 0 ]; then
    echo "❌ check-android-keep-annotations.sh fixture suite failed."
    exit 1
fi
echo "✅ check-android-keep-annotations.sh fixture suite passed."
