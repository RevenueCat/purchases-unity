#!/usr/bin/env bash
# Checks that every Unity asset file in the SDK folders has a corresponding .meta file.
# Missing .meta files cause compilation failures when installing via UPM.
# Related: https://github.com/RevenueCat/purchases-unity/pull/875

set -e

MISSING=0

while IFS= read -r -d '' f; do
    if [ ! -f "${f}.meta" ]; then
        echo "❌ Missing .meta file for: $f"
        MISSING=1
    fi
done < <(find RevenueCat RevenueCatUI \( -name "*.cs" -o -name "*.asmdef" -o -name "*.shader" -o -name "*.json" \) -not -path "*/build/*" -print0)

if [ $MISSING -eq 1 ]; then
    echo ""
    echo "Unity requires a .meta file for every asset. Add the missing .meta files and commit them."
    exit 1
fi

echo "✅ All asset files have corresponding .meta files."
