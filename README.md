<p align="center">
  <img src="https://uploads-ssl.webflow.com/5e2613cf294dc30503dcefb7/5e752025f8c3a31d56a51408_logo_red%20(1).svg" width="350" alt="RevenueCat"/>
<br>
Unity in-app subscriptions made easy
</p>

[![openupm](https://img.shields.io/npm/v/com.revenuecat.purchases-unity?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.revenuecat.purchases-unity/)

# What is purchases-unity?

Purchases Unity is a client for the [RevenueCat](https://www.revenuecat.com/) subscription and purchase tracking system. It is an open source framework that provides a wrapper around `StoreKit`, `Google Play Billing` and the RevenueCat backend to make implementing in-app purchases in `Unity` easy.

## Features
|   | RevenueCat |
| --- | --- |
✅ | Server-side receipt validation
➡️ | [Webhooks](https://docs.revenuecat.com/docs/webhooks) - enhanced server-to-server communication with events for purchases, renewals, cancellations, and more   
🎯 | Subscription status tracking - know whether a user is subscribed whether they're on iOS or Android
📊 | Analytics - automatic calculation of metrics like conversion, mrr, and churn  
📝 | [Online documentation](https://docs.revenuecat.com/docs) up to date  
🔀 | [Integrations](https://www.revenuecat.com/integrations) - over a dozen integrations to easily send purchase data where you need it  
💯 | Well maintained - [frequent releases](https://github.com/RevenueCat/purchases-unity/releases)  
📮 | Great support - [Help Center](https://revenuecat.zendesk.com) 

## Getting Started
For more detailed information, you can view our complete documentation at [docs.revenuecat.com](https://docs.revenuecat.com/docs/unity).

## Dependencies and Unity IAP
We use StoreKit for iOS and BillingClient for Android. This plugin also depends on [purchases-ios](https://github.com/RevenueCat/purchases-ios), [purchases-android](https://github.com/RevenueCat/purchases-android) and [purchases-hybrid-common](https://github.com/RevenueCat/purchases-hybrid-common). 

[VERSIONS.md](https://github.com/RevenueCat/purchases-unity/blob/main/VERSIONS.md) contains the dependencies versions for each release.

If using this plugin alongside Unity IAP, please check the specific instructions in [our observer mode docs](https://docs.revenuecat.com/docs/unity#installation-with-unity-iap-side-by-side).

## Native dependency management

The native iOS and Android libraries are declared in `*Dependencies.xml` files and resolved at build time by an External Dependency Manager. Starting with v10, the plugin uses Unity's official [External Dependency Manager](https://docs.unity3d.com/Packages/com.unity.external-dependency-manager@2.1/manual/index.html) (`com.unity.external-dependency-manager`), which requires Unity 2022.3 or newer.

- **Unity Package Manager / OpenUPM**: Unity's External Dependency Manager is a dependency of the package and is installed automatically.
- **`.unitypackage`**: on first import, if the project has no dependency manager, the plugin installs Unity's External Dependency Manager through the Package Manager. This needs an interactive Editor session with access to the Unity registry. You can also install it beforehand from `Window > Package Manager > Unity Registry`.
- **Projects already using Google's EDM4U**: nothing changes. Google's [EDM4U](https://github.com/googlesamples/unity-jar-resolver) keeps working with this plugin, but it is deprecated and no longer maintained by Google. To migrate, install Unity's External Dependency Manager, choose it when prompted, and remove EDM4U once no other package depends on it. You can switch at any time from `Assets > External Dependency Manager`.

Keep a single dependency manager active in a project. In particular, projects built in batch mode (CI) must declare exactly one, since the selection prompt cannot be answered there.
