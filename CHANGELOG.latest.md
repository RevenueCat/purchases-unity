> [!WARNING]  
> If you don't have any login system in your app, please make sure your one-time purchase products have been correctly configured in the RevenueCat dashboard as either consumable or non-consumable. If they're incorrectly configured as consumables, RevenueCat will consume these purchases. This means that users won't be able to restore them from version 8.0.0 onward.
> Non-consumables are products that are meant to be bought only once, for example, lifetime subscriptions.


## RevenueCat SDK
### 🐞 Bugfixes
* **BEHAVIOR CHANGE** Change wrong default of `autoSyncPurchases` to `true` on runtime setups (#693) via Cesar de la Vega (@vegaro)
### 📦 Dependency Updates
* [AUTOMATIC BUMP] Updates purchases-hybrid-common to 17.8.0 (#678) via RevenueCat Git Bot (@RCGitBot)
  * [Android 9.7.2](https://github.com/RevenueCat/purchases-android/releases/tag/9.7.2)
  * [Android 9.7.1](https://github.com/RevenueCat/purchases-android/releases/tag/9.7.1)
  * [iOS 5.40.0](https://github.com/RevenueCat/purchases-ios/releases/tag/5.40.0)
  * [iOS 5.39.3](https://github.com/RevenueCat/purchases-ios/releases/tag/5.39.3)
  * [iOS 5.39.2](https://github.com/RevenueCat/purchases-ios/releases/tag/5.39.2)
  * [iOS 5.39.1](https://github.com/RevenueCat/purchases-ios/releases/tag/5.39.1)

### 🔄 Other Changes
* Update EDM4U in Subtester (#690) via Cesar de la Vega (@vegaro)
* Update baseProjectTemplate.gradle (#691) via Cesar de la Vega (@vegaro)
* Update changelog for release v7.8.0 (#687) via Antonio Pallares (@ajpallares)
* Add CODEOWNERS file (#688) via Antonio Pallares (@ajpallares)
* Bump fastlane-plugin-revenuecat_internal from `1593f78` to `7508f17` (#689) via dependabot[bot] (@dependabot[bot])
* Bump fastlane-plugin-revenuecat_internal from `e1c0e04` to `1593f78` (#683) via dependabot[bot] (@dependabot[bot])
* Update CircleCI orb (#680) via Cesar de la Vega (@vegaro)
* Bump fastlane-plugin-revenuecat_internal from `401d148` to `e1c0e04` (#679) via dependabot[bot] (@dependabot[bot])
* Bump fastlane-plugin-revenuecat_internal from `a6dc551` to `401d148` (#676) via dependabot[bot] (@dependabot[bot])
