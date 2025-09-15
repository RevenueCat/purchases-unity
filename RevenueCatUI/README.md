# RevenueCat UI for Unity (Stub Package)

This package provides the Unity API surface for RevenueCat UI (paywalls and customer center),
implemented with lightweight stubs so you can wire your flows without native dependencies.

Use this to integrate and test control flow in Editor and on devices. Device builds route
through minimal native stubs; Editor runs C# fallbacks. When ready, replace the minimal
native code with your real implementations.

## Installation

Add to your Unity project's `Packages/manifest.json`:

```
{
  "dependencies": {
    "com.revenuecat.purchases-unity": "file:../RevenueCat",       // existing core SDK
    "com.revenuecat.purchases-ui-unity": "file:../RevenueCatUI"   // this UI package (stub)
  }
}
```

### Requirements

- Unity 2022.3 LTS or later
- RevenueCat Unity SDK (this package depends on the main RevenueCat package)

### Dependencies

By default this package runs in stub mode. On iOS/Android device builds, the
API routes through minimal native stubs that immediately return results (no
external dependencies). In the Editor and other platforms, C# fallbacks return
the same immediate results. When you’re ready for real UI, replace the minimal
native code with your implementation and add dependencies as needed.

## Quick Start

```csharp
using RevenueCat.UI;

public class MySubscriptionManager : MonoBehaviour 
{
    public async void ShowPaywall()
    {
        if (!RevenueCatUI.IsSupported())
        {
            Debug.LogWarning("RevenueCat UI not supported on this platform");
            return;
        }

        try 
        {
            var result = await RevenueCatUI.PresentPaywall();
            
            switch (result.Result) 
            {
                case PaywallResultType.Purchased:
                    Debug.Log("User made a purchase!");
                    break;
                case PaywallResultType.Cancelled:
                    Debug.Log("User cancelled");
                    break;
            }
        }
        catch (System.Exception e) 
        {
            Debug.LogError($"Paywall error: {e.Message}");
        }
    }

    public async void ShowCustomerCenter()
    {
        if (RevenueCatUI.IsSupported())
        {
            await RevenueCatUI.PresentCustomerCenter();
        }
    }
}
```

## API Reference

### RevenueCatUI (Main API)

Implemented now (stubbed behavior everywhere):
- `PresentPaywall()` — returns `Cancelled` immediately
- `PresentPaywall(PaywallOptions)` — same as above with options
- `PresentPaywallIfNeeded(string[, PaywallOptions])` — returns `NotNeeded` immediately
- `PresentCustomerCenter()` — completes immediately
- `IsSupported()` — returns `true` on iOS/Android device builds when both Paywall and Customer Center are available; returns `false` on other platforms (Editor, Windows, macOS, WebGL, etc.)

### PaywallOptions

```csharp
var options = new PaywallOptions
{
    OfferingIdentifier = "premium",      // Optional: specific offering
    DisplayCloseButton = true            // Show close button (iOS only)
};
```

### PaywallResult

```csharp
public enum PaywallResultType 
{
    NotPresented,  // User already has entitlement
    Cancelled,     // User cancelled
    Error,         // An error occurred
    Purchased,     // User made a purchase
    Restored       // User restored purchases
}
```

## Architecture

### Native Implementation (reference only)
Minimal native code is provided as a reference scaffold:
- Android class: `com.revenuecat.unity.RevenueCatUI`
  - Uses pure-code callbacks via a Java interface registered from C# (no GameObject required)
- iOS functions: `rcui_presentPaywall`, `rcui_presentPaywallIfNeeded`, `rcui_presentCustomerCenter`, `rcui_isSupported`

If/when you add real native UI, update the platform presenters in
`RevenueCatUI/Scripts/Platforms/{Android,iOS}` to select your implementations.

### Platform Abstraction
- Factory pattern with `IPaywallPresenter` and `ICustomerCenterPresenter`
- Graceful fallbacks for unsupported platforms
- Unity-style async/await throughout

## Platform Support

- Editor / Any platform: C# fallbacks return immediate results (no UI)
- iOS/Android device: Minimal native stubs return immediate results (no UI)

## Examples

Examples are not included in this stub package. Use the Quick Start snippet above
or your own scene code to exercise the API.

## Troubleshooting (stub mode)

- Call `RevenueCatUI.IsSupported()` before presenting UI
- Initialize the core RevenueCat SDK before using UI components
- Check Unity console for logs tagged `[RevenueCatUI]`

## Modes

Single mode: stub behavior on all platforms (native stubs on devices, C# fallbacks in Editor).

## License

This package follows the same license as the main RevenueCat Unity SDK. 
