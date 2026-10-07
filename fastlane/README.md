fastlane documentation
----

# Installation

Make sure you have the latest version of the Xcode command line tools installed:

```sh
xcode-select --install
```

For _fastlane_ installation instructions, see [Installing _fastlane_](https://docs.fastlane.tools/#installing-fastlane)

# Available Actions

### bump

```sh
[bundle exec] fastlane bump
```

Bump version, edit changelog, and create pull request

### automatic_bump

```sh
[bundle exec] fastlane automatic_bump
```

Automatically bumps version, edit changelog, and create pull request

### github_release

```sh
[bundle exec] fastlane github_release
```

Make github release

### update_hybrid_common

```sh
[bundle exec] fastlane update_hybrid_common
```

Update hybrid common pod and gradle and pushes changes to a new branch if open_pr option is true

### xcode_build_maestro_test_app

```sh
[bundle exec] fastlane xcode_build_maestro_test_app
```

Build the Unity-generated Xcode project for iOS Simulator

### run_maestro_e2e_tests_ios

```sh
[bundle exec] fastlane run_maestro_e2e_tests_ios
```

Run maestro E2E tests on iOS (booted simulator required)

### run_maestro_e2e_tests_android

```sh
[bundle exec] fastlane run_maestro_e2e_tests_android
```

Run maestro E2E tests on Android (running emulator required)

### build_sdk_update_test_apps

```sh
[bundle exec] fastlane build_sdk_update_test_apps
```

Build released and local Unity SDK update test apps

### compile_sdk_update_test_apps_ios

```sh
[bundle exec] fastlane compile_sdk_update_test_apps_ios
```

Compile both Unity SDK update apps for iOS Simulator

### run_sdk_update_test

```sh
[bundle exec] fastlane run_sdk_update_test
```

Run a Unity SDK update Maestro test case

### tag_current_branch

```sh
[bundle exec] fastlane tag_current_branch
```

Tag current branch with current version number

----

This README.md is auto-generated and will be re-generated every time [_fastlane_](https://fastlane.tools) is run.

More information about _fastlane_ can be found on [fastlane.tools](https://fastlane.tools).

The documentation of _fastlane_ can be found on [docs.fastlane.tools](https://docs.fastlane.tools).
