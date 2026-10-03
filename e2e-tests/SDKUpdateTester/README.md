<!-- Created by Antonio Pallares. Copyright (c) 2026 RevenueCat, Inc. -->

# SDK update tests

This Unity app uses the C# Purchases SDK to log in, fetch customer info and offerings, and purchase
the `no_paywall` offering's monthly package. Both builds use `com.revenuecat.SDKUpdateTester`.
The release build resolves the published package from OpenUPM; the local build resolves `RevenueCat`
in this checkout. Build checks verify the package source, version, C# assembly path, native wrapper
versions, and the package's own hybrid-common dependency. The on-screen version comes from the
verified package metadata because Unity's SDK has no public version getter. It also displays the
resolved package source (`Registry` or `Local`) so the assertions distinguish builds even when their
SDK versions are equal. Each variant retains
its declared native dependencies.

The seven YAML files in `../maestro/sdk_update_tests` are unchanged copies from
[purchases-ios#7900](https://github.com/RevenueCat/purchases-ios/pull/7900),
[purchases-android#4381](https://github.com/RevenueCat/purchases-android/pull/4381), and
[purchases-kmp#1063](https://github.com/RevenueCat/purchases-kmp/pull/1063).
Release discovery and the install/run sequence use
[shared actions#161](https://github.com/RevenueCat/fastlane-plugin-revenuecat_internal/pull/161).
Remove the temporary plugin pin when that PR merges.

## Run locally

Use Unity 6000.2 with iOS and Android Build Support. Set `UNITY_PATH` to its editor executable and
`MAESTRO_TEST_STORE_API_KEY` to the Workflows Test Store key. CI provides `WORKFLOWS_TEST_STORE_API_KEY`
through the `maestro` context. Its `no_paywall` offering contains `$rc_monthly`, with product
`pro_monthly_subscription` granting `pro`. Keys are written into generated resources inside the
ignored build directory. Do not publish apps, APKs, generated projects, or derived data as artifacts.

```sh
bundle exec fastlane build_sdk_update_test_apps platform:android
bundle exec fastlane run_sdk_update_test platform:android test_case:anonymous_user
bundle exec fastlane run_sdk_update_test platform:android test_case:logged_in_user
```

Replace `android` with `ios` on a Mac for the iOS Simulator. Boot one device before running the tests.
Android builds share the default debug signing key and use version codes 1 and 2. The app is installed
over its previous version, preserving data. Each case starts clean, retries through the shared runner,
and stores its final JUnit results separately in `fastlane/test_output/sdk_update_tests`.

Both variants are saved under `build/sdk_update_tests/<platform>/{release,local}`, each with a
`version.txt`, package lock and dependency reports. Unity retains its last released version on main
until the next bump, so release discovery uses the next minor version as its upper bound to include
the current stable release. `release_version:` can select a specific published version for reproduction.

CI follows the existing Unity jobs: containers build both variants per platform, then Mac and Android
runners test the updates. Linux iOS builds generate Xcode projects; `compile_sdk_update_test_apps_ios`
builds both projects on the Mac runner. The `sdk-update-tests` pipeline action runs only these jobs
on demand. The normal build and release gates also include them.

## Coverage limitation

Fetching customer info online can restore entitlements from the backend and hide a lost local cache.
These shared flows verify retained identity and visible entitlements, but do not prove offline cache
preservation. Screenshot comparisons also allow a small pixel difference, so they cannot prove an exact
user-ID match. Stronger assertions should be agreed across the SDKs under
[SDK-4526](https://linear.app/revenuecat/issue/SDK-4526) and mirrored in the shared native flows.
