### CocoaPods
Because of the CocoaPods trunk freeze (November 1 to 7), the `PurchasesHybridCommon` 20.0.0 pod this release depends on is not published yet; it will be after November 7. Until then, iOS projects that install through CocoaPods can't resolve this version. Let External Dependency Manager resolve iOS dependencies through Swift Package Manager, or stay on 9.x until the pod is available.

CocoaPods trunk becomes permanently read-only on December 2, 2026, so a later major release will drop CocoaPods support. We recommend moving to Swift Package Manager.

This release updates the Android SDK dependency from v10 to [v11](https://github.com/RevenueCat/purchases-android/releases/tag/11.0.0), which uses Google Play Billing Library 9.1.0, Kotlin 2.2 and Jetpack Compose BOM 2026.06.01. The iOS SDK dependency does not change major version. It also replaces Google's deprecated External Dependency Manager for Unity (EDM4U) with Unity's External Dependency Manager, which raises the minimum Unity version to 2022.3.33.

### Migration Guides

- See the [v10 migration guide](https://github.com/RevenueCat/purchases-unity/blob/main/migrations/v10-MIGRATION.md) for the full list of changes and how to adapt your project.
- See [Android Native - V11 API Migration Guide](https://github.com/RevenueCat/purchases-android/blob/main/migrations/v11-MIGRATION.md) for a more thorough explanation of the Android changes.

### 💥 Breaking Changes

- Minimum Unity version is 2022.3.33. Android builds must target API 35 or higher; projects with their own Kotlin code need Kotlin 2.1.0 or newer.
- Native dependencies resolve through Unity's External Dependency Manager (`com.unity.external-dependency-manager`). It is a UPM dependency of the package, and `Purchases.unitypackage` no longer bundles EDM4U; on first import the plugin installs Unity's manager if the project has none. Projects already using EDM4U can keep it.
- `SyncAmazonPurchase` takes a new trailing `purchaseTime` parameter.
- `PurchasesConfiguration.Builder` defaults now match the native SDKs: `StoreKitVersion.Default`, in-app messages shown automatically, `EntitlementVerificationMode.Informational`.
- iOS paywalls no longer call `OnPurchaseCompleted` for purchases without a `StoreTransaction`; previously it was called with `null`. This matches Android.
- Android: a blocked Play Store reports `PurchaseNotAllowedError` (code 3) instead of `StoreProblemError` (code 2).
- iOS: when offerings fail to load during `PurchasePackage`, the callback receives the offerings error instead of `ProductNotAvailableForPurchaseError`.
- Android apps with native Jetpack Compose code are raised to Compose BOM 2026.06.01 by the `RevenueCatUI` package.
- `Purchases.SubscriptionOption.InstallmentsInfo` is now the top-level `Purchases.InstallmentsInfo`, which adds the fields for 12-month commitment subscriptions on iOS.

* feat!: 12mo commitment support on iOS (#1141) via Will Taylor (@fire-at-will)
* [SDK-4535] Migrate to Unity's External Dependency Manager (#1117) via Toni Rico (@tonidero)
* Add required purchaseTime parameter to SyncAmazonPurchase (#1103) via Toni Rico (@tonidero)

### 🐞 Bugfixes
* Fix StoreKit version, In-app messages and Verification Mode defaults (#1108) via Rick (@rickvdl)

### 🔄 Other Changes
* ci: pause CocoaPods release checks during trunk freeze (#1158) via Antonio Pallares (@ajpallares)
* docs: explain the dual dependency manager case in the v10 migration guide (#1151) via Álvaro Brey (@AlvaroBrey)
* docs: cover the iOS offerings error change in the v10 migration guide (#1150) via Álvaro Brey (@AlvaroBrey)
* [10.0] Fix the AdMob package export and Subtester build after the EDM migration (#1149) via Álvaro Brey (@AlvaroBrey)
* Build against purchases-hybrid-common 20.0.0-SNAPSHOT (#1142) via Álvaro Brey (@AlvaroBrey)
* [10.0] Fix the Setup defaults test for the external purchase arguments (#1139) via Álvaro Brey (@AlvaroBrey)
* Add the v10 migration guide (#1132) via Álvaro Brey (@AlvaroBrey)

## RevenueCatUI SDK
### 🔄 Other Changes
* Remove the nullable StoreTransaction note from OnPurchaseCompleted (#1131) via Álvaro Brey (@AlvaroBrey)
