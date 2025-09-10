# RevenueCat UI for Unity

Unity UI components (paywalls and customer center) for RevenueCat's subscription management.

## Features

- 🎨 **Native Paywalls**: Present beautiful, native paywalls
- 🏪 **Customer Center**: Manage subscriptions and customer support
- 🔄 **Unity Integration**: Clean, async/await APIs following Unity conventions
- 📱 **Mobile-First**: iOS and Android support
- 🎯 **Production Ready**: Built on proven purchases-hybrid-common-ui

## Installation

### Option 1: Git URL (Development Branch)

Since this package is currently on the `ui-test_v1` branch, install via:

```
https://github.com/RevenueCat/purchases-unity.git?path=RevenueCatUI#ui-test_v1
```

### Option 2: Git URL (After Merge to Main)

Once merged to main, you can use:

```
https://github.com/RevenueCat/purchases-unity.git?path=RevenueCatUI
```

### Requirements

- Unity 2022.3 LTS or later
- RevenueCat Unity SDK (this package depends on the main RevenueCat package)

### Dependencies

This package is currently stub-only. It does not include native Android/iOS UI
components or dependency definitions. You can wire your flows against the API in
Editor and on devices (no-ops), and add native UI code separately when ready.

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

| Method | Description |
|--------|-------------|
| `PresentPaywall()` | Present paywall with default offering |
| `PresentPaywall(PaywallOptions)` | Present paywall with specific options |
| `PresentPaywallIfNeeded(string)` | Present paywall only if user lacks entitlement |
| `PresentPaywallIfNeeded(string, PaywallOptions)` | Conditional paywall with options |
| `PresentCustomerCenter()` | Present customer center |
| `IsSupported()` | Check if UI is supported on current platform |

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

### Native Implementation (optional)
This package ships with stubs only. To enable native UI later:
- Add your Android/iOS plugin code under `Plugins/` and platform presenters under `Runtime/Platforms/`.
- Define `REVENUECAT_UI_NATIVE` in Scripting Define Symbols so the factory uses native presenters.
- Provide dependency specs (EDM4U) in `Plugins/Editor/` when you add native code.

### Platform Abstraction
- Factory pattern with `IPaywallPresenter` and `ICustomerCenterPresenter`
- Graceful fallbacks for unsupported platforms
- Unity-style async/await throughout

## Platform Support

- Editor / Any platform: Stubs return immediate results (no UI)
- Native support: Add your native code and set `REVENUECAT_UI_NATIVE`

## Examples

See `RevenueCatUIExample.cs` for complete examples including:
- Basic paywall presentation
- Conditional paywalls based on entitlements  
- Customer center presentation
- Error handling and result processing
- Integration with RevenueCat SDK

## Troubleshooting

### iOS Issues
- Ensure Xcode project has iOS 15.0+ deployment target
- Verify CocoaPods dependencies are resolved
- Check that `PurchasesHybridCommonUI` is included

### Android Issues  
- Verify Unity activity extends `FragmentActivity`
- Check that `purchases-hybrid-common-ui` is in dependencies
- Ensure minimum API level 24

### General
- Call `RevenueCatUI.IsSupported()` before presenting UI
- Initialize RevenueCat SDK before using UI components
- Check Unity console for debug logs with `[RevenueCatUI]` prefix

## Stub Mode (No Native Dependencies)

To integrate the API without pulling native UI dependencies yet, enable stub mode:

- Add scripting define symbol `REVENUECAT_UI_STUBS` in Unity Player Settings.
- Behavior in stub mode:
  - `PresentPaywall()` returns `Cancelled` immediately.
  - `PresentPaywallIfNeeded(...)` returns `NotNeeded` immediately.
  - `PresentCustomerCenter()` completes immediately.
  - `IsSupported()` returns `true` so you can wire flows.

Remove the define once you are ready to resolve Android/iOS dependencies and ship the real UI.

## License

This package follows the same license as the main RevenueCat Unity SDK. 
