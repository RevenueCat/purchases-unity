# Implementation Plan: Custom Variables Support for Unity RevenueCat SDK

## Overview

Add `customVariables` parameter to all paywall APIs, allowing developers to pass dynamic values for text substitution in paywalls using `{{ custom.variable_name }}` syntax.

## Files to Modify

### C# Layer (3 files)

#### 1. `RevenueCatUI/Scripts/PaywallOptions.cs`

- Add `internal Dictionary<string, string> CustomVariables { get; }` property
- Add `customVariables` parameter to all constructors
- Add helper method to serialize to JSON string

```csharp
using System.Collections.Generic;

// Add property
internal Dictionary<string, string> CustomVariables { get; }

// Update constructor (line ~63)
public PaywallOptions(bool displayCloseButton = false, Dictionary<string, string> customVariables = null)
{
    _offeringSelection = null;
    DisplayCloseButton = displayCloseButton;
    CustomVariables = customVariables;
}

// Update constructor (line ~74)
public PaywallOptions(Purchases.Offering offering, bool displayCloseButton = false, Dictionary<string, string> customVariables = null)
{
    _offeringSelection = offering != null ? new OfferingSelection.OfferingType(offering) : null;
    DisplayCloseButton = displayCloseButton;
    CustomVariables = customVariables;
}

// Update internal constructor (line ~80)
internal PaywallOptions(string offeringIdentifier, bool displayCloseButton = false, Dictionary<string, string> customVariables = null)
{
    _offeringSelection = !string.IsNullOrEmpty(offeringIdentifier) ? new OfferingSelection.IdentifierType(offeringIdentifier) : null;
    DisplayCloseButton = displayCloseButton;
    CustomVariables = customVariables;
}

// Add serialization helper method
internal string CustomVariablesToJsonString()
{
    if (CustomVariables == null || CustomVariables.Count == 0) return null;
    var dict = new SimpleJSON.JSONObject();
    foreach (var kvp in CustomVariables)
    {
        dict[kvp.Key] = kvp.Value;
    }
    return dict.ToString();
}
```

#### 2. `RevenueCatUI/Scripts/Platforms/iOS/IOSPaywallPresenter.cs`

- Update `DllImport` declarations to add `customVariablesJson` parameter
- Pass `customVariablesJson` in method calls

```csharp
// Update DllImport (line ~13)
[DllImport("__Internal")] private static extern void rcui_presentPaywall(
    string offeringIdentifier,
    string presentedOfferingContextJson,
    bool displayCloseButton,
    string customVariablesJson,  // NEW
    PaywallResultCallback cb);

[DllImport("__Internal")] private static extern void rcui_presentPaywallIfNeeded(
    string requiredEntitlementIdentifier,
    string offeringIdentifier,
    string presentedOfferingContextJson,
    bool displayCloseButton,
    string customVariablesJson,  // NEW
    PaywallResultCallback cb);

// Update PresentPaywallAsync (line ~30-31)
var presentedOfferingContextJson = options?.PresentedOfferingContext?.ToJsonString();
var customVariablesJson = options?.CustomVariablesToJsonString();  // NEW
rcui_presentPaywall(
    options?.OfferingIdentifier,
    presentedOfferingContextJson,
    options?.DisplayCloseButton ?? false,
    customVariablesJson,  // NEW
    OnResult);

// Update PresentPaywallIfNeededAsync (line ~53-55)
var presentedOfferingContextJson = options?.PresentedOfferingContext?.ToJsonString();
var customVariablesJson = options?.CustomVariablesToJsonString();  // NEW
rcui_presentPaywallIfNeeded(
    requiredEntitlementIdentifier,
    options?.OfferingIdentifier,
    presentedOfferingContextJson,
    options?.DisplayCloseButton ?? true,
    customVariablesJson,  // NEW
    OnResult);
```

#### 3. `RevenueCatUI/Scripts/Platforms/Android/AndroidPaywallPresenter.cs`

- Pass `customVariablesJson` in CallStatic method calls

```csharp
// Update PresentPaywallAsync (line ~52-59)
var offeringIdentifier = options?.OfferingIdentifier;
var displayCloseButton = options?.DisplayCloseButton ?? false;
var presentedOfferingContextJson = options?.PresentedOfferingContext?.ToJsonString();
var customVariablesJson = options?.CustomVariablesToJsonString();  // NEW

Debug.Log($"[RevenueCatUI][Android] presentPaywall offering='{offeringIdentifier ?? "<null>"}', " +
          $"displayCloseButton={displayCloseButton}");
var currentActivity = AndroidApplication.currentActivity;
_plugin.CallStatic("presentPaywall", new object[] {
    currentActivity,
    offeringIdentifier,
    presentedOfferingContextJson,
    displayCloseButton,
    customVariablesJson  // NEW
});

// Update PresentPaywallIfNeededAsync (line ~87-93)
var offeringIdentifier = options?.OfferingIdentifier;
var displayCloseButton = options?.DisplayCloseButton ?? true;
var presentedOfferingContextJson = options?.PresentedOfferingContext?.ToJsonString();
var customVariablesJson = options?.CustomVariablesToJsonString();  // NEW

Debug.Log($"[RevenueCatUI][Android] presentPaywallIfNeeded entitlement='{requiredEntitlementIdentifier}', '" +
          $"offering='{offeringIdentifier ?? "<null>"}', displayCloseButton={displayCloseButton}");
var currentActivity = AndroidApplication.currentActivity;
_plugin.CallStatic("presentPaywallIfNeeded", new object[] {
    currentActivity,
    requiredEntitlementIdentifier,
    offeringIdentifier,
    presentedOfferingContextJson,
    displayCloseButton,
    customVariablesJson  // NEW
});
```

---

### iOS Native Layer (1 file)

#### 4. `RevenueCatUI/Plugins/iOS/RevenueCatUI.m`

- Add constant for new option key
- Update `RCUICreateOptionsDictionary` to accept customVariablesJson
- Update function signatures

```objc
// Add constant (after line ~15)
static NSString *const kRCUIOptionCustomVariables = @"customVariables";

// Update RCUICreateOptionsDictionary (lines ~103-119)
static NSMutableDictionary *RCUICreateOptionsDictionary(
    NSString *offeringIdentifier,
    NSString *presentedOfferingContextJson,
    BOOL displayCloseButton,
    NSString *customVariablesJson)  // NEW parameter
{
    NSMutableDictionary *options = [NSMutableDictionary new];
    options[kRCUIOptionDisplayCloseButton] = @(displayCloseButton);

    if (offeringIdentifier.length > 0) {
        options[kRCUIOptionOfferingIdentifier] = offeringIdentifier;
    }

    if (presentedOfferingContextJson.length > 0) {
        id presentedOfferingContext = RCUIJSONObjectFromJSONString(presentedOfferingContextJson);
        if (presentedOfferingContext) {
            options[kRCUIOptionPresentedOfferingContext] = presentedOfferingContext;
        }
    }

    // NEW: Handle custom variables
    if (customVariablesJson.length > 0) {
        id customVariables = RCUIJSONObjectFromJSONString(customVariablesJson);
        if (customVariables) {
            options[kRCUIOptionCustomVariables] = customVariables;
        }
    }

    return options;
}

// Update RCUIPresentPaywallInternal (lines ~130-150)
static void RCUIPresentPaywallInternal(
    NSString *offeringIdentifier,
    NSString *presentedOfferingContextJson,
    BOOL displayCloseButton,
    NSString *customVariablesJson,  // NEW parameter
    RCUIPaywallResultCallback callback)
{
    dispatch_async(dispatch_get_main_queue(), ^{
        if (@available(iOS 15.0, *)) {
            __block PaywallProxy *proxy = [[PaywallProxy alloc] init];

            NSMutableDictionary *options = RCUICreateOptionsDictionary(
                offeringIdentifier,
                presentedOfferingContextJson,
                displayCloseButton,
                customVariablesJson);  // NEW

            [proxy presentPaywallWithOptions:options
                        paywallResultHandler:^(NSString * _Nonnull resultName) {
                NSString *token = RCUINormalizedResultToken(resultName);
                RCUIInvokeCallback(callback, token, nil);
                proxy = nil;
            }];
        } else {
            RCUIInvokeCallback(callback, @"NOT_PRESENTED", @"Requires iOS 15.0+");
        }
    });
}

// Update RCUIPresentPaywallIfNeededInternal (lines ~152-174)
static void RCUIPresentPaywallIfNeededInternal(
    NSString *requiredEntitlementIdentifier,
    NSString *offeringIdentifier,
    NSString *presentedOfferingContextJson,
    BOOL displayCloseButton,
    NSString *customVariablesJson,  // NEW parameter
    RCUIPaywallResultCallback callback)
{
    dispatch_async(dispatch_get_main_queue(), ^{
        if (@available(iOS 15.0, *)) {
            __block PaywallProxy *proxy = [[PaywallProxy alloc] init];

            NSMutableDictionary *options = RCUICreateOptionsDictionary(
                offeringIdentifier,
                presentedOfferingContextJson,
                displayCloseButton,
                customVariablesJson);  // NEW
            options[kRCUIOptionRequiredEntitlementIdentifier] = requiredEntitlementIdentifier;

            [proxy presentPaywallIfNeededWithOptions:options
                                paywallResultHandler:^(NSString * _Nonnull resultName) {
                NSString *token = RCUINormalizedResultToken(resultName);
                RCUIInvokeCallback(callback, token, nil);
                proxy = nil;
            }];
        } else {
            RCUIInvokeCallback(callback, @"NOT_PRESENTED", @"Requires iOS 15.0+");
        }
    });
}

// Update rcui_presentPaywall (lines ~176-184)
void rcui_presentPaywall(
    const char *offeringIdentifier,
    const char *presentedOfferingContextJson,
    bool displayCloseButton,
    const char *customVariablesJson,  // NEW parameter
    RCUIPaywallResultCallback callback)
{
    if (!RCUIEnsureReady(callback)) {
        return;
    }

    NSString *offering = RCUIStringFromCString(offeringIdentifier);
    NSString *contextJson = RCUIStringFromCString(presentedOfferingContextJson);
    NSString *customVarsJson = RCUIStringFromCString(customVariablesJson);  // NEW
    RCUIPresentPaywallInternal(offering, contextJson, displayCloseButton ? YES : NO, customVarsJson, callback);
}

// Update rcui_presentPaywallIfNeeded (lines ~186-205)
void rcui_presentPaywallIfNeeded(
    const char *requiredEntitlementIdentifier,
    const char *offeringIdentifier,
    const char *presentedOfferingContextJson,
    bool displayCloseButton,
    const char *customVariablesJson,  // NEW parameter
    RCUIPaywallResultCallback callback)
{
    if (!RCUIEnsureReady(callback)) {
        return;
    }

    NSString *entitlement = RCUIStringFromCString(requiredEntitlementIdentifier);
    NSString *offering = RCUIStringFromCString(offeringIdentifier);
    NSString *contextJson = RCUIStringFromCString(presentedOfferingContextJson);
    NSString *customVarsJson = RCUIStringFromCString(customVariablesJson);  // NEW

    if (entitlement.length == 0) {
        RCUIPresentPaywallInternal(offering, contextJson, displayCloseButton ? YES : NO, customVarsJson, callback);
        return;
    }

    RCUIPresentPaywallIfNeededInternal(entitlement, offering, contextJson, displayCloseButton ? YES : NO, customVarsJson, callback);
}
```

---

### Android Native Layer (3 files)

#### 5. `RevenueCatUI/Plugins/Android/RevenueCatUI.androidlib/src/main/java/com/revenuecat/purchasesunity/ui/PaywallUnityOptions.java`

- Add `customVariablesJson` field
- Update constructor and Parcelable implementation
- Add getter method

```java
// Add field (after line ~15)
@Nullable
private final String customVariablesJson;

// Update constructor (lines ~17-27)
public PaywallUnityOptions(
        @Nullable String offeringId,
        boolean shouldDisplayDismissButton,
        @Nullable String requiredEntitlementIdentifier,
        @Nullable String presentedOfferingContextJson,
        @Nullable String customVariablesJson  // NEW
) {
    this.offeringId = offeringId;
    this.shouldDisplayDismissButton = shouldDisplayDismissButton;
    this.requiredEntitlementIdentifier = requiredEntitlementIdentifier;
    this.presentedOfferingContextJson = presentedOfferingContextJson;
    this.customVariablesJson = customVariablesJson;  // NEW
}

// Update Parcel constructor (lines ~29-34)
protected PaywallUnityOptions(Parcel in) {
    offeringId = in.readString();
    shouldDisplayDismissButton = in.readByte() != 0;
    requiredEntitlementIdentifier = in.readString();
    presentedOfferingContextJson = in.readString();
    customVariablesJson = in.readString();  // NEW
}

// Add getter (after line ~53)
@Nullable
public String getCustomVariablesJson() {
    return customVariablesJson;
}

// Update writeToParcel (lines ~56-61)
@Override
public void writeToParcel(Parcel dest, int flags) {
    dest.writeString(offeringId);
    dest.writeByte((byte) (shouldDisplayDismissButton ? 1 : 0));
    dest.writeString(requiredEntitlementIdentifier);
    dest.writeString(presentedOfferingContextJson);
    dest.writeString(customVariablesJson);  // NEW
}
```

#### 6. `RevenueCatUI/Plugins/Android/RevenueCatUI.androidlib/src/main/java/com/revenuecat/purchasesunity/ui/RevenueCatUI.java`

- Update method signatures to add `customVariablesJson` parameter

```java
// Update presentPaywall (line ~35-37)
public static void presentPaywall(
    Activity activity,
    String offeringIdentifier,
    String presentedOfferingContextJson,
    boolean displayCloseButton,
    String customVariablesJson)  // NEW
{
    PaywallTrampolineActivity.presentPaywall(
        activity,
        offeringIdentifier,
        presentedOfferingContextJson,
        displayCloseButton,
        customVariablesJson);  // NEW
}

// Update presentPaywallIfNeeded (lines ~39-41)
public static void presentPaywallIfNeeded(
    Activity activity,
    String requiredEntitlementIdentifier,
    String offeringIdentifier,
    String presentedOfferingContextJson,
    boolean displayCloseButton,
    String customVariablesJson)  // NEW
{
    PaywallTrampolineActivity.presentPaywallIfNeeded(
        activity,
        requiredEntitlementIdentifier,
        offeringIdentifier,
        presentedOfferingContextJson,
        displayCloseButton,
        customVariablesJson);  // NEW
}
```

#### 7. `RevenueCatUI/Plugins/Android/RevenueCatUI.androidlib/src/main/java/com/revenuecat/purchasesunity/ui/PaywallTrampolineActivity.java`

- Add import for custom variable handling
- Update method signatures and option construction
- Parse and apply custom variables to launcher

```java
// Add imports (after line ~18)
import java.util.Map;
import java.util.HashMap;
import com.revenuecat.purchases.ui.revenuecatui.PaywallOptions;

// Update launchPaywallIfNeeded (lines ~56-92)
private void launchPaywallIfNeeded(PaywallUnityOptions options) {
    String requiredEntitlementIdentifier = options.getRequiredEntitlementIdentifier();
    String offeringId = options.getOfferingId();
    boolean shouldDisplayDismissButton = options.getShouldDisplayDismissButton();
    String presentedOfferingContextJson = options.getPresentedOfferingContextJson();
    String customVariablesJson = options.getCustomVariablesJson();  // NEW
    Map<String, String> customVariables = parseCustomVariables(customVariablesJson);  // NEW

    if (offeringId == null) {
        // Build PaywallOptions with custom variables if available
        // Note: May need to use PaywallActivityLauncher API that supports custom variables
        launcher.launchIfNeeded(
                requiredEntitlementIdentifier,
                null,
                customVariables,  // Pass custom variables
                shouldDisplayDismissButton,
                Build.VERSION.SDK_INT >= 35,
                paywallDisplayResult -> {
                    if (!paywallDisplayResult) {
                        RevenueCatUI.sendPaywallResult(RESULT_NOT_PRESENTED);
                        finish();
                    }
                }
        );
    } else {
        PresentedOfferingContext presentedOfferingContext = mapPresentedOfferingContext(presentedOfferingContextJson, offeringId);
        launcher.launchIfNeededWithOfferingId(
                requiredEntitlementIdentifier,
                offeringId,
                presentedOfferingContext,
                customVariables,  // Pass custom variables instead of null
                shouldDisplayDismissButton,
                Build.VERSION.SDK_INT >= 35,
                paywallDisplayResult -> {
                    if (!paywallDisplayResult) {
                        RevenueCatUI.sendPaywallResult(RESULT_NOT_PRESENTED);
                        finish();
                    }
                }
        );
    }
}

// Update launchPaywall (lines ~95-115)
private void launchPaywall(PaywallUnityOptions options) {
    String offeringId = options.getOfferingId();
    boolean shouldDisplayDismissButton = options.getShouldDisplayDismissButton();
    String presentedOfferingContextJson = options.getPresentedOfferingContextJson();
    String customVariablesJson = options.getCustomVariablesJson();  // NEW
    Map<String, String> customVariables = parseCustomVariables(customVariablesJson);  // NEW

    if (offeringId != null) {
        PresentedOfferingContext presentedOfferingContext = mapPresentedOfferingContext(presentedOfferingContextJson, offeringId);
        launcher.launchWithOfferingId(
                offeringId,
                presentedOfferingContext,
                customVariables,  // Pass custom variables instead of null
                shouldDisplayDismissButton
        );
    } else {
        launcher.launch(
            null,
            customVariables,  // Pass custom variables instead of null
            shouldDisplayDismissButton
        );
    }
}

// Add helper method (after mapPresentedOfferingContext method, around line ~146)
@Nullable
private Map<String, String> parseCustomVariables(@Nullable String jsonString) {
    if (jsonString == null || jsonString.isEmpty()) {
        return null;
    }
    try {
        JSONObject json = new JSONObject(jsonString);
        Map<String, String> result = new HashMap<>();
        java.util.Iterator<String> keys = json.keys();
        while (keys.hasNext()) {
            String key = keys.next();
            String value = json.optString(key);
            if (value != null) {
                result.put(key, value);
            }
        }
        return result.isEmpty() ? null : result;
    } catch (JSONException e) {
        Log.w(TAG, "Failed to parse custom variables JSON: " + jsonString, e);
        return null;
    }
}

// Update presentPaywall static method (lines ~177-195)
public static void presentPaywall(
    Activity activity,
    @Nullable String offeringIdentifier,
    @Nullable String presentedOfferingContextJson,
    boolean displayCloseButton,
    @Nullable String customVariablesJson)  // NEW
{
    if (activity == null) {
        Log.e(TAG, "Activity is null; cannot launch paywall");
        RevenueCatUI.sendPaywallResult(RESULT_ERROR);
        return;
    }

    PaywallUnityOptions options = new PaywallUnityOptions(
        offeringIdentifier,
        displayCloseButton,
        null,
        presentedOfferingContextJson,
        customVariablesJson);  // NEW

    Intent intent = new Intent(activity, PaywallTrampolineActivity.class);
    intent.putExtra(EXTRA_PAYWALL_OPTIONS, options);

    try {
        activity.startActivity(intent);
    } catch (Throwable t) {
        Log.e(TAG, "Error launching PaywallTrampolineActivity", t);
        RevenueCatUI.sendPaywallResult(RESULT_ERROR);
    }
}

// Update presentPaywallIfNeeded static method (lines ~197-221)
public static void presentPaywallIfNeeded(
    Activity activity,
    String requiredEntitlementIdentifier,
    @Nullable String offeringIdentifier,
    @Nullable String presentedOfferingContextJson,
    boolean displayCloseButton,
    @Nullable String customVariablesJson)  // NEW
{
    if (activity == null) {
        Log.e(TAG, "Activity is null; cannot launch paywall");
        RevenueCatUI.sendPaywallResult(RESULT_ERROR);
        return;
    }

    if (requiredEntitlementIdentifier == null) {
        Log.e(TAG, "Required entitlement identifier is null; cannot launch paywall if needed");
        RevenueCatUI.sendPaywallResult(RESULT_ERROR);
        return;
    }

    PaywallUnityOptions options = new PaywallUnityOptions(
        offeringIdentifier,
        displayCloseButton,
        requiredEntitlementIdentifier,
        presentedOfferingContextJson,
        customVariablesJson);  // NEW

    Intent intent = new Intent(activity, PaywallTrampolineActivity.class);
    intent.putExtra(EXTRA_PAYWALL_OPTIONS, options);

    try {
        activity.startActivity(intent);
    } catch (Throwable t) {
        Log.e(TAG, "Error launching PaywallTrampolineActivity for presentPaywallIfNeeded", t);
        RevenueCatUI.sendPaywallResult(RESULT_ERROR);
    }
}
```

---

### API Tests (1 file)

#### 8. `IntegrationTests/Assets/APITests/PaywallOptionsAPITests.cs` (NEW FILE)

```csharp
using System.Collections.Generic;
using UnityEngine;
using RevenueCatUI;

namespace DefaultNamespace
{
    public class PaywallOptionsAPITests : MonoBehaviour
    {
        private void Start()
        {
            // Test PaywallOptions with no parameters
            PaywallOptions options1 = new PaywallOptions();

            // Test PaywallOptions with displayCloseButton only
            PaywallOptions options2 = new PaywallOptions(displayCloseButton: true);

            // Test PaywallOptions with customVariables
            PaywallOptions options3 = new PaywallOptions(
                displayCloseButton: false,
                customVariables: new Dictionary<string, string>
                {
                    { "player_name", "John" },
                    { "level", "5" }
                }
            );

            // Test PaywallOptions with offering
            Purchases.Offering offering = null; // Would be real offering in actual test
            PaywallOptions options4 = new PaywallOptions(
                offering: offering,
                displayCloseButton: true,
                customVariables: new Dictionary<string, string>
                {
                    { "player_name", "Jane" }
                }
            );
        }
    }
}
```

---

## Implementation Order

1. **C# Layer**
   - `PaywallOptions.cs` (add property, update constructors, add serialization)
   - `IOSPaywallPresenter.cs` (update DllImport and calls)
   - `AndroidPaywallPresenter.cs` (update CallStatic calls)

2. **iOS Native Layer**
   - `RevenueCatUI.m` (add constant, update functions)

3. **Android Native Layer**
   - `PaywallUnityOptions.java` (add field, update Parcelable)
   - `RevenueCatUI.java` (update method signatures)
   - `PaywallTrampolineActivity.java` (parse and apply custom variables)

4. **API Tests**
   - `PaywallOptionsAPITests.cs` (verify API surface)

---

## Verification

1. **Build iOS**
   - Open Unity project
   - Build for iOS
   - Verify no compilation errors in Xcode
   - Test paywall presentation with custom variables

2. **Build Android**
   - Open Unity project
   - Build for Android
   - Verify no compilation errors
   - Test paywall presentation with custom variables

3. **Manual Testing**
   - Configure a paywall with `{{ custom.player_name }}` variable in RevenueCat dashboard
   - Present paywall with `customVariables: { "player_name": "TestUser" }`
   - Verify the variable is substituted correctly

---

## Notes

- **Type**: `Dictionary<string, string>?` - nullable, string keys and values only
- **Native SDK validation**: Native SDKs validate key format (starts with letter, alphanumeric + underscores)
- **No C#-side validation needed**: Native layer handles validation
- **PaywallProxy support**: `PaywallProxy.PaywallOptionsKeys.customVariables` must exist in PurchasesHybridCommonUI for iOS modal presentation
- **Android SDK support**: Requires `PaywallActivityLauncher` methods that accept `Map<String, String>` for custom variables (check SDK version compatibility)

---

## Dependencies

- **iOS**: PurchasesHybridCommonUI must support `customVariables` in `PaywallProxy` options
- **Android**: RevenueCatUI Android SDK must support custom variables in `PaywallActivityLauncher` methods

---

## Usage Example

```csharp
using RevenueCatUI;
using System.Collections.Generic;

// Basic usage
var result = await PaywallsPresenter.Present(new PaywallOptions(
    customVariables: new Dictionary<string, string>
    {
        { "player_name", "John" },
        { "current_level", "42" },
        { "coins_balance", "1000" }
    }
));

// With offering and close button
var offerings = await Purchases.GetOfferings();
var result = await PaywallsPresenter.Present(new PaywallOptions(
    offering: offerings.Current,
    displayCloseButton: true,
    customVariables: new Dictionary<string, string>
    {
        { "player_name", currentPlayer.Name }
    }
));

// Present if needed with custom variables
var result = await PaywallsPresenter.PresentIfNeeded(
    "premium",
    new PaywallOptions(
        customVariables: new Dictionary<string, string>
        {
            { "player_name", "Jane" }
        }
    )
);
```
