# Maestro E2E Test App

A minimal Unity app used by Maestro end-to-end tests to verify RevenueCat SDK integration.

## Prerequisites

- Unity Editor (with iOS Build Support and/or Android Build Support modules)
- Xcode (iOS) / Android Studio (Android)
- [Maestro](https://maestro.mobile.dev/) CLI

## Setup

This project requires manual scene setup in the Unity Editor because `.unity` scene
files cannot be created outside the editor. See [setup-instructions.md](./setup-instructions.md)
for detailed step-by-step instructions.

## Running Locally

1. Open the project in Unity Editor
2. File > Build Settings > select iOS or Android
3. Build and Run

## API Key

The app initialises RevenueCat with the placeholder `MAESTRO_TESTS_REVENUECAT_API_KEY`.
CI replaces this placeholder with the real key from the
`RC_E2E_TEST_API_KEY_PRODUCTION_TEST_STORE` environment variable (provided by the
CircleCI `e2e-tests` context) before building.

To run locally, either:
- Replace the placeholder in `Assets/Scripts/MaestroTestApp.cs` with a valid API key
  (do **not** commit it), or
- Substitute it the same way CI does:

```sh
sed -i '' "s|MAESTRO_TESTS_REVENUECAT_API_KEY|$RC_E2E_TEST_API_KEY_PRODUCTION_TEST_STORE|g" \
  Assets/Scripts/MaestroTestApp.cs
```

The checkpoint flows also pass an optional `rc_test_api_key` launch argument, read from
`MAESTRO_RC_TEST_API_KEY`. When it is set, the app configures Purchases with it instead of the
placeholder, so a local run never needs the key written to source. For example, with the key in the
macOS login Keychain:

```sh
MAESTRO_RC_TEST_API_KEY="$(security find-generic-password -s RC_TEST_API_KEY -w ~/Library/Keychains/login.keychain-db)" \
  maestro --device <simulator-or-emulator-id> test \
  ../maestro/e2e_tests/checkpoint_custom_presenter_purchase.yaml \
  ../maestro/e2e_tests/checkpoint_custom_presenter_back.yaml \
  ../maestro/e2e_tests/checkpoint_custom_presenter_unmount.yaml \
  ../maestro/e2e_tests/checkpoint_custom_error_dialog.yaml
```

## Custom checkpoint coverage

The custom presenter screen receives its offering and typed custom variables from the native
checkpoint. A checkpoint owner object holds the checkpoint registration while the screen owns the
paywall UI, so the unmount flow detaches a real registration without dismissing the paywall.
Counters expose premature callbacks and callbacks to a detached owner instead of hiding them.

The four flows cover buying before completing the presentation, backing out and retrying,
completing after the owner detaches and then remounting, and custom error-dialog content. The error
case uses the invalid checkpoint identifier `!` so it doesn't rely on a network outage to reach the
error-dialog factory.

## RevenueCat Project

The test uses a RevenueCat project configured with:
- A **V2 Paywall** (the test asserts "Paywall V2" is visible)
- A `default_workflows` offering with the three-page workflow and exit offer used by the
  `paywall_exit_offer_dismisses` Maestro flow
- A `pro` entitlement (the test checks entitlement status after purchase)
- The **Test Store** environment for purchase confirmation
- A checkpoint `donottouch_sdk_everyone` whose flow presents a paywall to every user with the
  monthly package (`pro_monthly_subscription`, granting `pro`) preselected, used by the
  `checkpoint_purchase` and custom presenter flows. The `checkpoint_unknown_identifier` flow uses an
  identifier that must **not** exist in the dashboard.

## Dependencies

The RevenueCat and RevenueCatUI Unity packages must be imported into the project
manually via the Unity Package Manager.
