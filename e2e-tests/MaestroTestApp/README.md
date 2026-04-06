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
In CI, the Fastlane lane replaces this placeholder with the real key from the
`RC_E2E_TEST_API_KEY_PRODUCTION_TEST_STORE` environment variable (provided by the
CircleCI `e2e-tests` context) before building.

To run locally, either:
- Replace the placeholder in `Assets/Scripts/MaestroTestApp.cs` with a valid API key
  (do **not** commit it), or
- Export the env var and run the same `sed` command the Fastlane lane uses.

## RevenueCat Project

The test uses a RevenueCat project configured with:
- A **V2 Paywall** (the test asserts "Paywall V2" is visible)
- A `pro` entitlement (the test checks entitlement status after purchase)
- The **Test Store** environment for purchase confirmation

## Dependencies

The RevenueCat and RevenueCatUI Unity packages must be imported into the project
manually via the Unity Package Manager.
