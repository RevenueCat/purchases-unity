# RevenueCat UI for Unity (Stub Package)

This package provides the Unity API surface for RevenueCat UI (paywalls and customer center),
implemented with lightweight stubs so you can wire your flows without native dependencies.

Use this to integrate and test control flow on devices. Device builds route
through minimal native stubs; in the Unity Editor the UI is reported as
unsupported by default (no UI shown). When ready, replace the minimal native
code with your real implementations.

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

- Unity 2022.3 LTS or later (package.json enforces 2022.3)
- RevenueCat Unity SDK (this package depends on the main RevenueCat package)

### Dependencies

By default this package runs in stub mode. On iOS/Android device builds, the
API routes through minimal native stubs that immediately return results (no
external dependencies). In the Editor and other platforms, the API reports
unsupported and no UI is presented. When you’re ready for real UI, replace the
minimal native code with your implementation and add dependencies as needed.

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
- `PresentPaywallIfNeeded(string[, PaywallOptions])` — returns `NotPresented` immediately
- `PresentCustomerCenter()` — completes immediately
- `IsSupported()` — returns `true` on iOS/Android device builds when both Paywall and Customer Center are available; returns `false` on other platforms (Editor, Windows, macOS, WebGL, etc.)
- `IsPaywallSupported()` — per-feature capability
- `IsCustomerCenterSupported()` — per-feature capability

### PaywallOptions

```csharp
var options = new PaywallOptions
{
    OfferingIdentifier = "premium",      // Optional: specific offering
    DisplayCloseButton = true            // Show close button (iOS only)
};
```

### Callbacks and Fallback

- Default: Pure code callbacks. `PresentPaywall(...)` returns a `Task<PaywallResult>` and uses a native function-pointer callback to complete it. No GameObject is created implicitly.
- Optional fallback: UnitySendMessage. If your environment requires or you prefer a GameObject-based callback, opt in explicitly:

```csharp
// Optional: enable UnitySendMessage fallback
RevenueCat.UI.RevenueCatUI.EnableUnityMessageFallback(); // or provide a custom receiver name

// Present
var result = await RevenueCat.UI.RevenueCatUI.PresentPaywall();
```

Disable fallback at any time:

```csharp
RevenueCat.UI.RevenueCatUI.DisableUnityMessageFallback();
```

Notes:
- The fallback registers a persistent GameObject to receive callbacks from native via `UnitySendMessage`.
- Pure-code remains the primary mechanism; fallback is off by default.

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
- Android class: `com.revenuecat.unity.ui.RevenueCatUI`
  - Uses pure-code callbacks via Java interfaces registered from C# (no GameObject required)
  - Separate callbacks per feature: `PaywallCallbacks` and `CustomerCenterCallbacks`
- iOS functions: `rcui_presentPaywall`, `rcui_presentPaywallIfNeeded`, `rcui_presentCustomerCenter`, `rcui_isSupported`
  - Pure-code C callbacks per feature: paywall functions return results via a paywall callback; customer center uses its own callback. No GameObject required.

If/when you add real native UI, update the platform presenters in
`RevenueCatUI/Scripts/Platforms/{Android,iOS}` to select your implementations.

### Platform Abstraction
- Factory pattern with `IPaywallPresenter` and `ICustomerCenterPresenter`
- Graceful fallbacks for unsupported platforms
- Unity-style async/await throughout

### Threading and Logging
- Callbacks may complete on any thread. If you need to interact with Unity APIs,
  dispatch back to the main thread (e.g., via a main-thread dispatcher).
- Logs are verbose in Development builds. Production builds minimize logs; gate
  additional logging behind your own flags where needed.

## Platform Support

- iOS/Android device: Minimal native stubs return immediate results (no UI)
- Editor / Other platforms: Reported unsupported (no UI)

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
