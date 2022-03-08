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
