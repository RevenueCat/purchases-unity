This latest release updates the Android SDK dependency from v7 to [v8](https://github.com/RevenueCat/purchases-android/releases/tag/6.0.0) to use BillingClient 7 and updates the iOS SDK dependency from v4 to v5 to use StoreKit 2 by default in the SDK.

### Migration Guides

- See [Android Native - V8 API Migration Guide](https://github.com/RevenueCat/purchases-android/blob/main/migrations/v8-MIGRATION.md) for a more thorough explanation of the Android changes.
- See [iOS Native - V5 Migration Guide](https://github.com/RevenueCat/purchases-ios/blob/main/Sources/DocCDocumentation/DocCDocumentation.docc/V5_API_Migration_guide.md) for a more thorough explanation of the iOS changes. Notably, this version uses StoreKit 2 to process purchases by default.

### New Minimum OS Versions

This release raises the minumum required OS versions to the following:

- iOS 13.0
- tvOS 13.0
- watchOS 6.2
- macOS 10.15
- Android: SDK 21 (Android 5.0)

### In-App Purchase Key Required for StoreKit 2

In order to use StoreKit 2, you must configure your In-App Purchase Key in the RevenueCat dashboard. You can find instructions describing how to do this [here](https://www.revenuecat.com/docs/in-app-purchase-key-configuration).

### `usesStoreKit2IfAvailable` is now `storeKitVersion`

When configuring the SDK, the `usesStoreKit2IfAvailable` parameter has been replaced by an optional `storeKitVersion` parameter. It defaults to letting the iOS SDK determine the most appropriate version of StoreKit at runtime. If you'd like to use a specific version of StoreKit, you may provide a value for `storeKitVersion` like so:

```
Purchases purchases = GetComponent<Purchases>();
Purchases.PurchasesConfiguration purchasesConfiguration =
    Purchases.PurchasesConfiguration.Builder.Init("api_key")
    .SetStoreKitVersion(Purchases.StoreKitVersion.StoreKit2)
    .Build();
purchases.Configure(purchasesConfiguration);
```

### Observer Mode is now PurchasesAreCompletedBy

Version 7.0 of the SDK deprecates the term "Observer Mode" (and the APIs where this term was used), and replaces it with `PurchasesAreCompletedBy` (either RevenueCat or your app). When specifying that your app will complete purchases, you must provide the StoreKit version that your app is using to make purchases on iOS. If your app is only available on Android, you may provide any value since the native Android SDK ignores this value.

You can enable it when configuring the SDK:

```
Purchases purchases = GetComponent<Purchases>();
Purchases.PurchasesConfiguration purchasesConfiguration =
    Purchases.PurchasesConfiguration.Builder.Init("api_key")
    .SetPurchasesAreCompletedBy(Purchases.PurchasesAreCompletedBy.MyApp, Purchases.StoreKitVersion.StoreKit2)
    .Build();
purchases.Configure(purchasesConfiguration);
```

#### ⚠️ Observing Purchases Completed by Your App on macOS

By default, when purchases are completed by your app using StoreKit 2 on macOS, the SDK does not detect a user's purchase until after the user foregrounds the app after the purchase has been made. If you'd like RevenueCat to immediately detect the user's purchase, call `Purchases.recordPurchase(productID)` for any new purchases, like so:

```
Purchases purchases = GetComponent<Purchases>();
purchases.recordPurchase(productID, (transaction, error) => { ... });
```

#### Observing Purchases Completed by Your App with StoreKit 1

If purchases are completed by your app using StoreKit 1, you will need to explicitly configure the SDK to use StoreKit 1:

```typescript
Purchases purchases = GetComponent<Purchases>();
Purchases.PurchasesConfiguration purchasesConfiguration =
    Purchases.PurchasesConfiguration.Builder.Init("api_key")
    .SetPurchasesAreCompletedBy(Purchases.PurchasesAreCompletedBy.MyApp, Purchases.StoreKitVersion.StoreKit1)
    .Build();
purchases.Configure(purchasesConfiguration);
```

Full migration guide to V7: [Unity - V7 API Migration Guide](migrations/v7-MIGRATION.md)

### New Features
* `Amazon`: Add getAmazonLWAConsentStatus method to support Quick Subscribe (#442) via Mark Villacampa (@MarkVillacampa)
### Dependency Updates
* Bump rexml from 3.2.9 to 3.3.3 (#486) via dependabot[bot] (@dependabot[bot])
* Bump danger from 9.4.3 to 9.5.0 (#487) via dependabot[bot] (@dependabot[bot])
* Bump fastlane from 2.221.1 to 2.222.0 (#480) via dependabot[bot] (@dependabot[bot])
* Update `VERSIONS.md` to include Billing client version and update fastlane plugin (#476) via Toni Rico (@tonidero)
### Other Changes
* Fix `Gemfile.lock` with new fastlane plugin dependencies (#479) via Toni Rico (@tonidero)
* Update Unity IAP compatiiblity (#475) via Andy Boedo (@aboedo)
