#!/usr/bin/env bash
# Checks that every public Java/Kotlin type in the Unity Android bridge carries @Keep.
#
# Unity reaches these types from C# only by string name via JNI (AndroidJavaClass and
# AndroidJavaProxy), so R8/ProGuard in a minified consumer app must not strip or rename them.
# purchases-android no longer ships a blanket `-keep class com.revenuecat.** { *; }` rule, so
# without @Keep these classes/interfaces can be removed or obfuscated and the JNI lookups break
# at runtime. Note: @Keep on an outer class does NOT protect its nested types — each public
# nested type needs its own @Keep.
#
# Limitation: comment markers (//, /* */) inside string literals are not special-cased. This is
# harmless for declaration detection in practice and keeps the scanner simple.
# Related: https://github.com/RevenueCat/purchases-unity (proguard hardening)

set -euo pipefail

cd "$(dirname "$0")/.."

# Directories to scan. Defaults to the folders holding the Android bridge compiled into consumer
# apps; override by passing one or more paths as arguments (used by the fixture test suite).
if [ "$#" -gt 0 ]; then
    ROOTS=("$@")
else
    ROOTS=(
        "RevenueCat/Plugins/Android"
        "RevenueCatUI/Plugins/Android"
    )
fi

# Fail loudly if a root moved/was renamed, otherwise the scan below would silently pass.
for root in "${ROOTS[@]}"; do
    if [ ! -d "$root" ]; then
        echo "ERROR: source root '$root' does not exist. Did the Android bridge folders move?"
        echo "Update ROOTS in $(basename "$0") to match."
        exit 1
    fi
done

# Fail if there is nothing to scan, so the check can never become a no-op.
file_count=$(find "${ROOTS[@]}" \( -name '*.java' -o -name '*.kt' \) -not -path '*/build/*' | wc -l | tr -d ' ')
if [ "$file_count" -eq 0 ]; then
    echo "ERROR: no .java/.kt files found under the Android source roots; the @Keep check would be a no-op."
    exit 1
fi

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

        # Net parenthesis balance of a string ("(" minus ")"), used to span multi-line annotations.
        function nparen(s,   i, ch, c) {
            c = 0
            for (i = 1; i <= length(s); i++) {
                ch = substr(s, i, 1)
                if (ch == "(") c++
                else if (ch == ")") c--
            }
            return c
        }

        # True if the line carries an @Keep annotation token (anywhere, e.g. after another
        # annotation on the same line: `@SuppressWarnings(...) @Keep`).
        function haskeep(s) { return (s ~ /(^|[^A-Za-z0-9_])@Keep([ \t(]|$)/) }

        # Return the code-only part of a line, dropping // tails and /* */ spans (incl. multi-line
        # via the persistent in_block flag).
        function strip(s,   out, i, n, two) {
            out = ""; i = 1; n = length(s)
            while (i <= n) {
                two = substr(s, i, 2)
                if (in_block) {
                    if (two == "*/") { in_block = 0; i += 2 } else { i++ }
                    continue
                }
                if (two == "/*") { in_block = 1; i += 2; continue }
                if (two == "//") break
                out = out substr(s, i, 1); i++
            }
            return out
        }

        BEGIN {
            kw = (lang == "kotlin") ? "class|interface|object" : "class|interface|enum"
            decl_re = "(^|[^A-Za-z0-9_])(" kw ")[ \t]+[A-Za-z_]"
            name_re = ".*(" kw ")[ \t]+"
        }
        {
            t = trim(strip($0))

            # Continuation of a multi-line annotation argument list: stay in the annotation block.
            if (ann_depth > 0) {
                if (haskeep(t)) seen_keep = 1   # e.g. @Keep on the line that closes the args
                ann_depth += nparen(t)
                if (ann_depth < 0) ann_depth = 0
                next                          # preserve seen_keep
            }

            if (t == "") next                 # blank or fully-commented line: preserve seen_keep

            # Type declaration (checked before annotations so inline "@Keep public class X" works).
            if (t ~ decl_re) {
                if (haskeep(t)) seen_keep = 1                               # inline @Keep
                match(t, decl_re)
                prefix = substr(t, 1, RSTART - 1)                          # modifiers before keyword
                if (lang == "kotlin")
                    is_public = (prefix !~ /(^|[ \t])(private|internal|protected)([ \t]|$)/)
                else
                    is_public = (prefix ~ /(^|[ \t])public([ \t]|$)/)
                if (is_public && !seen_keep) {
                    name = t
                    sub(name_re, "", name)
                    sub(/[^A-Za-z0-9_].*/, "", name)
                    printf "%s:%d: public type \047%s\047 is missing @Keep\n", file, NR, name
                    found = 1
                }
                seen_keep = 0
                next
            }

            # Annotation line (no declaration on it).
            if (substr(t, 1, 1) == "@") {
                if (haskeep(t)) seen_keep = 1
                ann_depth = nparen(t)
                if (ann_depth < 0) ann_depth = 0
                next
            }

            # Any other code line ends the annotation block.
            seen_keep = 0
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
