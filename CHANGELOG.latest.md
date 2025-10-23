> [!WARNING]  
> If you don't have any login system in your app, please make sure your one-time purchase products have been correctly configured in the RevenueCat dashboard as either consumable or non-consumable. If they're incorrectly configured as consumables, RevenueCat will consume these purchases. This means that users won't be able to restore them from version 8.0.0 onward.
> Non-consumables are products that are meant to be bought only once, for example, lifetime subscriptions.


## RevenueCat SDK
### 📦 Dependency Updates
* [AUTOMATIC BUMP] Updates purchases-hybrid-common to 17.11.0 (#727) via RevenueCat Git Bot (@RCGitBot)
  * [Android 9.11.0](https://github.com/RevenueCat/purchases-android/releases/tag/9.11.0)
  * [Android 9.10.0](https://github.com/RevenueCat/purchases-android/releases/tag/9.10.0)
  * [iOS 5.44.1](https://github.com/RevenueCat/purchases-ios/releases/tag/5.44.1)
  * [iOS 5.44.0](https://github.com/RevenueCat/purchases-ios/releases/tag/5.44.0)

## RevenueCatUI SDK
RevenueCatUI adds Paywalls and Customer Center to the RevenueCat Unity SDK. Paywalls and Customer Center can be configured in the RevenueCat dashboard and presented on iOS and Android with one line of code.

### ✨ New Features
* **Paywalls**: Present paywalls configured in the RevenueCat dashboard using the `PaywallsPresenter` API or `PaywallsBehaviour` MonoBehaviour component
  - Use `await PaywallsPresenter.Present()` to show a paywall
  - Use `await PaywallsPresenter.PresentIfNeeded(requiredEntitlementIdentifier)` to conditionally present based on entitlement status
  - Configure paywalls through Unity's Inspector with the `PaywallsBehaviour` component
  - Supports both original template paywalls and Paywalls V2
  - Available on iOS and Android device builds (Unity Editor not supported for UI)
* **Customer Center**: Provide a self-service interface for users to manage their subscriptions
  - Use `await CustomerCenterPresenter.Present()` to show the Customer Center
  - Allows users to view subscription details, manage billing, and access support
  - Fully customizable through the RevenueCat dashboard
  - Available on iOS and Android device builds

### 📦 Installation
1. Install the core RevenueCat Unity SDK (if not already installed)
2. Install the new PurchasesUI SDK
3. Configure the SDK as normal, then call `PaywallsPresenter.Present()` or `CustomerCenterPresenter.Present()` from any script

For more details, see the [Paywalls documentation](https://www.revenuecat.com/docs/tools/paywalls/installation) and [Customer Center documentation](https://www.revenuecat.com/docs/tools/customer-center/customer-center-unity#installation)

### 🔄 Other Changes
* Add importing PurchasesUI to IntegrationTests (#730) via Cesar de la Vega (@vegaro)
* Bump fastlane-plugin-revenuecat_internal from `25c7fb8` to `525d48c` (#725) via dependabot[bot] (@dependabot[bot])
