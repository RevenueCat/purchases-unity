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
        var result = await PaywallsPresenter.Present(new PaywallOptions
        {
            OfferingIdentifier = "default",
            DisplayCloseButton = true
        });

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
            requiredEntitlementIdentifier: "pro",
            options: new PaywallOptions { OfferingIdentifier = "default" }
        );
    }
}
```

## MonoBehaviour helper (for scene-driven UI)

Use `RevenueCatUI/PaywallsBehaviour` directly from your scene and wire it to UI events.

Steps:

1. Add `PaywallsBehaviour` to a GameObject in your scene.
2. Add your own script (e.g., `SubscribeButton`) and reference the `PaywallsBehaviour` via the Inspector.
3. Wire your script's handler to a `Button.onClick`.

Awaiting the result in your handler:

```csharp
using System.Threading.Tasks;
using RevenueCatUI;
using UnityEngine;
using UnityEngine.UI;

public class SubscribeButton : MonoBehaviour
{
    [SerializeField] private PaywallsBehaviour paywalls;
    [SerializeField] private Button button;

    private void Awake()
    {
        if (button != null)
            button.interactable = paywalls != null && paywalls.IsSupported();
    }

    public async void OnClick()
    {
        var result = await paywalls.PresentPaywall(new PaywallOptions("default", true));
    }
}
```

Fire-and-forget (no await), if you don't need the result immediately:

```csharp
using RevenueCatUI;
using UnityEngine;

public class SubscribeButton : MonoBehaviour
{
    [SerializeField] private PaywallsBehaviour paywalls;

    public void OnClick()
    {
        _ = paywalls.PresentPaywall();
    }
}
```

## API reference

- `RevenueCatUI.PaywallsPresenter.IsSupported()` → `bool`
- `RevenueCatUI.PaywallsPresenter.Present(PaywallOptions options = null)` → `Task<PaywallResult>`
- `RevenueCatUI.PaywallsPresenter.PresentIfNeeded(string requiredEntitlementIdentifier, PaywallOptions options = null)` → `Task<PaywallResult>`
- `RevenueCatUI.PaywallsBehaviour` MonoBehaviour helper with:
  - `PresentPaywall(PaywallOptions options = null)` → `Task<PaywallResult>`
  - `PresentPaywallIfNeeded(string requiredEntitlementIdentifier, PaywallOptions options = null)` → `Task<PaywallResult>`
  - `IsSupported()` → `bool`

`PaywallOptions`:

- `OfferingIdentifier` (string, optional): offering to present. If omitted, the current offering is used.
- `DisplayCloseButton` (bool): only applies to original template paywalls. Ignored for Paywalls V2.

`PaywallResult.Result` values:

- `Purchased`
- `Restored`
- `Cancelled`
- `NotPresented`
- `Error`

## Platform notes

- Use `IsSupported()` before presenting to avoid calls on unsupported platforms.
- The paywall UI is only available on iOS and Android device builds. Editor, desktop, and WebGL return `false` for `IsSupported()`.

## Troubleshooting

- Build to device when testing the paywall UI.
- Ensure you have an active Offering and Paywall configured in the RevenueCat dashboard and that product identifiers are correctly set up in the stores.

## Links

- Documentation: [RevenueCat docs](https://www.revenuecat.com/docs)
- Changelog: `CHANGELOG.md`
- License: `LICENSE`

