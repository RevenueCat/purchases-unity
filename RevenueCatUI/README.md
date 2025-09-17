# RevenueCat UI for Unity (Stub Package)

This package provides the Unity API surface for RevenueCat UI (paywalls),
implemented with lightweight stubs so you can wire flows without native dependencies.

Use this to integrate and test control flow on devices. Device builds route
through minimal native stubs; in the Unity Editor the UI is reported as
unsupported (no UI shown). When ready, replace the minimal native code with your
real implementations.

## Installation

Add to your Unity project's `Packages/manifest.json` during development only:

```
{
  "dependencies": {
    "com.revenuecat.purchases-unity": "file:../RevenueCat",
    "com.revenuecat.purchases-ui-unity": "file:../RevenueCatUI"
  }
}
```

Do not include this dependency in distribution packages.

### Requirements

- Unity 2022.3 LTS or later (package.json enforces 2022.3)
- RevenueCat Unity SDK (this package depends on the main RevenueCat package)

## Quick Start

```csharp
using RevenueCat.UI;

public class MySubscriptionManager : MonoBehaviour 
{
    public async void ShowPaywall()
    {
        if (!RevenueCatUI.IsSupported())
            return;

        var result = await RevenueCatUI.PresentPaywall();
        Debug.Log($"Paywall result: {result.Result}");
    }
}
```

## API Reference (stubbed)

- `PresentPaywall()` — returns `Cancelled` immediately
- `PresentPaywall(PaywallOptions)` — same as above with options
- `PresentPaywallIfNeeded(string[, PaywallOptions])` — returns `NotPresented` immediately
- `IsSupported()` — returns `true` on iOS/Android device builds when Paywall is available; returns `false` on other platforms

### PaywallOptions

```csharp
var options = new PaywallOptions
{
    OfferingIdentifier = "premium",
    DisplayCloseButton = true // iOS only (ignored by V2 paywalls)
};
```

### PaywallResult

```csharp
public enum PaywallResultType 
{
    NotPresented,
    Cancelled,
    Error,
    Purchased,
    Restored
}
```

## Architecture

### Native Implementation (reference only)
- Android class: `com.revenuecat.unity.ui.RevenueCatUI`
  - Uses pure-code callbacks via a Java interface registered from C# (no GameObject required)
  - Callback interface: `PaywallCallbacks`
- iOS functions: `rcui_presentPaywall`, `rcui_presentPaywallIfNeeded`, `rcui_isSupported`
  - Pure-code C callbacks for paywall results. No GameObject required.

If/when you add real native UI, update the platform presenters in
`RevenueCatUI/Scripts/Platforms/{Android,iOS}` to select your implementations.

### Platform Abstraction
- Factory pattern with `IPaywallPresenter`
- Graceful fallbacks for unsupported platforms
- Unity-style async/await throughout

### Threading and Logging
- Callbacks may complete on any thread. If you need to interact with Unity APIs,
  dispatch back to the main thread.
- Logs are verbose in Development builds; production builds minimize logs.

## Platform Support

- iOS/Android device: Minimal native stubs return immediate results (no UI)
- Editor / Other platforms: Reported unsupported (no UI)

## License

This package follows the same license as the main RevenueCat Unity SDK.

