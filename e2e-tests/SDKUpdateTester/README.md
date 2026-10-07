<!-- Created by Antonio Pallares. Copyright (c) 2026 RevenueCat, Inc. -->

# SDK update tests

This Unity app purchases the `no_paywall` offering's monthly package, granting `pro`.
Maestro installs a build using the released SDK, then installs a build using this checkout over it.
It compares screenshots of the app user ID and active entitlements for anonymous and logged-in users.

## Run locally

From the repository root, install the tools (`mise install`), Ruby gems (`bundle install`),
Maestro and the Android SDK or Xcode. Set `MAESTRO_TEST_STORE_API_KEY` to the
`Workflows Test Store` project's key.

Install the Unity editor specified in [ProjectVersion.txt](ProjectSettings/ProjectVersion.txt)
with iOS and Android Build Support, and set `UNITY_PATH` to its editor executable.

Boot one Android emulator, then run:

```sh
bundle exec fastlane build_sdk_update_test_apps platform:android
bundle exec fastlane run_sdk_update_test platform:android test_case:anonymous_user
bundle exec fastlane run_sdk_update_test platform:android test_case:logged_in_user
```

For iOS, replace `android` with `ios` on a Mac with a booted simulator, and run
`bundle exec fastlane compile_sdk_update_test_apps_ios` after building and before running the cases.
