## 5.0.0-alpha.1
**RevenueCat Purchases Unity v5** is here!! 😻

This latest release updates the Android SDK dependency from v5 to [v6](https://github.com/RevenueCat/purchases-android/releases/tag/6.0.0) to use BillingClient 5. This version of BillingClient brings an entire new subscription model which has resulted in large changes across the entire SDK.

### Migration Guides
- See [Android Native - 5.x to 6.x Migration](https://www.revenuecat.com/docs/android-native-5x-to-6x-migration) for a
  more thorough explanation of the new Google subscription model announced with BillingClient 5 and how to take
  advantage of it in V6. This guide includes tips on product setup with the new model.

### New `SubscriptionOption` concept

#### Purchasing
In v4, a Google Play Android `Package` or `StoreProduct` represented a single purchaseable entity, and free trials or intro
offers would automatically be applied to the purchase if the user was eligible.

Now, in Unity v5, a Google Play Android `Package` or `StoreProduct` represents a duration of a subscription and contains all the ways to purchase that duration -- any offers and its base plan. Each of these purchase options are `SubscriptionOption`s.
When passing a `Package` to `purchasePackage()` or `StoreProduct` to `purchaseStoreProduct()`, the SDK will use the following logic to choose which
`SubscriptionOption` to purchase:
- Filters out offers with "rc-ignore-offer" tag
- Uses `SubscriptionOption` with the longest free trial or cheapest first phase
    - Only offers the user is eligible will be applied
- Falls back to base plan

For more control, purchase subscription options with the new `purchaseSubscriptionOption()` method.

#### Models

`StoreProduct` now has a few new properties use for Google Play Android:
- `defaultOption`
  - A subscription option that will automatically be applie when purchasing a `Package` or `StoreProduct`
- `subscriptionOptions`
  - A list of subscription options (could be null)

### Observer Mode

Observer mode is still supported in v5. Other than updating the SDK version, there are no changes required.

### Offline Entitlements

✨ With this new feature, even if our main and backup servers are down, the SDK can continue to process purchases. This is enabled transparently to the user, and when the servers come back online, the SDK automatically syncs the information so it can be visible in the dashboard.

### Offering Metadata

✨ Metadata allows attaching arbitrary information as key/value pairs to your Offering to control how to display your products inside your app. The metadata you configure in an Offering is available from the RevenueCat SDK. For example, you could use it to remotely configure strings on your paywall, or even URLs of images shown on the paywall.

See the [metadata documentation](https://www.revenuecat.com/docs/offering-metadata) for more info!

## 4.16.0
### Dependency Updates
* [AUTOMATIC BUMP] Updates purchases-hybrid-common to 4.18.0 (#288) via RevenueCat Git Bot (@RCGitBot)
  * [iOS 4.21.0](https://github.com/RevenueCat/purchases-ios/releases/tag/4.21.0)
  * [iOS 4.19.1](https://github.com/RevenueCat/purchases-ios/releases/tag/4.19.1)
  * [iOS 4.20.0](https://github.com/RevenueCat/purchases-ios/releases/tag/4.20.0)
* Bump fastlane from 2.212.2 to 2.213.0 (#285) via dependabot[bot] (@dependabot[bot])

## 4.15.0
### Dependency Updates
* [AUTOMATIC BUMP] Updates purchases-hybrid-common to 4.16.0 (#282) via RevenueCat Git Bot (@RCGitBot)
  * [iOS 4.19.0](https://github.com/RevenueCat/purchases-ios/releases/tag/4.19.0)

## 4.14.0
### Dependency Updates
* [AUTOMATIC BUMP] Updates purchases-hybrid-common to 4.15.0 (#279) via RevenueCat Git Bot (@RCGitBot)
  * [iOS 4.18.0](https://github.com/RevenueCat/purchases-ios/releases/tag/4.18.0)
* Bump danger from 9.2.0 to 9.3.0 (#275) via dependabot[bot] (@dependabot[bot])
### Other Changes
* Update fastlane-plugin-revenuecat version (#277) via Cesar de la Vega (@vegaro)

## 4.13.3
### Dependency Updates
* [AUTOMATIC BUMP] Updates purchases-hybrid-common to 4.14.3 (#272) via RevenueCat Git Bot (@RCGitBot)
  * [iOS 4.17.11](https://github.com/RevenueCat/purchases-ios/releases/tag/4.17.11)
  * [iOS 4.17.10](https://github.com/RevenueCat/purchases-ios/releases/tag/4.17.10)
* Bump fastlane from 2.212.1 to 2.212.2 (#270) via dependabot[bot] (@dependabot[bot])

## 4.13.2
### Dependency Updates
* [AUTOMATIC BUMP] Updates purchases-hybrid-common to 4.14.2 (#267) via RevenueCat Git Bot (@RCGitBot)
  * [iOS 4.17.9](https://github.com/RevenueCat/purchases-ios/releases/tag/4.17.9)

## 4.13.1
### Dependency Updates
* [AUTOMATIC BUMP] Updates purchases-hybrid-common to 4.14.1 (#263) via RevenueCat Git Bot (@RCGitBot)
  * [iOS 4.17.8](https://github.com/RevenueCat/purchases-ios/releases/tag/4.17.8)

## 4.13.0
### New Features
* add `ImmediateAndChargeFullPrice` proration mode (#258) via Andy Boedo (@aboedo)
### Dependency Updates
* [AUTOMATIC BUMP] Updates purchases-hybrid-common to 4.14.0 (#260) via RevenueCat Git Bot (@RCGitBot)
  * [Android 5.8.2](https://github.com/RevenueCat/purchases-android/releases/tag/5.8.2)
### Other Changes
* deprecate `UsesStoreKit2IfAvailable` field (#259) via Andy Boedo (@aboedo)

## 4.12.2
### Dependency Updates
* [AUTOMATIC BUMP] Updates purchases-hybrid-common to 4.13.5 (#255) via RevenueCat Git Bot (@RCGitBot)

## 4.12.1
### Dependency Updates
* [AUTOMATIC BUMP] Updates purchases-hybrid-common to 4.13.4 (#252) via RevenueCat Git Bot (@RCGitBot)
  * [iOS 4.17.7](https://github.com/RevenueCat/purchases-ios/releases/tag/4.17.7)
* Bump fastlane from 2.212.0 to 2.212.1 (#251) via dependabot[bot] (@dependabot[bot])
### Other Changes
* Runs integration tests on main too (#249) via Cesar de la Vega (@vegaro)

## 4.12.0
### New Features
* Adds Purchases.SetLogHandler (#237) via Cesar de la Vega (@vegaro)
### Bugfixes
* Fix for `NSInvalidArgumentException` in `GetPromotionalOffer` (#245) via Cesar de la Vega (@vegaro)
### Dependency Updates
* [AUTOMATIC BUMP] Updates purchases-hybrid-common to 4.13.3 (#247) via RevenueCat Git Bot (@RCGitBot)
  * [iOS 4.17.6](https://github.com/RevenueCat/purchases-ios/releases/tag/4.17.6)
* [AUTOMATIC BUMP] Updates purchases-hybrid-common to 4.13.2 (#244) via RevenueCat Git Bot (@RCGitBot)
  * [iOS 4.17.6](https://github.com/RevenueCat/purchases-ios/releases/tag/4.17.6)
* Bump fastlane from 2.211.0 to 2.212.0 (#243) via dependabot[bot] (@dependabot[bot])

## 4.11.0
### New Features
* Added `Purchases.SetLogLevel` (#226) via NachoSoto (@NachoSoto)
### Dependency Updates
* [AUTOMATIC BUMP] Updates purchases-hybrid-common to 4.13.1 (#240) via RevenueCat Git Bot (@RCGitBot)
  * [Android 5.7.1](https://github.com/RevenueCat/purchases-android/releases/tag/5.7.1)
  * [iOS 4.17.5](https://github.com/RevenueCat/purchases-ios/releases/tag/4.17.5)
* [AUTOMATIC BUMP] Updates purchases-hybrid-common to 4.13.0 (#239) via RevenueCat Git Bot (@RCGitBot)
  * [Android 5.7.1](https://github.com/RevenueCat/purchases-android/releases/tag/5.7.1)
  * [iOS 4.17.5](https://github.com/RevenueCat/purchases-ios/releases/tag/4.17.5)

## 4.10.1
### Dependency Updates
* [AUTOMATIC BUMP] Updates purchases-hybrid-common to 4.12.1 (#235) via RevenueCat Git Bot (@RCGitBot)
  * [iOS 4.17.4](https://github.com/RevenueCat/purchases-ios/releases/tag/4.17.4)
### Other Changes
* Adds dependabot (#234) via Cesar de la Vega (@vegaro)

## 4.10.0
### Dependency Updates
* Update gems (#231) via Cesar de la Vega (@vegaro)
* [AUTOMATIC BUMP] Updates purchases-hybrid-common to 4.12.0 (#230) via RevenueCat Git Bot (@RCGitBot)
  * [Android 5.7.0](https://github.com/RevenueCat/purchases-android/releases/tag/5.7.0)
  * [iOS 4.17.3](https://github.com/RevenueCat/purchases-ios/releases/tag/4.17.3)
### Other Changes
* `.gitignore` `.vsconfig` Visual Studio files (#227) via NachoSoto (@NachoSoto)

## 4.9.0
### New Features
* Add StoreProduct.SubscriptionPeriod (#222) via Cesar de la Vega (@vegaro)
### Dependency Updates
* [AUTOMATIC BUMP] Updates purchases-hybrid-common to 4.10.0 (#224) via RevenueCat Git Bot (@RCGitBot)
  * [Android 5.6.7](https://github.com/RevenueCat/purchases-android/releases/tag/5.6.7)
  * [iOS 4.17.2](https://github.com/RevenueCat/purchases-ios/releases/tag/4.17.2)
  * [iOS 4.17.1](https://github.com/RevenueCat/purchases-ios/releases/tag/4.17.1)
  * [iOS 4.17.0](https://github.com/RevenueCat/purchases-ios/releases/tag/4.17.0)

## 4.8.0
### Dependency Updates
* [AUTOMATIC BUMP] Updates purchases-hybrid-common to 4.8.0 (#217) via RevenueCat Git Bot (@RCGitBot)
### Other Changes
* Update Gemfile.lock (#218) via Cesar de la Vega (@vegaro)

## 4.7.0
### New Features
* Added SetCleverTapID, SetMixpanelDistinctID and SetFirebaseAppInstanceID (#209) via Andy Boedo (@aboedo)
### Dependency Updates
* [AUTOMATIC BUMP] Updates purchases-hybrid-common to 4.7.0 (#211) via RevenueCat Git Bot (@RCGitBot)
  * [iOS 4.16.0](https://github.com/RevenueCat/purchases-ios/releases/tag/4.16.0)
### Other Changes
* Adds missing params for better changelogs when bumping (#214) via Cesar de la Vega (@vegaro)
* Update fastlane plugin (#213) via Cesar de la Vega (@vegaro)
* added extra link to the correct file for symlinking sources (#210) via Andy Boedo (@aboedo)
* add openupm badge (#204) via Andy Boedo (@aboedo)
* remove stalebot in favor of SLAs in Zendesk (#208) via Andy Boedo (@aboedo)
* Update fastlane-plugin-revenuecat_internal to latest version (#206) via Cesar de la Vega (@vegaro)

## 4.6.4
### Other Changes
* Support unity package manager (UPM) (#175) via Toni Rico (@tonidero)
* [AUTOMATIC BUMP] Updates purchases-hybrid-common to 4.5.4 (#201) via RevenueCat Git Bot (@RCGitBot)

## 4.6.3
### Other Changes
* [AUTOMATIC BUMP] Updates purchases-hybrid-common to 4.5.3 (#198) via RevenueCat Git Bot (@RCGitBot)

## 4.6.2
### Other Changes
* [AUTOMATIC BUMP] Updates purchases-hybrid-common to 4.5.2 (#195) via RevenueCat Git Bot (@RCGitBot)

## 4.6.1
### Other Changes
* [AUTOMATIC BUMP] Updates purchases-hybrid-common to 4.5.1 (#192) via RevenueCat Git Bot (@RCGitBot)

## 4.6.0
### Other Changes
* [AUTOMATIC BUMP] Updates purchases-hybrid-common to 4.5.0 (#189) via RevenueCat Git Bot (@RCGitBot)

## 4.5.3
### Other Changes
* [AUTOMATIC BUMP] Updates purchases-hybrid-common to 4.4.4 (#186) via RevenueCat Git Bot (@RCGitBot)

## 4.5.2
### Other Changes
* [AUTOMATIC BUMP] Updates purchases-hybrid-common to 4.4.3 (#183) via RevenueCat Git Bot (@RCGitBot)
* Fix CircleCI deployment workflow (#180) via Toni Rico (@tonidero)

## 4.5.1
### Other Changes
* Fix github release workflow (#177) via Toni Rico (@tonidero)
* [AUTOMATIC BUMP] Updates purchases-hybrid-common to 4.4.1 (#178) via RevenueCat Git Bot (@RCGitBot)
* Update Fastlane plugin (#173) via Cesar de la Vega (@vegaro)

## 4.5.0
### Other Changes
* [AUTOMATIC] Updates purchases-hybrid-common to 4.4.0 (#172) via RevenueCat Git Bot (@RCGitBot)
* Makes hold job depend on tests (#170) via Cesar de la Vega (@vegaro)
* Adds missing parameter to bump_version_update_changelog_create_pr (#169) via Cesar de la Vega (@vegaro)

## 4.4.1
### Other Changes
* [AUTOMATIC] Updates purchases-hybrid-common to 4.3.6 (#166) via RevenueCat Git Bot (@RCGitBot)
* Update fastlane-plugin-revenuecat_internal (#167) via Cesar de la Vega (@vegaro)

## 4.4.0
### Bugfixes
* fix compatibility issues with C# 7.3 (#162) via Andy Boedo (@aboedo)
### Other Changes
* [AUTOMATIC] Updates purchases-hybrid-common to 4.3.2 (#163) via RevenueCat Git Bot (@RCGitBot)
* Release trains (#159) via Cesar de la Vega (@vegaro)

## 4.3.1
### Bugfixes
* Update receivedUpdatedCustomerInfo delegate (#158) via Cesar de la Vega (@vegaro)
### Other Changes
* Update fastlane plugin (#156) via Cesar de la Vega (@vegaro)
* Add new SetUsesStoreKit2IfAvailable api to api tests (#154) via Toni Rico (@tonidero)

## 4.3.0
### New Features
* Support for `usesStoreKit2IfAvailable` config option (#151) via Toni Rico (@tonidero)
### Bugfixes
* Fix support for DangerousSettings in iOS (#152) via Toni Rico (@tonidero)

## 4.2.0
### ⚠️ Important: If you're using RevenueCat along with Unity IAP side-by-side ⚠️
Starting with this version you need to use Unity IAP 4.4.0+ if you are using Unity IAP together with RevenueCat in your project. You can update from Unity Package Manager.
If you need to use an older version of Unity IAP, you can continue to use purchases-unity < 4.2.0.
### API Changes
* `StoreTransaction`: `RevenueCatId` and `ProductId` have been deprecated in favor of `TransactionIdentifier` and `ProductIdentifier` respectively. (#145) via Toni Rico (@tonidero)
### Bugfixes
* Fix example compatibility with Unity 2020 (#139) via Andy Boedo (@aboedo)
### Other Changes
* Subtester: Fix unity android export (#142) via Andy Boedo (@aboedo)
* Update AMAZON-INSTRUCTIONS.md (#143) via Andy Boedo (@aboedo)

## 4.1.0

- Introduced New AdServices Integration on iOS. Call `EnableAdServicesAttributionTokenCollection()` to enable this integration.
More information [in our docs](https://docs.revenuecat.com/docs/apple-search-ads) and in the [announcement blogpost](https://www.revenuecat.com/blog/iad-vs-adservices-whats-the-difference/).

## 4.0.0

RevenueCat Unity SDK v4 is here!!

![Dancing cats](https://media.giphy.com/media/lkbNG2zqzHZUA/giphy.gif)

[Full Changelog](https://github.com/revenuecat/purchases-unity/compare/3.5.3...4.0.0)


#### Amazon Appstore Support
We have introduced support for using the Amazon Appstore. We have extensively tested this, and there are some apps using our pre-release Amazon versions in production.

However, we have found some inconsistencies in the way Amazon Appstore prices are reported. We are actively working on patching these inconsistencies.

Please help us help you by reporting any issues you find. [New RevenueCat Issue](https://github.com/RevenueCat/purchases-unity/issues/new/).

You can enable Amazon Appstore support by configuring the SDK using the new `RevenueCatAPIKeyAmazon` field.

For more information around configuration please take a look at the [Amazon Appstore section in our docs](https://docs.revenuecat.com/docs/amazon-platform-resources). The official [Amazon In-App Purchasing docs](https://developer.amazon.com/docs/in-app-purchasing/iap-overview.html) also contain very valuable information, especially around testing and best practices.

⚠️ ⚠️ Important ⚠️ ⚠️ In order to use Unity IAP < 4.4.0 with RevenueCat in Observer mode,
you need to use the `Purchases-UnityIAP.unityPackage`. For Unity IAP >= 4.4.0, you can use the `Purchases.unitypackage` package.

For Amazon installation instructions please follow take a look at the [Amazon instructions document](https://github.com/RevenueCat/purchases-unity/blob/4.0.0/AMAZON-INSTRUCTIONS.md)

#### StoreKit 2 support
This version of the SDK automatically uses StoreKit 2 APIs under the hood only for APIs that the RevenueCat team has determined work better than StoreKit 1.

#### New types and cleaned up naming
New types that wrap native types from Apple, Google and Amazon, and we cleaned up the naming of other types and methods for a more consistent experience.

### Removed APIs
- `Identify` and `CreateAlias` have been removed in favor of `LogIn`.
- `Reset` has been removed in favor of `LogOut`.
- `GetEntitlements` has been removed in favor of `GetOfferings`.
- `AttributionKey` and `Purchases.AddAttributionData` have been removed in favor of `Set<NetworkID> methods`.
- `revenueCatAPIKey` has been removed in favor of `revenueCatAPIKeyApple`, `revenueCatAPIKeyGoogle` and `revenueCatAPIKeyAmazon`.

### Renamed APIs

| 3.x | 4.0.0 |
| :-: | :-: |
| `PurchaserInfo` | `CustomerInfo` |
| `Transaction` | `StoreTransaction` |
| `Product` | `StoreProduct` |
| `PaymentDiscount` | `PromotionalOffer` |
| `Purchases.RestoreTransactions` | `Purchases.RestorePurchases` |
| `Purchases.GetPaymentDiscount` | `Purchases.GetPromotionalOffer` |
| `Purchases.UpdatedPurchaserInfoListener` | `Purchases.UpdatedCustomerInfoListener` |
| `Discount.identifier` | `Discount.Identifier` |
| `Discount.price` | `Discount.Price` |
| `Discount.priceString` | `Discount.PriceString` |
| `Discount.cycles` | `Discount.Cycles` |
| `Discount.period` | `Discount.Period` |
| `Discount.unit` | `Discount.Unit` |
| `Discount.periodUnit` | `Discount.PeriodUnit` |
| `Discount.periodNumberOfUnits` | `Discount.PeriodNumberOfUnits` |
| `Error.message` | `Error.Message` |
| `Error.code` | `Error.Code` |
| `Error.underlyingErrorMessage` | `Error.UnderlyingErrorMessage` |
| `Error.readableErrorCode` | `Error.ReadableErrorCode` |


### New APIs
- Introduced the ability to configure the SDK programmatically through calling `Configure` https://github.com/RevenueCat/purchases-unity/pull/125

### Other
- Added docstrings for most public entities. https://github.com/RevenueCat/purchases-unity/pull/131, https://github.com/RevenueCat/purchases-unity/pull/130, https://github.com/RevenueCat/purchases-unity/pull/128

## 4.0.0-rc.2

### New changes since Release Candidate 1

- Updated `purchases-hybrid-common` to 3.3.0 [Changelog here](https://github.com/RevenueCat/purchases-hybrid-common/releases/tag/3.3.0)
- Updated `purchases-android` to 5.3.0, which fixes a null pointer exception when calling configure from a thread in Android.  [Changelog here](https://github.com/RevenueCat/purchases-android/releases/tag/5.3.0)
- Updated instructions for observer mode, which don't require excluding the billing client anymore.

## 4.0.0-rc.1
⚠️ ⚠️ Important ⚠️ ⚠️ Observer mode for Amazon is not supported yet.

⚠️ ⚠️ Important ⚠️ ⚠️ In order to use Unity IAP in order with RevenueCat in Observer mode,
you need to use the `Purchases-UnityIAP.unityPackage`.

For Amazon installation instructions please follow take a look at the [Amazon instructions document](https://github.com/RevenueCat/purchases-unity/blob/4.0.0-rc.1/AMAZON-INSTRUCTIONS.md)

### Features

#### Amazon Appstore Support
We have introduced support for using the Amazon Appstore. We have extensively tested this, and there are some apps using our pre-release Amazon versions in production.

However, we have found some inconsistencies in the way Amazon Appstore prices are reported. We are actively working on patching these inconsistencies.

Please help us help you by reporting any issues you find. [New RevenueCat Issue](https://github.com/RevenueCat/purchases-unity/issues/new/).

You can enable Amazon Appstore support by configuring the SDK using the new `RevenueCatAPIKeyAmazon` field.

For more information around configuration please take a look at the [Amazon Appstore section in our docs](https://docs.revenuecat.com/docs/amazon-platform-resources). The official [Amazon In-App Purchasing docs](https://developer.amazon.com/docs/in-app-purchasing/iap-overview.html) also contain very valuable information, especially around testing and best practices.

#### StoreKit 2 support
This version of the SDK automatically uses StoreKit 2 APIs under the hood only for APIs that the RevenueCat team has determined work better than StoreKit 1.

#### New types and cleaned up naming
New types that wrap native types from Apple, Google and Amazon, and we cleaned up the naming of other types and methods for a more consistent experience.

### Removed APIs
- `Identify` and `CreateAlias` have been removed in favor of `LogIn`.
- `Reset` has been removed in favor of `LogOut`.
- `GetEntitlements` has been removed in favor of `GetOfferings`.
- `AttributionKey` and `Purchases.AddAttributionData` have been removed in favor of `Set<NetworkID> methods`.
- `revenueCatAPIKey` has been removed in favor of `revenueCatAPIKeyApple`, `revenueCatAPIKeyGoogle` and `revenueCatAPIKeyAmazon`.

### Renamed APIs

| 3.x | 4.0.0 |
| :-: | :-: |
| `PurchaserInfo` | `CustomerInfo` |
| `Transaction` | `StoreTransaction` |
| `Product` | `StoreProduct` |
| `PaymentDiscount` | `PromotionalOffer` |
| `Purchases.RestoreTransactions` | `Purchases.RestorePurchases` |
| `Purchases.GetPaymentDiscount` | `Purchases.GetPromotionalOffer` |
| `Purchases.UpdatedPurchaserInfoListener` | `Purchases.UpdatedCustomerInfoListener` |
| `Discount.identifier` | `Discount.Identifier` |
| `Discount.price` | `Discount.Price` |
| `Discount.priceString` | `Discount.PriceString` |
| `Discount.cycles` | `Discount.Cycles` |
| `Discount.period` | `Discount.Period` |
| `Discount.unit` | `Discount.Unit` |
| `Discount.periodUnit` | `Discount.PeriodUnit` |
| `Discount.periodNumberOfUnits` | `Discount.PeriodNumberOfUnits` |
| `Error.message` | `Error.Message` |
| `Error.code` | `Error.Code` |
| `Error.underlyingErrorMessage` | `Error.UnderlyingErrorMessage` |
| `Error.readableErrorCode` | `Error.ReadableErrorCode` |

## 3.5.3

When installing this release, make sure to make a fresh installation by removing the RevenueCat folder before importing the package. That way obsolete files are deleted from your project.

- Fixed a bug where [namespace Editor on iOS post install collided with UnityEditor.Editor](https://github.com/RevenueCat/purchases-unity/issues/98)
    https://github.com/RevenueCat/purchases-unity/pull/99
- Renamed `Dummy.swift` to `PurchasesDummy.swift` to fix collisions with other plugins that also add a `Dummy.swift` file. https://github.com/RevenueCat/purchases-unity/pull/100

## 3.5.2

- Fixed a [crash when the deprecatedLegacyRevenueCatAPIKey hadn't been set when running on Android simulators](https://github.com/RevenueCat/purchases-unity/issues/89)
    https://github.com/RevenueCat/purchases-unity/pull/93
- Fixed a bug where `CheckTrialOrIntroductoryPriceEligibility` could prompt for login credentials on iOS if the user hasn't logged into App Store.
- Bump`purchases-hybrid-common` to `2.0.1` [Changelog here](https://github.com/RevenueCat/purchases-hybrid-common/releases/tag/2.0.0)
- Bump `purchases-ios` to `3.14.1` ([Changelog here](https://github.com/RevenueCat/purchases-ios/releases/3.14.1))
- Bump `purchases-android` to `4.6.1` ([Changelog here](https://github.com/RevenueCat/purchases-android/releases/4.6.1))

## 3.5.1

- Fixed issue where builds got rejected when uploading to App Store Connect because of the existence of a `Frameworks` folder in the archive
    https://github.com/RevenueCat/purchases-unity/pull/84
- Added post-install script that automatically links the `StoreKit` framework to the main target
    https://github.com/RevenueCat/purchases-unity/pull/85
- Bump`purchases-hybrid-common` to `2.0.0` [Changelog here](https://github.com/RevenueCat/purchases-hybrid-common/releases/tag/2.0.0)
- Bump `purchases-ios` to `3.14.0` ([Changelog here](https://github.com/RevenueCat/purchases-ios/releases/3.14.0))
- Bump `purchases-android` to `4.6.0` ([Changelog here](https://github.com/RevenueCat/purchases-android/releases/4.6.0))

## 3.5.0

- Bump`purchases-hybrid-common` to `1.10.0` [Changelog here](https://github.com/RevenueCat/purchases-hybrid-common/releases/tag/1.10.0)
- Bump `purchases-ios` to `3.13.0` ([Changelog here](https://github.com/RevenueCat/purchases-ios/releases/3.13.0))
- Bump `purchases-android` to `4.4.0` ([Changelog here](https://github.com/RevenueCat/purchases-android/releases/4.4.0))
- Added support for Airship integration via `setAirshipChannelID`
    https://github.com/RevenueCat/purchases-unity/pull/71

## 3.4.2

- Bump `purchases-ios` to 3.12.8
    [3.12.8 Changelog here](https://github.com/RevenueCat/purchases-ios/releases/tag/3.12.8)
    [3.12.7 Changelog here](https://github.com/RevenueCat/purchases-ios/releases/tag/3.12.7)
    [3.12.6 Changelog here](https://github.com/RevenueCat/purchases-ios/releases/tag/3.12.6)
    [3.12.5 Changelog here](https://github.com/RevenueCat/purchases-ios/releases/tag/3.12.5)
    [3.12.4 Changelog here](https://github.com/RevenueCat/purchases-ios/releases/tag/3.12.4)
 - Bump `purchases-android` to 4.3.3
    [4.3.3 Changelog here](https://github.com/RevenueCat/purchases-android/releases/tag/4.3.3)
    [4.3.2 Changelog here](https://github.com/RevenueCat/purchases-android/releases/tag/4.3.2)

## 3.4.1

- Bumped purchases-android to 4.3.1 [Changelog here](https://github.com/RevenueCat/purchases-android/releases/4.3.1),
which fixes https://github.com/RevenueCat/purchases-unity/issues/61

## 3.4.0

- Adds iOS promotional offers support
    https://github.com/RevenueCat/purchases-unity/pull/59

## 3.3.0

### Identity V3:

In this version, we’ve redesigned the way that user identification works.
Detailed docs about the new system are available [here](https://docs.revenuecat.com/v3.2/docs/user-ids).

#### New methods
- Introduces `LogIn`, a new way of identifying users, which also returns whether a new user has been registered in the system.
`LogIn` uses a new backend endpoint.
- Introduces `LogOut`, a replacement for `Reset`.

#### Deprecations
- deprecates `CreateAlias` in favor of `LogIn`.
- deprecates `Identify` in favor of `LogIn`.
- deprecates `Reset` in favor of `LogOut`.
- deprecates `SetAllowSharingStoreAccount` in favor of dashboard-side configuration.
    https://github.com/RevenueCat/purchases-unity/pull/48/

#### Other
- Bumped purchases-ios to 3.12.2 [Changelog here](https://github.com/RevenueCat/purchases-ios/releases/3.12.2)
- Bumped purchases-android to 4.3.0 [Changelog here](https://github.com/RevenueCat/purchases-android/releases/4.3.0)
- Updated BillingClient to version [4.0.0](https://developer.android.com/google/play/billing/release-notes#4-0).
     https://github.com/RevenueCat/purchases-android/commit/f6554bbf7376c3fd492f0bc67183a9f35889ae78

## 3.2.0
- Added canMakePaymentsMethod (https://github.com/RevenueCat/purchases-unity/pull/52)
- Fixed missing meta files for Subtester sample app (https://github.com/RevenueCat/purchases-unity/pull/47)
- Bumped purchases-hybrid-common to 1.7.1 [Changelog here](https://github.com/RevenueCat/purchases-hybrid-common/releases/1.7.1)
- Bumped purchases-ios to 3.11.1 [Changelog here](https://github.com/RevenueCat/purchases-ios/releases/3.11.1)
- Bumped purchases-android to 4.2.1 [Changelog here](https://github.com/RevenueCat/purchases-hybrid-common/releases/4.2.1)

## 3.1.1
- Ensured that the `purchases-hybrid-common` version being used by the SDK is locked using `[x.y.z]`
    https://github.com/RevenueCat/purchases-unity/pull/44
- Bumped purchases-hybrid-common to 1.6.2 [Changelog here](https://github.com/RevenueCat/purchases-hybrid-common/releases/1.6.2)
- Bumped purchases-ios to 3.10.7 [Changelog here](https://github.com/RevenueCat/purchases-ios/releases/3.10.7)
- Bumped purchases-android to 4.0.5 [Changelog here](https://github.com/RevenueCat/purchases-hybrid-common/releases/4.0.5)
    https://github.com/RevenueCat/purchases-flutter/pull/171

## 3.1.0
- iOS:
    - Added a new method `setSimulatesAskToBuyInSandbox`, that allows developers to test deferred purchases easily.
- Bumped purchases-hybrid-common to 1.6.1 [Changelog here](https://github.com/RevenueCat/purchases-hybrid-common/releases/1.6.1)
- Bumped purchases-ios to 3.10.6 [Changelog here](https://github.com/RevenueCat/purchases-ios/releases/3.10.6)
- Bumped purchases-android to 4.0.4 [Changelog here](https://github.com/RevenueCat/purchases-hybrid-common/releases/4.0.4)
    https://github.com/RevenueCat/purchases-unity/pull/43

## 3.0.1
- Fixed a crash in iOS when parsing dates in milliseconds, as well as a bug in Android that caused dates that were reported as `milliseconds` to actually have values in seconds.
https://github.com/RevenueCat/purchases-unity/pull/39
- Bumped `purchases-hybrid-common` to 1.5.1 [Changelog here](https://github.com/RevenueCat/purchases-hybrid-common/releases/1.5.1)

## 3.0.0

- removes deprecated `MakePurchase`, replaced by `PurchaseProduct`
- iOS:
    - added new method, `syncPurchases`, that enables syncing the purchases in the local receipt with the backend without risking a password prompt. The method was already available on Android.
    - added a new method, `presentCodeRedemptionSheet`, for offer codes redemption.
    - fixed a bug where values for dates in milliseconds were actually in seconds (https://github.com/RevenueCat/purchases-hybrid-common/pull/62)
- Bumped `purchases-hybrid-common` to 1.5.0 [Changelog here](https://github.com/RevenueCat/purchases-hybrid-common/releases/1.5.0)
- Bumped `purchases-ios` to 3.9.2 [Changelog here](https://github.com/RevenueCat/purchases-ios/releases/3.9.2)
- Bumped `purchases-android` to 4.0.1 [Changelog here](https://github.com/RevenueCat/purchases-android/releases/4.0.1)

## 2.3.1

- Bumped common files to 1.4.5 [Changelog here](https://github.com/RevenueCat/purchases-hybrid-common/releases/tag/1.4.4)
- Bumped iOS to 3.7.5 [Changelog here](https://github.com/RevenueCat/purchases-ios/releases/tag/3.7.5)

## 2.3.0

- Bumped common files to 1.4.4 [Changelog here](https://github.com/RevenueCat/purchases-hybrid-common/releases/tag/1.4.4)
- Bumped iOS to 3.7.2 [Changelog here](https://github.com/RevenueCat/purchases-ios/releases/tag/3.7.2)
- Bumped Android to 3.5.2 [Changelog here](https://github.com/RevenueCat/purchases-android/releases/tag/3.5.2)
- Added a new property `NonSubscriptionTransactions` in `PurchaserInfo` to better manage non-subscriptions
- Attribution V2:
 - Deprecated addAttribution in favor of setAdjustId, setAppsflyerId, setFbAnonymousId, setMparticleId.
 - Added support for OneSignal via setOnesignalId
 - Added setMediaSource, setCampaign, setAdGroup, setAd, setKeyword, setCreative, and collectDeviceIdentifiers

## 2.2.1

- Fixes duplicated files error in iOS.

## 2.2.0

- Adds proxyURL property, useful for kids category apps, so that they can set up a proxy to send requests through. Do not use this unless you've talked to RevenueCat support about it.
- Adds userDefaultsSuiteName. Set this to use a specific NSUserDefaults suite for RevenueCat. This might be handy if you are deleting all NSUserDefaults in your app and leaving RevenueCat in a bad state.
- Update to ExternalDependencyManager 1.2.156
- A lot of internal changes:
  - iOS now downloads the dependency from CocoaPods
  - Android downloads the common code from Maven
  - Removed a bunch of scripts
  - Removed the Android project and added the files to the plugin folder
  - Added platform version and name to the http headers
  - Added a script to create the unitypackage
  - Moved inner classes to their own files
  - Use SimpleJSON instead of JSONUtility


**iOS Native SDK Update 3.4.0**
 - [Release Notes](https://github.com/RevenueCat/purchases-ios/releases/)

**Android Native SDK Update 3.2.0**
 - [Release Notes](https://github.com/RevenueCat/purchases-ios/releases/)

## 2.1.0

- Adds Subscriber Attributes, which allow developers to store additional, structured information
for a user in RevenueCat. More info: https://docs.revenuecat.com/docs/user-attributes.
- Added new method to invalidate the purchaser info cache, useful when promotional purchases are granted from outside the app.

**iOS Native SDK Update 3.2.2**
 - [Release Notes 3.2.2](https://github.com/RevenueCat/purchases-ios/releases/tag/3.2.2)
 - [Release Notes 3.2.1](https://github.com/RevenueCat/purchases-ios/releases/tag/3.2.1)
 - [Release Notes 3.1.2](https://github.com/RevenueCat/purchases-ios/releases/tag/3.1.2)
 - [Release Notes 3.1.1](https://github.com/RevenueCat/purchases-ios/releases/tag/3.1.1)
 - [Release Notes 3.1.0](https://github.com/RevenueCat/purchases-ios/releases/tag/3.1.0)
 - [Release Notes 3.0.4](https://github.com/RevenueCat/purchases-ios/releases/tag/3.0.4)
 - [Release Notes 3.0.3](https://github.com/RevenueCat/purchases-ios/releases/tag/3.0.3)
 - [Release Notes 3.0.2](https://github.com/RevenueCat/purchases-ios/releases/tag/3.0.2)

**Android Native SDK Update 3.1.0**
 - [Release Notes 3.1.0](https://github.com/RevenueCat/purchases-ios/releases/tag/3.1.0)
 - [Release Notes 3.0.7](https://github.com/RevenueCat/purchases-ios/releases/tag/3.0.7)
 - [Release Notes 3.0.6](https://github.com/RevenueCat/purchases-ios/releases/tag/3.0.6)
 - [Release Notes 3.0.5](https://github.com/RevenueCat/purchases-ios/releases/tag/3.0.5)

## 2.0.9

- Fixes updated PurchaserInfo listener in iOS

## 2.0.8

- Fixes active entitlements in iOS and the format of the dates in the EntitlementInfo objects in iOS

## 2.0.7

- Adds optional type parameter to GetProducts call

## 2.0.6

- Adds `CheckTrialOrIntroductoryPriceEligibility`

## 2.0.5

- Fixes Product class
- Removes unnecessary iOS import
- Adds missing iOS files

## 2.0.4

- Updates Android library to 3.0.3

## 2.0.3

- Makes Offerings.Current nullable

## 2.0.2

- Rolls back Play services resolver to v1.2.132

## 2.0.1

- Makes packages inside Offering nullable.

## 2.0.0

- `PurchaserInfo.LatestExpirationDate` can be null now, it was equal to epoch before if there was no expiration date. Same for `PurchaserInfo.AllExpirationDates`, that can contain nullable values now.
- Added Play Services Resolver (https://github.com/googlesamples/unity-jar-resolver) to help with the Android dependencies management. Make sure you import it when importing our package.
- Support for new Offerings system.
- Deprecates `makePurchase` methods. Replaces with `purchasePackage`
- Deprecates `getEntitlements` method. Replaces with `getOfferings`
- See our migration guide for more info: https://docs.revenuecat.com/v3.0/docs/offerings-migration
- Updates to BillingClient 2.0.3. If finishTransactions is set to false (or observerMode is true when configuring the SDK), this SDK won't acknowledge any purchase and you have to do it yourself.
- Adds proration mode support on upgrades/downgrades
- Adds more PurchaserInfo missing properties. `activeEntitlements`, `expirationsForActiveEntitlements` and `purchaseDatesForActiveEntitlements` have been removed from PurchaserInfo. For more info check out https://docs.revenuecat.com/docs/purchaserinfo
- New identity changes:
  - The .createAlias() method is no longer required, use .identify() instead
  - .identify() will create an alias if being called from an anonymous ID generated by RevenueCat
  - Added an isAnonymous property to Purchases.sharedInstance
  - Improved offline use

## 1.2.2

- Updates Android SDK to 3.4.1 to include multiple bugfixes.

## 1.2.1

- Fixes introductory offer period normalization in iOS.

## 1.2.0

- Upgrades iOS SDK to https://github.com/RevenueCat/purchases-ios/releases/tag/2.5.0
- Upgrades Android SDK to https://github.com/RevenueCat/purchases-android/releases/tag/2.3.0.
- Adds Facebook as supported attribution network.
- Deprecates setAutomaticAttributionCollection in favor of setAutomaticAppleSearchAdsAttributionCollection. This is just a change in the function name. Disabled by default.
- Adds introductory pricing to the iOS product.

## 1.1.0

- Updates iOS SDK to 2.3.0. Check out the changelog for a full list of changes https://github.com/RevenueCat/purchases-ios/releases/tag/2.3.0
- Updates Android SDK to 2.2.5. Check out the changelog for a full list of changes https://github.com/RevenueCat/purchases-android/releases/tag/2.2.5
- ** BREAKING CHANGE ** makePurchase parameter oldSKUs is not an array anymore, it only accepts a string now. This is due to changes in the BillingClient.
- AddAttributionData can be called before the SDK has been setup. A network user identifier can be send to the addAttribution function, replacing the previous rc_appsflyer_id parameter.
- Adds an optional configuration boolean observerMode. This will set the value of finishTransactions at configuration time.

### Android only:

- addAttribution will automatically add the rc_gps_adid parameter.
- ** ANDROID BREAKING CHANGE ** Call syncPurchases to send purchases information to RevenueCat after any restore or purchase if you are using the SDK in observerMode. See our guide on Migrating Subscriptions for more information on syncPurchases: https://docs.revenuecat.com/docs/migrating-existing-subscriptions

### iOS only

- addAttribution will automatically add the rc_idfa and rc_idfv parameters if the AdSupport and UIKit frameworks are included, respectively.
- Apple Search Ad attribution can be automatically collected by setting the automaticAttributionCollection boolean to true before the SDK is configured.

## 1.0.2

- Updates iOS SDK to 2.1.1

## 1.0.1

- Fixing crash on iOS when missing an underlying error

## 1.0.0

- Updates SDKs to 2.1.0. This means there is new functions added:
- Changes the SDK to use callback functions instead of delegates. There is a UpdatedPurchaserInfoListener that sends a purchaser info object. This listener is used to listen to changes in the purchaser info.
- Added setDebugLogsEnabled to display debug logs.
- Added getPurchaserInfo function to get the latest purchaser info known by the SDK.
- Added getEntitlements
- Added getAppUserId

## 0.6.1

- Adds setFinishTransactions for iOS
- Adds more attribution networks

## 0.6.0

- Updates iOS SDK to 1.20 and Android SDK to 1.4.0.
- Adds identify, create alias and reset call

## 0.5.4

- Fixes onRestoreTransactions never being called.

## 0.5.3

- Fixes onRestoreTransactions not being called if there are no tokens.

## 0.5.2

- Fixes crash due to not able to find Kotlin dependency.

## 0.5.1

- Adds requestDate to the purchaser info to avoid edge cases.

## 0.5.0

- Enhance the PurchasesListener protocol to include methods for restore succeeded and failed.

## 0.4.2

- Add Android support for Adjust

## 0.4.1

- Add support for idfa data in Adjust
