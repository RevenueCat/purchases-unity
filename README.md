# purchases-unity

RevenueCat's Unity SDK wraps StoreKit and Google Play Billing to handle purchases and subscription status. The UI package adds Paywalls and Customer Center.

[![OpenUPM version](https://img.shields.io/npm/v/com.revenuecat.purchases-unity?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.revenuecat.purchases-unity/) [![License: MIT](https://img.shields.io/github/license/RevenueCat/purchases-unity)](https://github.com/RevenueCat/purchases-unity/blob/main/LICENSE)

[Docs](https://www.revenuecat.com/docs?utm_source=github&utm_medium=sdk_readme&utm_content=purchases-unity) · [Install guide](https://www.revenuecat.com/docs/getting-started/installation/unity?utm_source=github&utm_medium=sdk_readme&utm_content=purchases-unity) · [Sample app](https://github.com/RevenueCat/purchases-unity/tree/main/Subtester) · [Changelog](https://github.com/RevenueCat/purchases-unity/releases) · [Migration guide](https://github.com/RevenueCat/purchases-unity/blob/main/migrations/v11-MIGRATION.md)

## Requirements

| Requirement | Minimum |
|---|---|
| Unity | 2022.3.33f1 |
| iOS | 13.0 |
| Android Target API Level | 35 |

Paywalls and Customer Center need iOS 15 or later and Android 7.0 (API level 24) or later. The SDK doesn't run in the Unity Editor, so build and run your app on a device or simulator to test it.

## Installation

Install the core package from OpenUPM with the [OpenUPM CLI](https://openupm.com/docs/getting-started.html):

```shell
openupm add com.revenuecat.purchases-unity
```

If you show Paywalls or Customer Center, add the UI package next: `openupm add com.revenuecat.purchases-ui-unity`.

- Without the CLI: in **Project Settings > Package Manager**, add a scoped registry with URL `https://package.openupm.com` and scopes `com.openupm`, `com.revenuecat.purchases-unity`, and (for the UI package) `com.revenuecat.purchases-ui-unity`, then install from **My Registries**. See the [install guide](https://www.revenuecat.com/docs/getting-started/installation/unity?utm_source=github&utm_medium=sdk_readme&utm_content=purchases-unity#option-1-recommended-install-using-openupm).
- `.unitypackage`: import [`Purchases.unitypackage`](https://github.com/RevenueCat/purchases-unity/releases/latest/download/Purchases.unitypackage) from the latest release, plus [`PurchasesUI.unitypackage`](https://github.com/RevenueCat/purchases-unity/releases/latest/download/PurchasesUI.unitypackage) for Paywalls or Customer Center.

RevenueCat uses a dependency manager for its native iOS and Android libraries.

- OpenUPM installs Unity’s [External Dependency Manager](https://docs.unity3d.com/Packages/com.unity.external-dependency-manager@2.1/manual/index.html) automatically.
- If installing via `.unitypackage`, Unity’s manager will be installed on first import if you open the project in the Unity Editor and the project has no dependency manager. If CI performs the first import in batch mode, add the manager to your project beforehand from Window > Package Manager > Unity Registry.

If your project already uses Google’s EDM4U, you can keep using it. EDM4U is deprecated. To switch to Unity’s manager and avoid having both installed, follow the [v10 migration guide](https://github.com/RevenueCat/purchases-unity/blob/main/migrations/v10-MIGRATION.md#unitys-external-dependency-manager-replaces-edm4u).

Starting with version 11, RevenueCat's iOS dependencies are no longer published to CocoaPods, so the External Dependency Manager must add them with Swift Package Manager. Check that **Swift Package Manager Integration** is enabled in the iOS Resolver settings: **Edit > Project Settings > External Dependency Manager > iOS Resolver** with Unity's External Dependency Manager (2.1 or later), or **Assets > External Dependency Manager > iOS Resolver > Settings** with EDM4U (1.2.189 or later). Other CocoaPods dependencies in your project keep working. If your project installs RevenueCat through CocoaPods, [migrate to Swift Package Manager](https://www.revenuecat.com/docs/sdk-guides/unity-cocoapods-to-spm-migration?utm_source=github&utm_medium=sdk_readme&utm_content=purchases-unity) before you upgrade to version 11.

On Android, set **Player Settings > Other Settings > Target API Level** to 35 or higher. Lower values fail the Gradle build.

| Package | What it's for | When you need it |
|---|---|---|
| `com.revenuecat.purchases-unity` | Purchases, entitlements, subscription status | Always |
| `com.revenuecat.purchases-ui-unity` | Paywalls and Customer Center | You show RevenueCat Paywalls or Customer Center |
| `com.revenuecat.purchases-admob-unity` | Tracks AdMob ad events in RevenueCat | You show AdMob ads. Import [`PurchasesAdMob.unitypackage`](https://github.com/RevenueCat/purchases-unity/releases/latest/download/PurchasesAdMob.unitypackage) (not on OpenUPM). Needs the Google Mobile Ads Unity plugin and iOS 15 or later |

## Quick start

Before you start, create a RevenueCat project, connect your store or start with the Test Store, and add an entitlement. See the [Quickstart](https://www.revenuecat.com/docs/getting-started/quickstart?utm_source=github&utm_medium=sdk_readme&utm_content=purchases-unity).

Add the `Purchases` component to one GameObject and keep it as a singleton. Enter your public API keys in the Inspector (the Test Store key while you test). The component configures the SDK when it starts.

To configure from code, check **Use Runtime Setup** on the `Purchases` component. Attach this `PurchasesSetup.cs` script to the same GameObject and replace the placeholder with the public API key for the store you're testing. The `DefaultExecutionOrder` attribute makes this script's `Start()` run after `Purchases.Start()` initializes the SDK wrapper.

```csharp
using UnityEngine;

[DefaultExecutionOrder(1000)]
public class PurchasesSetup : MonoBehaviour
{
    private void Start()
    {
        var builder = Purchases.PurchasesConfiguration.Builder.Init(<public_api_key>);
        builder.SetAppUserId(<app_user_id>);
        GetComponent<Purchases>().Configure(builder.Build());
    }
}
```

Replace `<app_user_id>` with the signed-in user’s ID from your account system. For anonymous users, omit the `SetAppUserId` line and RevenueCat will generate an ID.

After configuration, check whether the user has an active entitlement before showing a paywall. Replace `<my_entitlement_identifier>` with your entitlement identifier.

```csharp
var purchases = GetComponent<Purchases>();
purchases.GetCustomerInfo((customerInfo, error) =>
{
  if (customerInfo.Entitlements.Active.ContainsKey(<my_entitlement_identifier>)) {
    // Unlock that great "pro" content
  }
});
```

## What's included

- [Paywalls](https://www.revenuecat.com/docs/tools/paywalls?utm_source=github&utm_medium=sdk_readme&utm_content=purchases-unity): build single-screen or multi-step paywalls in the dashboard and change them without an app update. Needs the UI package.
- [Customer Center](https://www.revenuecat.com/docs/tools/customer-center?utm_source=github&utm_medium=sdk_readme&utm_content=purchases-unity): a self-service screen where users cancel subscriptions, restore purchases, and, on iOS, request refunds or change plans. Needs the UI package.
- [Placements](https://www.revenuecat.com/docs/tools/targeting/placements?utm_source=github&utm_medium=sdk_readme&utm_content=purchases-unity): serve a different Offering at each paywall location in your app.
- [Test Store](https://www.revenuecat.com/docs/test-and-launch/sandbox/test-store?utm_source=github&utm_medium=sdk_readme&utm_content=purchases-unity): test purchases before you set up App Store Connect or Google Play Console.
- [In-app currency](https://www.revenuecat.com/docs/offerings/in-app-currency?utm_source=github&utm_medium=sdk_readme&utm_content=purchases-unity): read the current Customer's in-app currency balances.
- [Redemption Links](https://www.revenuecat.com/docs/web/redemption-links?utm_source=github&utm_medium=sdk_readme&utm_content=purchases-unity): unlock a web purchase in your app from a deep link.
- [iOS subscription offers](https://www.revenuecat.com/docs/subscription-guidance/subscription-offers/ios-subscription-offers?utm_source=github&utm_medium=sdk_readme&utm_content=purchases-unity): promotional offers, the offer code redemption sheet, and win-back offers.
- [Your own purchase code](https://www.revenuecat.com/docs/migrating-to-revenuecat/sdk-or-not/finishing-transactions?utm_source=github&utm_medium=sdk_readme&utm_content=purchases-unity): set `purchasesAreCompletedBy` to `MyApp` and `storeKitVersion` to the StoreKit version your code uses, to keep completing purchases with your own code. To run RevenueCat alongside Unity IAP 5 or later, see [Unity IAP side by side](https://www.revenuecat.com/docs/getting-started/installation/unity?utm_source=github&utm_medium=sdk_readme&utm_content=purchases-unity#installation-with-unity-iap-side-by-side).
- [Offline entitlements](https://www.revenuecat.com/docs/customers/customer-info?utm_source=github&utm_medium=sdk_readme&utm_content=purchases-unity#offline-entitlements): the SDK grants entitlements for new subscription purchases while RevenueCat's servers are down. It's unavailable when your app completes purchases itself.
- [Trusted Entitlements](https://www.revenuecat.com/docs/customers/trusted-entitlements?utm_source=github&utm_medium=sdk_readme&utm_content=purchases-unity): the SDK checks a signature on entitlement data and reports the result, so your app can detect tampered responses. Verification is on by default in informational mode and doesn't block access on its own.
- [Amazon Appstore](https://www.revenuecat.com/docs/platform-resources/amazon-platform-resources?utm_source=github&utm_medium=sdk_readme&utm_content=purchases-unity): set **Use Amazon** and your Amazon API key on the `Purchases` component. Unity IAP 5 dropped Amazon support, so let RevenueCat complete Amazon purchases.
- [Ad Monetization](https://www.revenuecat.com/docs/ad-monetization?utm_source=github&utm_medium=sdk_readme&utm_content=purchases-unity): report ad impressions, clicks, and revenue next to subscription data (iOS 15 or later). The [AdMob adapter](https://www.revenuecat.com/docs/ad-monetization/admob?utm_source=github&utm_medium=sdk_readme&utm_content=purchases-unity) tracks AdMob events automatically.

## Documentation

- [Install guide](https://www.revenuecat.com/docs/getting-started/installation/unity?utm_source=github&utm_medium=sdk_readme&utm_content=purchases-unity): OpenUPM and `.unitypackage` steps, Android `launchMode` and ProGuard rules, and running alongside Unity IAP.
- [Configure the SDK](https://www.revenuecat.com/docs/getting-started/configuring-sdk?utm_source=github&utm_medium=sdk_readme&utm_content=purchases-unity): API keys, debug logs, and switching between Test Store and real stores.
- [Displaying paywalls](https://www.revenuecat.com/docs/tools/paywalls/displaying-paywalls?utm_source=github&utm_medium=sdk_readme&utm_content=purchases-unity#unity): present a paywall with `PaywallsPresenter` or the `PaywallsBehaviour` component.
- [Customer Center on Unity](https://www.revenuecat.com/docs/tools/customer-center/customer-center-unity?utm_source=github&utm_medium=sdk_readme&utm_content=purchases-unity): install the UI package and present Customer Center.
- [Testing purchases](https://www.revenuecat.com/docs/test-and-launch/sandbox?utm_source=github&utm_medium=sdk_readme&utm_content=purchases-unity): Test Store and sandbox testing.
- [Migration guide](https://github.com/RevenueCat/purchases-unity/blob/main/migrations/v11-MIGRATION.md): upgrading to version 11 (Swift Package Manager required on iOS). If you're upgrading from version 9, start with the [version 10 guide](https://github.com/RevenueCat/purchases-unity/blob/main/migrations/v10-MIGRATION.md) (Target API Level 35, Unity's External Dependency Manager, API changes).

## Using an AI coding agent

Your agent's training data is likely older than the current version of this SDK. Point it at current sources:

- [llms.txt](https://www.revenuecat.com/docs/llms.txt?utm_source=github&utm_medium=sdk_readme&utm_content=purchases-unity): an index of the docs. Add `.md` to any docs URL for a Markdown version.
- [RevenueCat MCP server](https://www.revenuecat.com/docs/tools/mcp?utm_source=github&utm_medium=sdk_readme&utm_content=purchases-unity): lets your agent read and change your RevenueCat project configuration.
- [AI Toolkit](https://github.com/RevenueCat/ai-toolkit): installs RevenueCat skills and the MCP server in one step.
- [Releases](https://github.com/RevenueCat/purchases-unity/releases): check the latest version before adding the dependency.

## Sample apps

- [Subtester](https://github.com/RevenueCat/purchases-unity/tree/main/Subtester): a Unity 6 test app with a screen for each SDK area, including login, Offerings and placements, purchases, `CustomerInfo`, Paywalls, Customer attributes, and AdMob ads. It loads the packages from this repo's folders, so open it from a clone.

## Getting help

- Bugs: open an issue in [this repo](https://github.com/RevenueCat/purchases-unity/issues).
- Account or configuration questions: [RevenueCat Support](https://www.revenuecat.com/docs/revenuecat-support/support-first-steps?utm_source=github&utm_medium=sdk_readme&utm_content=purchases-unity).
- Discussion: [RevenueCat Community](https://community.revenuecat.com/).

## Contributing

This repository is published from the `main` branch and release tags of RevenueCat's internal development repository. Issues are tracked here. Pull requests are welcome: a maintainer reviews them here and brings accepted changes into the internal repository, so they can't be merged directly.

See the [contributing guide](https://github.com/RevenueCat/purchases-unity/blob/main/CONTRIBUTING.md).

## License

MIT. See [LICENSE](https://github.com/RevenueCat/purchases-unity/blob/main/LICENSE).
