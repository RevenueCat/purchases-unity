# RevenueCat UI for Unity (Beta)

RevenueCat UI adds native paywall presentation to the RevenueCat Unity SDK. It lets you present a paywall configured in the RevenueCat dashboard on iOS and Android with one line of code.

## Requirements

- RevenueCat Unity SDK installed in your project
- Unity Editor is not supported for displaying the paywall UI

## Installation

1. Install the core RevenueCat Unity SDK (if not already installed) following the official [docs](https://www.revenuecat.com/docs/getting-started/installation/unity).
2. In Unity, import the downloaded `RevenueCatUI.unitypackage`.

## Setup

Configure the core RevenueCat SDK as you normally would:

- Add a `Purchases` component to a GameObject and set your API keys in the Inspector; or
- Configure programmatically early in app startup using `PurchasesConfiguration` with `useRuntimeSetup` enabled on your `Purchases` component.

## Presenting a paywall

Use the static API from any script. The call returns a `Task<RevenueCatUI.PaywallResult>` you can `await`.

```csharp
using System.Threading.Tasks;
using RevenueCatUI;

public class ShowPaywallExample
{
    public async Task Show()
    {
        var result = await PaywallsPresenter.Present();

        switch (result.Result)
        {
            case PaywallResultType.Purchased:
                break;
            case PaywallResultType.Restored:
                break;
            case PaywallResultType.NotPresented:
                break;
            case PaywallResultType.Cancelled:
                break;
            case PaywallResultType.Error:
                break;
        }
    }
}
```

Alternatively, present the paywall only if the user does not have a required entitlement:

```csharp
using System.Threading.Tasks;
using RevenueCatUI;

public class ConditionalPaywallExample
{
    public async Task ShowIfNeeded()
    {
        var result = await PaywallsPresenter.PresentIfNeeded(
            requiredEntitlementIdentifier: "pro"
        );
    }
}
```

## Using PaywallsBehaviour Component

For developers who prefer configuring paywalls through Unity's Inspector, you can use the `PaywallsBehaviour` MonoBehaviour component:

1. Add a `PaywallsBehaviour` component to any GameObject
2. Configure the options in the Inspector:
   - **Offering Identifier**: Leave empty to use current offering, or specify an offering ID
   - **Display Close Button**: Whether to show a close button (original templates only)
   - **Required Entitlement Identifier**: Optional - paywall will only show if user lacks this entitlement
3. Wire up UnityEvent callbacks: `OnPurchased`, `OnRestored`, `OnCancelled`, `OnNotPresented`, `OnError`
4. Call `PresentPaywall()` method (e.g., from a UI Button's OnClick event)

This provides an alternative to the code-based `PaywallsPresenter` API for Unity Editor workflows.

## Configuring PaywallOptions

`PaywallOptions` allows you to customize how the paywall is presented. There are three ways to create it:

### Present the current offering
```csharp
new PaywallOptions()
```

### Present a specific offering by identifier
```csharp
new PaywallOptions("my_offering_id")
```

### Present from an Offering object
```csharp
var offerings = await Purchases.GetOfferings();
var offering = offerings.Current;
new PaywallOptions(offering)
```

### Display Close Button

You can optionally display a close button by passing `displayCloseButton: true` to any constructor:

```csharp
new PaywallOptions(displayCloseButton: true)
new PaywallOptions("my_offering_id", displayCloseButton: true)
new PaywallOptions(offering, displayCloseButton: true)
```

**Note:** The `displayCloseButton` parameter only applies to original template paywalls and is ignored for Paywalls V2.

## API reference

### PaywallsPresenter Methods
- `PaywallsPresenter.Present(PaywallOptions options = null)` → `Task<PaywallResult>`
  - Presents a paywall configured in the RevenueCat dashboard
  - If `options` is null, presents the current offering without a close button
  
- `PaywallsPresenter.PresentIfNeeded(string requiredEntitlementIdentifier, PaywallOptions options = null)` → `Task<PaywallResult>`
  - Presents a paywall only if the user doesn't have the specified entitlement
  - Returns `PaywallResultType.NotPresented` if the user already has the entitlement

### PaywallResult Values

The `PaywallResult.Result` property can have the following values:

- `Purchased` - User completed a purchase
- `Restored` - User restored their purchases
- `Cancelled` - User dismissed the paywall without making a purchase
- `NotPresented` - Paywall was not shown (user already has entitlement when using `PresentIfNeeded`)
- `Error` - An error occurred

## Platform notes

- The paywall UI is only available on iOS and Android device builds.

## Troubleshooting

- Build to device when testing the paywall UI.
- Ensure you have an active Offering and Paywall configured in the RevenueCat dashboard and that product identifiers are correctly set up in the stores.

## Links

- Documentation: [RevenueCat docs](https://www.revenuecat.com/docs)
- Changelog: `CHANGELOG.md`
- License: `LICENSE`
