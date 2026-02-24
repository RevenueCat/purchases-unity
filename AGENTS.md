# purchases-unity — Development Guidelines

This file provides guidance to AI coding agents when working with code in this repository.

## Project Overview

RevenueCat's official Unity SDK for in-app purchases and subscriptions. Provides C# scripts and native plugins for iOS and Android integration within Unity projects.

**Related repositories:**
- **iOS SDK**: https://github.com/RevenueCat/purchases-ios
- **Android SDK**: https://github.com/RevenueCat/purchases-android
- **Hybrid Common**: https://github.com/RevenueCat/purchases-hybrid-common — Native bridge layer

When implementing features or debugging, check these repos for reference and patterns.

## Important: Public API Stability

**Do NOT introduce breaking changes to the public API.** The SDK is used by many production Unity games and apps.

**Safe changes:**
- Adding new optional parameters to existing methods
- Adding new classes, methods, or properties
- Bug fixes that don't change method signatures
- Internal implementation changes

**Requires explicit approval:**
- Removing or renaming public classes/methods/properties
- Changing method signatures (parameter types, required params)
- Changing return types
- Modifying behavior in ways that break existing integrations

## Code Structure

```
purchases-unity/
├── RevenueCat/                    # Main SDK package (UPM)
│   ├── Scripts/                   # C# core SDK files
│   │   ├── Purchases.cs           # Main entry point (MonoBehaviour singleton)
│   │   ├── PurchasesConfiguration.cs  # Builder pattern config
│   │   ├── PurchasesWrapper*.cs   # Platform-specific wrappers
│   │   └── *.cs                   # Data models and utilities
│   ├── Plugins/
│   │   ├── iOS/                   # Native iOS integration (Objective-C)
│   │   ├── Android/               # Native Android integration (Java)
│   │   └── Editor/                # Dependency configuration
│   └── Editor/                    # Editor tools & post-install
├── RevenueCatUI/                  # UI SDK package (UPM)
│   ├── Scripts/                   # Paywall/Customer center UI
│   └── Plugins/Android/           # Android UI library
├── IntegrationTests/              # Full Unity test project
│   └── Assets/APITests/           # API test files
├── Subtester/                     # Package creation testing project
├── scripts/
│   └── create-unity-package.sh    # Main build script
└── fastlane/                      # Release automation
```

## Common Development Commands

### Unity Package Creation

```bash
# Create .unitypackage files
./scripts/create-unity-package.sh -u <unity_path> [-v]
```

### Fastlane Commands

Refer to `fastlane/README.md` for fastlane actions

## Project Architecture

### Main Entry Point: `Purchases.cs`
- **MonoBehaviour Singleton**: Attach to GameObject or use `Purchases.shared`
- **Inspector Configuration**: API keys and options configurable in Unity Editor
- **Programmatic Setup**: `PurchasesConfiguration.Builder(apiKey)` for runtime config

### Platform Wrappers
- `PurchasesWrapperiOS.cs` — iOS native bridge
- `PurchasesWrapperAndroid.cs` — Android native bridge
- `PurchasesWrapperNoop.cs` — No-op for unsupported platforms
- `PurchasesWrapper.cs` — Abstract interface

### Core Data Models
- `CustomerInfo.cs` — Subscription status and entitlements
- `Offerings.cs` / `Package.cs` — Product offerings
- `StoreProduct.cs` / `SubscriptionInfo.cs` — Product details
- `EntitlementInfo.cs` — Entitlement data

### UI Components (RevenueCatUI)
- `PaywallPresenter.cs` — Paywall display logic
- `PaywallsBehaviour.cs` — Paywall behavior controller
- `CustomerCenterPresenter.cs` — Customer center UI

### Dependencies
- `purchases-hybrid-common` — Native iOS/Android bridge
- External Dependency Manager (EDM4U) — Manages native dependencies

## Constraints / Support Policy

| Platform | Minimum Version |
|----------|-----------------|
| Unity | 2021.3+ |
| iOS | 13.0+ |
| Android | API 21+ |
| Play Billing Library | 8.0.0+ |

Don't raise minimum versions unless explicitly required and justified.

## Testing

Tests are located in `IntegrationTests/Assets/APITests/`:

- `PurchasesAPITests.cs` — Main SDK tests
- `CustomerInfoAPITests.cs` — Customer info tests
- `EntitlementInfoAPITests.cs` — Entitlement tests
- And more...

Run tests via Unity Test Runner in Edit Mode.

## Development Workflow

1. Create feature branch from main
2. Modify C# scripts in `RevenueCat/Scripts/` or `RevenueCatUI/Scripts/`
3. Modify native layer classes `RevenueCat/Plugins/`
4. Write tests in `IntegrationTests/Assets/APITests/`
5. Test locally with Subtester project
6. Create PR with detailed explanation

## Assembly Definitions

- `RevenueCat/Scripts/revenuecat.purchases-unity.asmdef` — Main SDK
- `RevenueCat/Editor/revenuecat.purchases-unity.Editor.asmdef` — Editor tools
- `RevenueCatUI/Scripts/revenuecat.purchases-unity-ui.asmdef` — UI components

## Pull Request Labels

When creating a pull request, **always add one of these labels** to categorize the change:

| Label | When to Use |
|-------|-------------|
| `pr:feat` | New user-facing features or enhancements |
| `pr:fix` | Bug fixes |
| `pr:other` | Internal changes, refactors, CI, docs, or anything that shouldn't trigger a release |

## When the Task is Ambiguous

1. Search for similar existing implementation in this repo first
2. Check purchases-ios, purchases-android, and purchases-hybrid-common for patterns
3. If there's a pattern, follow it exactly
4. If not, propose options with tradeoffs and pick the safest default

## Guardrails

- **Don't invent APIs or file paths** — verify they exist before referencing them
- **Don't remove code you don't understand** — ask for context first
- **Don't make large refactors** unless explicitly requested
- **Keep diffs minimal** — only touch what's necessary, preserve existing formatting
- **Don't break the public API** — if tests fail, investigate why
- **Test on both iOS and Android** — validate on real devices when possible
- **Update VERSIONS.md** when dependencies change
- **Never commit API keys or secrets** — do not stage or commit credentials or sensitive data
