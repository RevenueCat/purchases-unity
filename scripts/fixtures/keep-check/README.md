# keep-check fixtures

Fixtures for `scripts/test-check-android-keep-annotations.sh`, which exercises
`scripts/check-android-keep-annotations.sh`.

These `.java`/`.kt` files are **not compiled** by anything — they live outside the SDK source
roots and the Unity Assets/Gradle source sets. They exist only to pin the checker's behavior.

- `pass/` — every public type is annotated (or is exempt: package-private Java, `private`/`internal`
  Kotlin, commented-out, or nested-private). The checker must report **zero** findings here.
- `fail/` — public types intentionally missing `@Keep`. The checker must flag exactly the type
  names the test harness expects.
- `empty/` — no `.java`/`.kt`, to exercise the "no files to scan" guard.

When you add a case, drop a file in `pass/` or `fail/`. For `fail/`, also add the expected flagged
type name(s) to `EXPECTED_FLAGGED` in the test harness.
