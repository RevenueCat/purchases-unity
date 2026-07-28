# Subtester Sample App

A sample/tester app for the RevenueCat Unity SDK. This project imports the SDK packages via local UPM file references in `Packages/manifest.json`, so edits to `RevenueCat/` and `RevenueCatUI/` in the repo are picked up live during development.

## Requirements

- **Unity Editor**: 6000.2.6f2 (see `ProjectSettings/ProjectVersion.txt`)

## Setup

1. Open the project in the Unity Editor (6000.2.6f2).
2. Open the scene: `Assets/Scenes/Main.unity`
3. Configure RevenueCat API keys: select the Purchases component in the scene and set the API keys in its inspector fields.

## How It Works

The app is a UI Toolkit–driven tester built around `TesterApp.cs`. It displays a tab bar with screens for each major SDK feature area:

- **Identity**: Manage app user ID and customer identity.
- **Offerings**: Browse available offerings and products.
- **Purchase**: Execute purchases (only works on device; no-op in editor).
- **Customer**: View current customer information and active subscriptions.
- **Paywalls**: Display and test RevenueCatUI paywalls with custom variables.
- **Attributes**: Manage subscriber attributes.
- **Tools**: Utility functions (reset user, refresh data, etc.).

All screens log their output to an on-screen console.

## Running on Device

The RevenueCat SDK is a no-op in the Unity editor (native wrapper only exists on device). To test real SDK behavior:

1. Build to an iOS or Android device, or iOS simulator.
2. For iOS simulator, `SKConfig.storekit` is configured for StoreKit testing.

## Development

Edit files in `RevenueCat/` and `RevenueCatUI/`, then reload the scene or scripts in the editor—the local UPM references will pick up your changes immediately.
