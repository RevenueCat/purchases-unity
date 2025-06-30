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

- **Unity 2022.3 LTS or later**
- **RevenueCat Unity SDK** - This package depends on the main RevenueCat package
- **iOS 15.0+** - Required for paywall presentation
- **Android API 24+** - Required for UI components

### Dependencies

This package automatically includes:
- **iOS**: `PurchasesHybridCommonUI` via CocoaPods
- **Android**: `purchases-hybrid-common-ui` via Gradle

Dependencies are managed by Unity's External Dependency Manager.

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

### iOS Implementation
- **Native Bridge**: `RevenueCatUIBridge.mm` (Objective-C++)
- **Dependencies**: `PurchasesHybridCommonUI` via External Dependency Manager
- **Minimum Version**: iOS 15.0 (required for paywall presentation)

### Android Implementation  
- **Java Bridge**: `RevenueCatUIPlugin.java`
- **Dependencies**: `purchases-hybrid-common-ui` via Gradle
- **Minimum Version**: API 24 (Android 7.0)

### Platform Abstraction
- Factory pattern with `IPaywallPresenter` and `ICustomerCenterPresenter`
- Graceful fallbacks for unsupported platforms
- Unity-style async/await throughout

## Platform Support

| Platform | Paywalls | Customer Center | Notes |
|----------|----------|----------------|-------|
| iOS      | ✅       | ✅             | Requires iOS 15.0+ |
| Android  | ✅       | ✅             | Requires API 24+ |  
| Editor   | ❌       | ❌             | Logs warnings only |
| Other    | ❌       | ❌             | Graceful fallbacks |

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

## License

This package follows the same license as the main RevenueCat Unity SDK. 