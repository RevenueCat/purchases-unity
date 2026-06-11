#!/usr/bin/env bash
# Checks that every public Java/Kotlin type in the Unity Android bridge carries @Keep.
#
# Unity reaches these types from C# only by string name via JNI (AndroidJavaClass and
# AndroidJavaProxy), so R8/ProGuard in a minified consumer app must not strip or rename them.
# purchases-android no longer ships a blanket `-keep class com.revenuecat.** { *; }` rule, so
# without @Keep these classes/interfaces can be removed or obfuscated and the JNI lookups break
# at runtime. Note: @Keep on an outer class does NOT protect its nested types — each public
# nested type needs its own @Keep.
# Related: https://github.com/RevenueCat/purchases-unity (proguard hardening)

set -euo pipefail

cd "$(dirname "$0")/.."

# Directories that hold the Android bridge compiled into consumer apps.
ROOTS=(
    "RevenueCat/Plugins/Android"
    "RevenueCatUI/Plugins/Android"
)

MISSING=0

while IFS= read -r -d '' file; do
    case "$file" in
        *.kt) lang=kotlin ;;
        *)    lang=java ;;
    esac

    # Visibility differs by language:
    #   Java   -> a type is public only if it carries the `public` modifier.
    #   Kotlin -> types are public by default; only private/internal/protected opt out.
    # Type keywords also differ: Kotlin uses object instead of Java's enum (enum class still
    # matches via `class`). Avoid \b so the regex works on both macOS awk and Linux gawk.
    awk -v file="$file" -v lang="$lang" '
        function trim(s) { gsub(/^[ \t]+|[ \t]+$/, "", s); return s }
        BEGIN {
            kw = (lang == "kotlin") ? "class|interface|object" : "class|interface|enum"
            decl_re = "(^|[^A-Za-z0-9_])(" kw ")[ \t]+[A-Za-z_]"
            name_re = ".*(" kw ")[ \t]+"
        }
        {
            t = trim($0)
            is_comment = (t ~ /^(\/\/|\/\*|\*)/)

            # Remember @Keep seen in the current contiguous annotation/comment block.
            if (!is_comment && t ~ /^@Keep([ \t(]|$)/) seen_keep = 1

            is_type = (!is_comment && t ~ decl_re)
            if (lang == "kotlin")
                is_public = (t !~ /(^|[ \t])(private|internal|protected)([ \t])/)
            else
                is_public = (t ~ /(^|[ \t])public([ \t])/)

            if (is_type && is_public) {
                if (t ~ /@Keep([ \t(]|$)/) seen_keep = 1   # @Keep inline on the decl line
                if (!seen_keep) {
                    name = t
                    sub(name_re, "", name)
                    sub(/[^A-Za-z0-9_].*/, "", name)
                    printf "%s:%d: public type \047%s\047 is missing @Keep\n", file, NR, name
                    found = 1
                }
                seen_keep = 0
                next
            }
            # A type decl line (even non-public) still ends the annotation block.
            if (is_type) { seen_keep = 0; next }

            # Annotations, comments and blank lines preserve the flag; other code clears it.
            if (!(t == "" || is_comment || t ~ /^@/)) seen_keep = 0
        }
        END { exit (found ? 1 : 0) }
    ' "$file" || MISSING=1
done < <(find "${ROOTS[@]}" \( -name '*.java' -o -name '*.kt' \) -not -path '*/build/*' -print0)

if [ "$MISSING" -ne 0 ]; then
    echo ""
    echo "ERROR: public type(s) above are missing @Keep."
    echo "Add 'import androidx.annotation.Keep;' and annotate each public type (including nested"
    echo "ones) with @Keep so it survives R8 minification in consumer apps."
    exit 1
fi

echo "✅ All public Java/Kotlin types in the Unity Android bridge are annotated with @Keep."
