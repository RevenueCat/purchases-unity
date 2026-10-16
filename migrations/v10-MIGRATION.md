## v10 API Changes

This release updates the Android SDK dependency from v10 to v11, which uses Google Play Billing Library 9.1.0 and Kotlin 2.2. The iOS SDK dependency does not change major version.

### Migration Guides

- See [Android Native - V11 API Migration Guide](https://github.com/RevenueCat/purchases-android/blob/main/migrations/v11-MIGRATION.md) for a more thorough explanation of the Android changes.

### New Minimum Versions

- Android: compile SDK 35.

iOS and Android minimum OS versions do not change.

### Android compile SDK 35 or higher

The Android SDK requires apps to compile against API 35 or later. Unity compiles against the Target API Level, so set **Player Settings > Other Settings > Target API Level** to 35 or higher. Lower values fail the Gradle build with:

```
Dependency 'androidx.core:core:1.15.0' requires libraries and applications that
depend on it to compile against version 35 or later of the Android APIs.
```

Raising the Target API Level also raises `targetSdkVersion`, so your app gets the Android 15 behavior changes for apps targeting API 35, such as enforced edge-to-edge display.

The plugin's Android bridge is written in Java, so the Kotlin version of your Gradle template does not matter unless your project adds its own Kotlin code. In that case it must use Kotlin 2.1.0 or newer.

### `SyncAmazonPurchase` requires `purchaseTime`

`SyncAmazonPurchase` takes a new trailing `long purchaseTime` parameter. Pass the purchase time in milliseconds since epoch from the Amazon receipt.

```csharp
// Before
purchases.SyncAmazonPurchase(productID, receiptID, amazonUserID, isoCurrencyCode, price);

// After
purchases.SyncAmazonPurchase(productID, receiptID, amazonUserID, isoCurrencyCode, price, purchaseTimeMillis);
```

### Only if using `PurchasesAreCompletedBy.MyApp`: iOS paywalls no longer call `OnPurchaseCompleted`

When your `PurchaseLogic` completes a paywall purchase, iOS paywalls no longer call `OnPurchaseCompleted` for it. Previously it was called with a `null` `StoreTransaction`. This matches Android. Handle the purchase result in your `PurchaseLogic` instead.

### `PurchasesConfiguration.Builder` defaults match the native SDKs

Settings you don't set on `PurchasesConfiguration.Builder` used the C# default values instead of the native SDK defaults. They now match the native SDKs:

| Setting | Before | Now |
|---|---|---|
| `SetStoreKitVersion` | `StoreKit1` | `Default` |
| `SetShouldShowInAppMessagesAutomatically` | `false` | `true` |
| `SetEntitlementVerificationMode` | `Disabled` | `Informational` |

To keep the previous behavior, set these values explicitly.

### Jetpack Compose on Android

If your project has native Android code that uses Jetpack Compose, the `RevenueCatUI` package raises your Compose libraries to BOM 2026.06.01. See the [Android guide](https://github.com/RevenueCat/purchases-android/blob/main/migrations/v11-MIGRATION.md#jetpack-compose-revenuecatui).

### Blocked Play Store reports `PurchaseNotAllowedError`

When the Play Store is blocked on the device (for example in OEM kids modes), purchases now fail with `Error.Code` 3 (`ReadableErrorCode` `PurchaseNotAllowedError`) instead of 2 (`StoreProblemError`). Code 2 is still reported for other store failures, so handle the new code alongside it rather than replacing it.

### Other Play Billing libraries in your app

The Android SDK puts Play Billing Library 9.1.0 on your app's classpath. Any other in-app purchase plugin in the same app, for example one you use to complete purchases with your own code, must support Play Billing Library 9. Versions that use APIs it removed, such as `SkuDetails`, fail at build time or at runtime.

### Reporting undocumented issues:

Feel free to file an issue! [New RevenueCat Issue](https://github.com/RevenueCat/purchases-unity/issues/new/).
