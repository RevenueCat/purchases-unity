# Unity Callback Request Correlation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make every one-shot `Purchases` callback resolve the matching invocation when multiple calls of the same API are in flight.

**Architecture:** Generate a unique request ID in managed C# for every callback-based invocation, store the callback in a typed, thread-safe registry, and pass the ID through `IPurchasesWrapper` to the Android or iOS bridge. Native responses add the ID to their existing JSON payload; `Purchases` removes and invokes only the callback registered for that ID. Existing public `Purchases` method signatures and callback result payloads remain unchanged.

**Tech Stack:** Unity C#, Unity Test Framework/NUnit, Android Java/JNI through `AndroidJavaClass`, Objective-C/C ABI through `DllImport`, RevenueCat Hybrid Common.

## Global Constraints

- Do not remove, rename, or change the signature of any public `Purchases` API.
- Adding the internal bridge parameter must not raise the minimum supported versions: Unity 2021.3+, iOS 13.0+, Android API 21+, Play Billing Library 8.0.0+.
- Register every invocation, including invocations whose callback is `null`, so every native response still consumes exactly one request ID.
- Remove a callback from the registry before invoking user code, preserving the nested-call behavior fixed in PR #605.
- Do not assume responses arrive in invocation order.
- A missing, unknown, duplicated, or type-mismatched request ID must not invoke or remove an unrelated callback.
- Keep paywall and Customer Center single-presentation behavior unchanged.
- Do not add task-based public APIs or change the serialized `listener` field in this correctness fix.
- Do not change native response model fields other than adding the internal top-level `requestId`.
- Add no production dependency.

---

## File Structure

### New files

- `RevenueCat/Scripts/CallbackRegistry.cs` — owns request ID generation and typed, one-shot callback storage.
- `RevenueCat/Scripts/AssemblyInfo.cs` — grants the Editor test assembly access to internal callback infrastructure.
- `IntegrationTests/Assets/CallbackTests/Editor/CallbackRegistryTests.cs` — verifies out-of-order, null, duplicate, and type-mismatch behavior.
- `IntegrationTests/Assets/CallbackTests/Editor/PurchasesCallbackRoutingTests.cs` — verifies a real `Purchases` receive method dispatches out-of-order responses correctly.
- `IntegrationTests/Assets/CallbackTests/RevenueCat.CallbackTests.asmdef` — Editor-only NUnit test assembly.
- Unity `.meta` files for each new Unity asset and directory.

### Modified files

- `RevenueCat/Scripts/Purchases.cs` — replaces one mutable callback field per operation with the registry.
- `RevenueCat/Scripts/PurchasesWrapper.cs` — adds an optional request ID to one-shot internal bridge calls.
- `RevenueCat/Scripts/PurchasesWrapperAndroid.cs` — forwards request IDs into Java calls.
- `RevenueCat/Scripts/PurchasesWrapperiOS.cs` — forwards request IDs into C ABI calls.
- `RevenueCat/Scripts/PurchasesWrapperNoop.cs` — implements the updated interface without changing no-op behavior.
- `RevenueCat/Plugins/Android/PurchasesWrapper.java` — captures IDs in native completions and adds them to JSON responses.
- `RevenueCat/Plugins/iOS/PurchasesUnityHelper.m` — captures IDs in native completions and adds them to JSON responses.

---

### Task 1: Add the Typed One-Shot Callback Registry

**Files:**

- Create: `RevenueCat/Scripts/CallbackRegistry.cs`
- Create: `RevenueCat/Scripts/CallbackRegistry.cs.meta`
- Create: `RevenueCat/Scripts/AssemblyInfo.cs`
- Create: `RevenueCat/Scripts/AssemblyInfo.cs.meta`
- Create: `IntegrationTests/Assets/CallbackTests.meta`
- Create: `IntegrationTests/Assets/CallbackTests/Editor.meta`
- Create: `IntegrationTests/Assets/CallbackTests/RevenueCat.CallbackTests.asmdef`
- Create: `IntegrationTests/Assets/CallbackTests/RevenueCat.CallbackTests.asmdef.meta`
- Create: `IntegrationTests/Assets/CallbackTests/Editor/CallbackRegistryTests.cs`
- Create: `IntegrationTests/Assets/CallbackTests/Editor/CallbackRegistryTests.cs.meta`

**Interfaces:**

- Consumes: `System.Guid`, `Dictionary<string, CallbackEntry>`, and NUnit.
- Produces: `CallbackRegistry.Register<T>(T callback)`, `CallbackRegistry.TryTake<T>(string requestId, out T callback)`, and `CallbackRegistry.Clear()`.

- [ ] **Step 1: Create the test assembly and friend-assembly declaration**

Create `IntegrationTests/Assets/CallbackTests/RevenueCat.CallbackTests.asmdef`:

```json
{
    "name": "RevenueCat.CallbackTests",
    "rootNamespace": "",
    "references": [
        "revenuecat.purchases-unity"
    ],
    "includePlatforms": [
        "Editor"
    ],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": false,
    "defineConstraints": [],
    "versionDefines": [],
    "optionalUnityReferences": [
        "TestAssemblies"
    ],
    "noEngineReferences": false
}
```

Create `RevenueCat/Scripts/AssemblyInfo.cs`:

```csharp
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("RevenueCat.CallbackTests")]
```

Use these fixed GUIDs in the Unity metadata:

| Asset | GUID | Importer |
|---|---|---|
| `RevenueCat/Scripts/CallbackRegistry.cs.meta` | `2fe20b5d92334e0092468be12148e65f` | `MonoImporter` |
| `RevenueCat/Scripts/AssemblyInfo.cs.meta` | `268495a3951243cd8ff5400eb6d5cab5` | `MonoImporter` |
| `IntegrationTests/Assets/CallbackTests.meta` | `e6209c54c8c6414da0098f21cb948ecc` | folder `DefaultImporter` |
| `IntegrationTests/Assets/CallbackTests/Editor.meta` | `72e6daaaa28249daaf276311f1223b4a` | folder `DefaultImporter` |
| `RevenueCat.CallbackTests.asmdef.meta` | `9c209e381a124214a8091c97643c3476` | `AssemblyDefinitionImporter` |
| `CallbackRegistryTests.cs.meta` | `500580b206714eca87c1f8e73dbaa95a` | `MonoImporter` |

Create `RevenueCat/Scripts/CallbackRegistry.cs.meta`:

```yaml
fileFormatVersion: 2
guid: 2fe20b5d92334e0092468be12148e65f
MonoImporter:
  externalObjects: {}
  serializedVersion: 2
  defaultReferences: []
  executionOrder: 0
  icon: {instanceID: 0}
  userData:
  assetBundleName:
  assetBundleVariant:
```

Create `RevenueCat/Scripts/AssemblyInfo.cs.meta` with the same `MonoImporter` fields and:

```yaml
fileFormatVersion: 2
guid: 268495a3951243cd8ff5400eb6d5cab5
MonoImporter:
  externalObjects: {}
  serializedVersion: 2
  defaultReferences: []
  executionOrder: 0
  icon: {instanceID: 0}
  userData:
  assetBundleName:
  assetBundleVariant:
```

Create `IntegrationTests/Assets/CallbackTests.meta`:

```yaml
fileFormatVersion: 2
guid: e6209c54c8c6414da0098f21cb948ecc
folderAsset: yes
DefaultImporter:
  externalObjects: {}
  userData:
  assetBundleName:
  assetBundleVariant:
```

Create `IntegrationTests/Assets/CallbackTests/Editor.meta` with the same folder importer fields and:

```yaml
fileFormatVersion: 2
guid: 72e6daaaa28249daaf276311f1223b4a
folderAsset: yes
DefaultImporter:
  externalObjects: {}
  userData:
  assetBundleName:
  assetBundleVariant:
```

Create `IntegrationTests/Assets/CallbackTests/RevenueCat.CallbackTests.asmdef.meta`:

```yaml
fileFormatVersion: 2
guid: 9c209e381a124214a8091c97643c3476
AssemblyDefinitionImporter:
  externalObjects: {}
  userData:
  assetBundleName:
  assetBundleVariant:
```

Create `IntegrationTests/Assets/CallbackTests/Editor/CallbackRegistryTests.cs.meta`:

```yaml
fileFormatVersion: 2
guid: 500580b206714eca87c1f8e73dbaa95a
MonoImporter:
  externalObjects: {}
  serializedVersion: 2
  defaultReferences: []
  executionOrder: 0
  icon: {instanceID: 0}
  userData:
  assetBundleName:
  assetBundleVariant:
```

- [ ] **Step 2: Write failing registry tests**

Create `IntegrationTests/Assets/CallbackTests/Editor/CallbackRegistryTests.cs`:

```csharp
using System;
using NUnit.Framework;

public class CallbackRegistryTests
{
    private CallbackRegistry _registry;

    [SetUp]
    public void SetUp()
    {
        _registry = new CallbackRegistry();
    }

    [Test]
    public void TakesCallbacksByRequestIdInAnyOrder()
    {
        Action first = () => { };
        Action second = () => { };

        var firstId = _registry.Register(first);
        var secondId = _registry.Register(second);

        Assert.That(_registry.TryTake(secondId, out Action receivedSecond), Is.True);
        Assert.That(receivedSecond, Is.SameAs(second));
        Assert.That(_registry.TryTake(firstId, out Action receivedFirst), Is.True);
        Assert.That(receivedFirst, Is.SameAs(first));
    }

    [Test]
    public void TakingARequestIdTwiceFailsTheSecondTime()
    {
        var requestId = _registry.Register<Action>(() => { });

        Assert.That(_registry.TryTake(requestId, out Action _), Is.True);
        Assert.That(_registry.TryTake(requestId, out Action duplicate), Is.False);
        Assert.That(duplicate, Is.Null);
    }

    [Test]
    public void NullCallbackStillCreatesConsumableRequest()
    {
        Action callback = null;
        var requestId = _registry.Register(callback);

        Assert.That(requestId, Is.Not.Empty);
        Assert.That(_registry.TryTake(requestId, out Action received), Is.True);
        Assert.That(received, Is.Null);
        Assert.That(_registry.TryTake(requestId, out Action _), Is.False);
    }

    [Test]
    public void TypeMismatchDoesNotConsumeRequest()
    {
        Action callback = () => { };
        var requestId = _registry.Register(callback);

        Assert.That(_registry.TryTake(requestId, out Func<int> wrongType), Is.False);
        Assert.That(wrongType, Is.Null);
        Assert.That(_registry.TryTake(requestId, out Action received), Is.True);
        Assert.That(received, Is.SameAs(callback));
    }

    [Test]
    public void ClearRemovesEveryPendingRequest()
    {
        var firstId = _registry.Register<Action>(() => { });
        var secondId = _registry.Register<Action>(() => { });

        _registry.Clear();

        Assert.That(_registry.TryTake(firstId, out Action _), Is.False);
        Assert.That(_registry.TryTake(secondId, out Action _), Is.False);
    }
}
```

- [ ] **Step 3: Run the tests and verify they fail**

Prepare the IntegrationTests project without copying SDK sources:

```bash
ln -s "$PWD/RevenueCat" "$PWD/IntegrationTests/Assets/RevenueCat"
ln -s "$PWD/RevenueCatUI" "$PWD/IntegrationTests/Assets/RevenueCatUI"
```

Run:

```bash
"/Applications/Unity/Hub/Editor/6000.2.6f2/Unity.app/Contents/MacOS/Unity" \
  -batchmode -nographics -quit \
  -projectPath "$PWD/IntegrationTests" \
  -runTests -testPlatform EditMode \
  -testResults "$PWD/.context/callback-tests.xml" \
  -logFile -
```

Expected: compilation fails because `CallbackRegistry` does not exist.

- [ ] **Step 4: Implement the minimal registry**

Create `RevenueCat/Scripts/CallbackRegistry.cs`:

```csharp
using System;
using System.Collections.Generic;

internal sealed class CallbackRegistry
{
    private sealed class CallbackEntry
    {
        internal readonly Type Type;
        internal readonly object Callback;

        internal CallbackEntry(Type type, object callback)
        {
            Type = type;
            Callback = callback;
        }
    }

    private readonly object _lock = new object();
    private readonly Dictionary<string, CallbackEntry> _callbacks =
        new Dictionary<string, CallbackEntry>();

    internal string Register<T>(T callback) where T : class
    {
        var requestId = Guid.NewGuid().ToString("N");
        lock (_lock)
        {
            _callbacks.Add(requestId, new CallbackEntry(typeof(T), callback));
        }

        return requestId;
    }

    internal bool TryTake<T>(string requestId, out T callback) where T : class
    {
        callback = null;
        if (string.IsNullOrEmpty(requestId))
        {
            return false;
        }

        lock (_lock)
        {
            if (!_callbacks.TryGetValue(requestId, out var entry) || entry.Type != typeof(T))
            {
                return false;
            }

            _callbacks.Remove(requestId);
            callback = (T)entry.Callback;
            return true;
        }
    }

    internal void Clear()
    {
        lock (_lock)
        {
            _callbacks.Clear();
        }
    }
}
```

- [ ] **Step 5: Run the registry tests**

Run the same Unity command from Step 3.

Expected: all five `CallbackRegistryTests` pass.

- [ ] **Step 6: Commit the registry and its tests**

```bash
git add \
  RevenueCat/Scripts/CallbackRegistry.cs \
  RevenueCat/Scripts/CallbackRegistry.cs.meta \
  RevenueCat/Scripts/AssemblyInfo.cs \
  RevenueCat/Scripts/AssemblyInfo.cs.meta \
  IntegrationTests/Assets/CallbackTests.meta \
  IntegrationTests/Assets/CallbackTests/Editor.meta \
  IntegrationTests/Assets/CallbackTests/RevenueCat.CallbackTests.asmdef \
  IntegrationTests/Assets/CallbackTests/RevenueCat.CallbackTests.asmdef.meta \
  IntegrationTests/Assets/CallbackTests/Editor/CallbackRegistryTests.cs \
  IntegrationTests/Assets/CallbackTests/Editor/CallbackRegistryTests.cs.meta
git commit -m "test: add callback request registry"
```

---

### Task 2: Thread Request IDs Through the Managed Wrapper Boundary

**Files:**

- Modify: `RevenueCat/Scripts/PurchasesWrapper.cs`
- Modify: `RevenueCat/Scripts/PurchasesWrapperAndroid.cs`
- Modify: `RevenueCat/Scripts/PurchasesWrapperiOS.cs`
- Modify: `RevenueCat/Scripts/PurchasesWrapperNoop.cs`

**Interfaces:**

- Consumes: request IDs returned by `CallbackRegistry.Register<T>`.
- Produces: an optional final `string requestId = null` parameter on every callback-producing `IPurchasesWrapper` operation.

- [ ] **Step 1: Add request IDs to `IPurchasesWrapper`**

Append `string requestId = null` to these exact methods:

```csharp
void GetStorefront(string requestId = null);
void GetProducts(string[] productIdentifiers, string type = "subs", string requestId = null);
void PurchaseProduct(
    string productIdentifier,
    string type = "subs",
    string oldSku = null,
    Purchases.ProrationMode prorationMode =
        Purchases.ProrationMode.UnknownSubscriptionUpgradeDowngradePolicy,
    bool googleIsPersonalizedPrice = false,
    string presentedOfferingIdentifier = null,
    Purchases.PromotionalOffer discount = null,
    string requestId = null);
void PurchasePackage(
    Purchases.Package packageToPurchase,
    string oldSku = null,
    Purchases.ProrationMode prorationMode =
        Purchases.ProrationMode.UnknownSubscriptionUpgradeDowngradePolicy,
    bool googleIsPersonalizedPrice = false,
    Purchases.PromotionalOffer discount = null,
    string requestId = null);
void PurchaseSubscriptionOption(
    Purchases.SubscriptionOption subscriptionOption,
    Purchases.GoogleProductChangeInfo googleProductChangeInfo = null,
    bool googleIsPersonalizedPrice = false,
    string requestId = null);
void RestorePurchases(string requestId = null);
void LogIn(string appUserId, string requestId = null);
void LogOut(string requestId = null);
void GetCustomerInfo(string requestId = null);
void GetOfferings(string requestId = null);
void GetCurrentOfferingForPlacement(string placementIdentifier, string requestId = null);
void SyncAttributesAndOfferingsIfNeeded(string requestId = null);
void SyncPurchases(string requestId = null);
void CheckTrialOrIntroductoryPriceEligibility(
    string[] productIdentifiers,
    string requestId = null);
void RecordPurchase(string productID, string requestId = null);
void CanMakePayments(Purchases.BillingFeature[] features, string requestId = null);
void GetAmazonLWAConsentStatus(string requestId = null);
void GetPromotionalOffer(
    string productIdentifier,
    string discountIdentifier,
    string requestId = null);
void ParseAsWebPurchaseRedemption(string urlString, string requestId = null);
void RedeemWebPurchase(
    Purchases.WebPurchaseRedemption webPurchaseRedemption,
    string requestId = null);
void GetVirtualCurrencies(string requestId = null);
void GetEligibleWinBackOffersForProduct(
    Purchases.StoreProduct storeProduct,
    string requestId = null);
void GetEligibleWinBackOffersForPackage(
    Purchases.Package package,
    string requestId = null);
void PurchaseProductWithWinBackOffer(
    Purchases.StoreProduct storeProduct,
    Purchases.WinBackOffer winBackOffer,
    string requestId = null);
void PurchasePackageWithWinBackOffer(
    Purchases.Package package,
    Purchases.WinBackOffer winBackOffer,
    string requestId = null);
```

- [ ] **Step 2: Make the no-op wrapper compile against the updated interface**

Append the same optional final parameter to the corresponding methods in `PurchasesWrapperNoop.cs`. Do not make no-op methods invoke callbacks or synthesize responses.

Example:

```csharp
public void GetOfferings(string requestId = null)
{
}

public void PurchasePackage(
    Package packageToPurchase,
    string oldSku = null,
    ProrationMode prorationMode =
        ProrationMode.UnknownSubscriptionUpgradeDowngradePolicy,
    bool googleIsPersonalizedPrice = false,
    Purchases.PromotionalOffer discount = null,
    string requestId = null)
{
}
```

Apply this exact parameter rule to all methods listed in Step 1.

- [ ] **Step 3: Forward IDs from the Android managed wrapper**

Append the parameter to every corresponding `PurchasesWrapperAndroid` method and append `requestId` to its `CallPurchases` arguments.

Use these exact Java call shapes:

| C# method | `CallPurchases` argument list |
|---|---|
| `GetStorefront` | `"getStorefront", requestId` |
| `GetProducts` | `"getProducts", JsonUtility.ToJson(request), type, requestId` |
| `PurchaseProduct`, no `oldSku` | `"purchaseProduct", productIdentifier, type, requestId` |
| `PurchaseProduct`, with `oldSku` | existing six arguments, then `requestId` |
| `PurchasePackage`, no `oldSku` | `"purchasePackage", package.Identifier, contextJson, requestId` |
| `PurchasePackage`, with `oldSku` | existing five arguments, then `requestId` |
| `PurchaseSubscriptionOption` | existing six arguments, then `requestId` |
| `RestorePurchases` | `"restorePurchases", requestId` |
| `LogIn` | `"logIn", appUserId, requestId` |
| `LogOut` | `"logOut", requestId` |
| `GetCustomerInfo` | `"getCustomerInfo", requestId` |
| `GetOfferings` | `"getOfferings", requestId` |
| `GetCurrentOfferingForPlacement` | `"getCurrentOfferingForPlacement", placementIdentifier, requestId` |
| `SyncAttributesAndOfferingsIfNeeded` | `"syncAttributesAndOfferingsIfNeeded", requestId` |
| `SyncPurchases` | `"syncPurchases", requestId` |
| `CheckTrialOrIntroductoryPriceEligibility` | existing JSON request, then `requestId` |
| `CanMakePayments` | existing JSON request, then `requestId` |
| `GetAmazonLWAConsentStatus` | `"getAmazonLWAConsentStatus", requestId` |
| `GetPromotionalOffer` | product ID, discount ID, then `requestId` |
| `ParseAsWebPurchaseRedemption` | URL, then `requestId` |
| `RedeemWebPurchase` | redemption link, then `requestId` |
| `GetVirtualCurrencies` | `"getVirtualCurrencies", requestId` |
| `GetEligibleWinBackOffersForProduct` | product ID, then `requestId` |
| `GetEligibleWinBackOffersForPackage` | package product ID, then `requestId` |
| `PurchaseProductWithWinBackOffer` | product ID, offer ID, then `requestId` |
| `PurchasePackageWithWinBackOffer` | package ID, context JSON, offer ID, then `requestId` |

Keep Android `RecordPurchase` as a no-op, but accept its new optional parameter.

- [ ] **Step 4: Forward IDs from the iOS managed wrapper**

Append `string requestId` to each callback-producing `DllImport` and pass it from the public wrapper method. For example:

```csharp
[DllImport("__Internal")]
private static extern void _RCGetOfferings(string requestId);

public void GetOfferings(string requestId = null)
{
    _RCGetOfferings(requestId);
}

[DllImport("__Internal")]
private static extern void _RCPurchasePackage(
    string packageIdentifier,
    string presentedOfferingContextJSON,
    string signedDiscountTimestamp,
    string requestId);
```

Apply the final request-ID argument to every iOS C function corresponding to Step 1 except:

- `PurchaseSubscriptionOption`, which remains an iOS no-op but accepts the managed optional parameter.
- `GetAmazonLWAConsentStatus`, which remains an iOS no-op but accepts the managed optional parameter.

- [ ] **Step 5: Verify managed compilation**

Run the Unity EditMode command from Task 1.

Expected: all tests pass and there are no `IPurchasesWrapper` implementation errors. Native symbol signatures are not exercised in Editor mode yet.

- [ ] **Step 6: Commit the managed bridge signatures**

```bash
git add \
  RevenueCat/Scripts/PurchasesWrapper.cs \
  RevenueCat/Scripts/PurchasesWrapperAndroid.cs \
  RevenueCat/Scripts/PurchasesWrapperiOS.cs \
  RevenueCat/Scripts/PurchasesWrapperNoop.cs
git commit -m "refactor: pass callback request IDs through wrappers"
```

---

### Task 3: Replace Mutable Callback Slots in `Purchases`

**Files:**

- Modify: `RevenueCat/Scripts/Purchases.cs`
- Create: `IntegrationTests/Assets/CallbackTests/Editor/PurchasesCallbackRoutingTests.cs`
- Create: `IntegrationTests/Assets/CallbackTests/Editor/PurchasesCallbackRoutingTests.cs.meta`

**Interfaces:**

- Consumes: `CallbackRegistry` and request-ID-aware `IPurchasesWrapper`.
- Produces: `Purchases.RegisterCallback<T>`, `Purchases.TryTakeCallback<T>`, and correctly correlated receive handlers.

- [ ] **Step 1: Write failing end-to-end managed routing tests**

Create `IntegrationTests/Assets/CallbackTests/Editor/PurchasesCallbackRoutingTests.cs`:

```csharp
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PurchasesCallbackRoutingTests
{
    private GameObject _gameObject;
    private Purchases _purchases;

    [SetUp]
    public void SetUp()
    {
        _gameObject = new GameObject("PurchasesCallbackRoutingTests");
        _purchases = _gameObject.AddComponent<Purchases>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_gameObject);
    }

    [Test]
    public void StorefrontResponsesInvokeMatchingCallbacksOutOfOrder()
    {
        var received = new List<string>();
        var firstId = _purchases.RegisterCallback<Purchases.GetStorefrontFunc>(
            storefront => received.Add($"first:{storefront.CountryCode}"));
        var secondId = _purchases.RegisterCallback<Purchases.GetStorefrontFunc>(
            storefront => received.Add($"second:{storefront.CountryCode}"));

        _purchases.SendMessage(
            "_receiveStorefront",
            $"{{\"requestId\":\"{secondId}\",\"countryCode\":\"US\"}}");
        _purchases.SendMessage(
            "_receiveStorefront",
            $"{{\"requestId\":\"{firstId}\",\"countryCode\":\"CA\"}}");

        CollectionAssert.AreEqual(
            new[] { "second:US", "first:CA" },
            received);
    }

    [Test]
    public void DuplicateResponseDoesNotInvokeCallbackTwice()
    {
        var calls = 0;
        var requestId = _purchases.RegisterCallback<Purchases.GetStorefrontFunc>(
            _ => calls++);
        var response =
            $"{{\"requestId\":\"{requestId}\",\"countryCode\":\"US\"}}";

        _purchases.SendMessage("_receiveStorefront", response);
        LogAssert.Expect(
            LogType.Warning,
            $"No pending callback found for request ID '{requestId}'.");
        _purchases.SendMessage("_receiveStorefront", response);

        Assert.That(calls, Is.EqualTo(1));
    }
}
```

Create its metadata with GUID `624b47d426cd4d79839d856c18665862` and the `MonoImporter` template from Task 1.

- [ ] **Step 2: Run the routing tests and verify they fail**

Run the Unity EditMode command from Task 1.

Expected: compilation fails because `Purchases.RegisterCallback<T>` does not exist.

- [ ] **Step 3: Add registry helpers to `Purchases`**

Near `_wrapper`, add:

```csharp
private const string CallbackRequestIdKey = "requestId";
private readonly CallbackRegistry _callbackRegistry = new CallbackRegistry();

internal string RegisterCallback<T>(T callback) where T : class
{
    return _callbackRegistry.Register(callback);
}

private bool TryTakeCallback<T>(JSONNode response, out T callback) where T : class
{
    callback = null;
    var requestIdNode = response?[CallbackRequestIdKey];
    var requestId =
        requestIdNode == null || requestIdNode.IsNull
            ? null
            : requestIdNode.Value;

    if (string.IsNullOrEmpty(requestId))
    {
        Debug.LogWarning("Received callback response without a request ID.");
        return false;
    }

    if (!_callbackRegistry.TryTake(requestId, out callback))
    {
        Debug.LogWarning($"No pending callback found for request ID '{requestId}'.");
        return false;
    }

    return true;
}

private void OnDestroy()
{
    _callbackRegistry.Clear();
}
```

If `Purchases` already gains an `OnDestroy` before implementation, merge `Clear()` into it rather than adding a second Unity message method.

- [ ] **Step 4: Register callbacks per invocation**

Delete all private one-shot callback properties, including `StorefrontCallback`, `ProductsCallback`, `MakePurchaseCallback`, and every other `*Callback { get; set; }` used by a completion response. Keep `LogHandler`, because it is a long-lived configured handler.

For every public callback API, replace assignment with registration and pass the returned ID:

```csharp
public void GetOfferings(GetOfferingsFunc callback)
{
    var requestId = RegisterCallback(callback);
    _wrapper.GetOfferings(requestId);
}

public void PurchasePackage(
    Package package,
    MakePurchaseFunc callback,
    string oldSku = null,
    ProrationMode prorationMode =
        ProrationMode.UnknownSubscriptionUpgradeDowngradePolicy,
    bool googleIsPersonalizedPrice = false)
{
    var requestId = RegisterCallback(callback);
    _wrapper.PurchasePackage(
        package,
        oldSku,
        prorationMode,
        googleIsPersonalizedPrice,
        requestId: requestId);
}
```

Apply the same transformation to this complete list:

| Public API | Delegate registered |
|---|---|
| `GetStorefront` | `GetStorefrontFunc` |
| `GetProducts` | `GetProductsFunc` |
| all seven purchase entry points | `MakePurchaseFunc` |
| `RestorePurchases` | `CustomerInfoFunc` |
| `LogIn` | `LogInFunc` |
| `LogOut` | `CustomerInfoFunc` |
| `GetCustomerInfo` | `CustomerInfoFunc` |
| `GetOfferings` | `GetOfferingsFunc` |
| `GetCurrentOfferingForPlacement` | `GetCurrentOfferingForPlacementFunc` |
| `SyncAttributesAndOfferingsIfNeeded` | `SyncAttributesAndOfferingsIfNeededFunc` |
| callback overload of `SyncPurchases` | `CustomerInfoFunc` |
| `CheckTrialOrIntroductoryPriceEligibility` | `CheckTrialOrIntroductoryPriceEligibilityFunc` |
| `RecordPurchase` | `RecordPurchaseFunc` |
| both `CanMakePayments` overloads through the feature-array overload | `CanMakePaymentsFunc` |
| `GetAmazonLWAConsentStatus` | `GetAmazonLWAConsentStatusFunc` |
| `GetPromotionalOffer` | `GetPromotionalOfferFunc` |
| `ParseAsWebPurchaseRedemption` | `ParseAsWebPurchaseRedemptionFunc` |
| `RedeemWebPurchase` | `RedeemWebPurchaseFunc` |
| `GetVirtualCurrencies` | `GetVirtualCurrenciesFunc` |
| `GetEligibleWinBackOffersForProduct` | `GetEligibleWinBackOffersForProductFunc` |
| `GetEligibleWinBackOffersForPackage` | `GetEligibleWinBackOffersForPackageFunc` |
| `PurchaseProductWithWinBackOffer` | `MakePurchaseFunc` |
| `PurchasePackageWithWinBackOffer` | `MakePurchaseFunc` |

- [ ] **Step 5: Resolve response callbacks by ID**

Every one-shot receive method must parse JSON first, call `TryTakeCallback`, and return if the request is unknown or the registered callback is null.

Use this exact shape:

```csharp
private void _getOfferings(string offeringsJson)
{
    Debug.Log("_getOfferings " + offeringsJson);
    var response = JSON.Parse(offeringsJson);
    if (!TryTakeCallback(response, out GetOfferingsFunc callback) || callback == null)
    {
        return;
    }

    if (ResponseHasError(response))
    {
        callback(null, new Error(response["error"]));
    }
    else
    {
        callback(new Offerings(response["offerings"]), null);
    }
}
```

For `_receiveStorefront`, stop comparing the whole payload to `"{}"` because correlated empty results are now `{"requestId":"..."}`:

```csharp
private void _receiveStorefront(string storefrontJson)
{
    Debug.Log("_receiveStorefront " + storefrontJson);
    var response = JSON.Parse(storefrontJson);
    if (!TryTakeCallback(response, out GetStorefrontFunc callback) || callback == null)
    {
        return;
    }

    var countryCode = response?["countryCode"];
    callback(countryCode == null || countryCode.IsNull
        ? null
        : new Storefront(countryCode.Value));
}
```

Apply typed lookup before existing response conversion in every handler:

| Receive method | Delegate type |
|---|---|
| `_receiveStorefront` | `GetStorefrontFunc` |
| `_receiveProducts` | `GetProductsFunc` |
| `_getCustomerInfo` | `CustomerInfoFunc` |
| `_makePurchase` | `MakePurchaseFunc` |
| `_restorePurchases` | `CustomerInfoFunc` |
| `_syncPurchases` | `CustomerInfoFunc` |
| `_logIn` | `LogInFunc` |
| `_logOut` | `CustomerInfoFunc` |
| `_getOfferings` | `GetOfferingsFunc` |
| `_getCurrentOfferingForPlacement` | `GetCurrentOfferingForPlacementFunc` |
| `_syncAttributesAndOfferingsIfNeeded` | `SyncAttributesAndOfferingsIfNeededFunc` |
| `_checkTrialOrIntroductoryPriceEligibility` | `CheckTrialOrIntroductoryPriceEligibilityFunc` |
| `_recordPurchase` | `RecordPurchaseFunc` |
| `_canMakePayments` | `CanMakePaymentsFunc` |
| `_getAmazonLWAConsentStatus` | `GetAmazonLWAConsentStatusFunc` |
| `_getPromotionalOffer` | `GetPromotionalOfferFunc` |
| `_parseAsWebPurchaseRedemption` | `ParseAsWebPurchaseRedemptionFunc` |
| `_redeemWebPurchase` | `RedeemWebPurchaseFunc` |
| `_getVirtualCurrencies` | `GetVirtualCurrenciesFunc` |
| `_getEligibleWinBackOffersForProduct` | `GetEligibleWinBackOffersForProductFunc` |
| `_getEligibleWinBackOffersForPackage` | `GetEligibleWinBackOffersForPackageFunc` |
| `_purchaseProductWithWinBackOffer` | `MakePurchaseFunc` |
| `_purchasePackageWithWinBackOffer` | `MakePurchaseFunc` |

Do not change `_receiveCustomerInfo` or `_handleLog`; neither is a one-shot completion.

- [ ] **Step 6: Run all managed callback tests**

Run the Unity EditMode command from Task 1.

Expected: seven callback tests pass: five registry tests and two routing tests.

- [ ] **Step 7: Commit managed correlation**

```bash
git add \
  RevenueCat/Scripts/Purchases.cs \
  IntegrationTests/Assets/CallbackTests/Editor/PurchasesCallbackRoutingTests.cs \
  IntegrationTests/Assets/CallbackTests/Editor/PurchasesCallbackRoutingTests.cs.meta
git commit -m "fix: correlate Unity callbacks by request ID"
```

---

### Task 4: Correlate Android Native Responses

**Files:**

- Modify: `RevenueCat/Plugins/Android/PurchasesWrapper.java`

**Interfaces:**

- Consumes: final `String requestId` arguments sent by `PurchasesWrapperAndroid`.
- Produces: every one-shot JSON response with a top-level `"requestId"` value.

- [ ] **Step 1: Add request-aware response helpers**

Keep the existing two-argument `sendJSONObject` for broadcasts and logs. Add these helpers and update callback helper signatures:

```java
private static final String REQUEST_ID = "requestId";

static void sendJSONObject(JSONObject object, String method, String requestId) {
    JSONObject response = object != null ? object : new JSONObject();
    try {
        response.put(REQUEST_ID, requestId);
    } catch (JSONException e) {
        logJSONException(e);
        return;
    }
    sendJSONObject(response, method);
}

static void sendEmptyJSONObject(String method, String requestId) {
    sendJSONObject(new JSONObject(), method, requestId);
}

private static void sendError(
        ErrorContainer error,
        String method,
        String requestId
) {
    JSONObject response = new JSONObject();
    try {
        response.put("error", MappersHelpersKt.convertToJson(error.getInfo()));
    } catch (JSONException e) {
        logJSONException(e);
    }
    sendJSONObject(response, method, requestId);
}

private static void sendCustomerInfo(
        Map<String, ?> map,
        String method,
        String requestId
) {
    JSONObject response = new JSONObject();
    try {
        response.put("customerInfo", MappersHelpersKt.convertToJson(map));
    } catch (JSONException e) {
        logJSONException(e);
    }
    sendJSONObject(response, method, requestId);
}

private static void sendErrorPurchase(
        ErrorContainer errorContainer,
        String requestId
) {
    JSONObject response = new JSONObject();
    try {
        response.put(
            "error",
            MappersHelpersKt.convertToJson(errorContainer.getInfo()));
        response.put(
            "userCancelled",
            errorContainer.getInfo().get("userCancelled"));
    } catch (JSONException e) {
        logJSONException(e);
    }
    sendJSONObject(response, MAKE_PURCHASE, requestId);
}
```

Change `getLogInListener` and `getCustomerInfoListener` to accept `requestId`, capture it in their anonymous `OnResult`, and call only the request-aware helper overloads.

- [ ] **Step 2: Add request IDs to Java entry points**

Add a final `String requestId` to all callback-producing Java methods and capture it in completions.

Use these exact overloads for purchase calls made with and without product-change information:

```java
public static void purchaseProduct(
        String productIdentifier,
        String type,
        String requestId
) {
    purchaseProduct(
        productIdentifier,
        type,
        null,
        0,
        false,
        null,
        requestId);
}

public static void purchasePackage(
        String packageIdentifier,
        String presentedOfferingContextJSON,
        String requestId
) {
    purchasePackage(
        packageIdentifier,
        presentedOfferingContextJSON,
        null,
        0,
        false,
        requestId);
}
```

Add `requestId` after the existing parameters on the full `purchaseProduct`, `purchasePackage`, and `purchaseSubscriptionOption` methods.

Apply the same final parameter to:

```text
getStorefront
getProducts
restorePurchases
logIn
logOut
getOfferings
getCurrentOfferingForPlacement
syncAttributesAndOfferingsIfNeeded
getAmazonLWAConsentStatus
getCustomerInfo
syncPurchases
checkTrialOrIntroductoryPriceEligibility
canMakePayments
getPromotionalOffer
parseAsWebPurchaseRedemption
redeemWebPurchase
getVirtualCurrencies
getEligibleWinBackOffersForProduct
getEligibleWinBackOffersForPackage
purchaseProductWithWinBackOffer
purchasePackageWithWinBackOffer
```

Every response or error in those methods must use `sendJSONObject(..., requestId)`, `sendEmptyJSONObject(..., requestId)`, `sendError(..., requestId)`, or `sendErrorPurchase(..., requestId)`.

For `parseAsWebPurchaseRedemption`, use an empty object for an invalid URL:

```java
public static void parseAsWebPurchaseRedemption(
        String urlString,
        String requestId
) {
    JSONObject response = new JSONObject();
    if (CommonKt.isWebPurchaseRedemptionURL(urlString)) {
        try {
            response.put("redemptionLink", urlString);
        } catch (JSONException e) {
            logJSONException(e);
        }
    }
    sendJSONObject(
        response,
        PARSE_AS_WEB_PURCHASE_REDEMPTION,
        requestId);
}
```

- [ ] **Step 3: Verify no one-shot Android response uses the uncorrelated helper**

Run:

```bash
rg -n \
  'send(JSONObject|EmptyJSONObject|Error|ErrorPurchase|CustomerInfo)\\(' \
  RevenueCat/Plugins/Android/PurchasesWrapper.java
```

Expected: `HANDLE_LOG` and updated-customer-info broadcasting may use the legacy helper; every method listed in Step 2 passes `requestId`.

- [ ] **Step 4: Import and compile for Android**

Run:

```bash
"/Applications/Unity/Hub/Editor/6000.2.6f2/Unity.app/Contents/MacOS/Unity" \
  -batchmode -nographics -quit \
  -projectPath "$PWD/IntegrationTests" \
  -buildTarget Android \
  -logFile -
```

Expected: Unity exits `0` with no C# or Java bridge signature errors during import. A full Android player build remains part of final verification.

- [ ] **Step 5: Commit Android correlation**

```bash
git add RevenueCat/Plugins/Android/PurchasesWrapper.java
git commit -m "fix(android): return Unity callback request IDs"
```

---

### Task 5: Correlate iOS Native Responses

**Files:**

- Modify: `RevenueCat/Plugins/iOS/PurchasesUnityHelper.m`

**Interfaces:**

- Consumes: final `const char *requestId` arguments from `PurchasesWrapperiOS`.
- Produces: every one-shot JSON response with a top-level `"requestId"` value.

- [ ] **Step 1: Add an Objective-C request-aware response helper**

Keep the existing `sendJSONObject:toMethod:` for broadcasts and logs. Add:

```objective-c
static NSString * const RCCallbackRequestIdKey = @"requestId";

- (void)sendJSONObject:(nullable NSDictionary *)jsonObject
             requestId:(NSString *)requestId
              toMethod:(NSString *)methodName {
    NSMutableDictionary *response = jsonObject
        ? [jsonObject mutableCopy]
        : [NSMutableDictionary new];
    response[RCCallbackRequestIdKey] = requestId;
    [self sendJSONObject:response toMethod:methodName];
}
```

Replace `sendEmptyResponseToMethod:` on callback paths with:

```objective-c
[self sendJSONObject:nil requestId:requestId toMethod:methodName];
```

Change the completion-block factories to capture request IDs:

```objective-c
- (void (^)(NSDictionary *, RCErrorContainer *))
    getCustomerInfoCompletionBlockFor:(NSString *)method
                            requestId:(NSString *)requestId;

- (void (^)(NSDictionary *, RCErrorContainer *))
    getLogInCompletionBlockForMethod:(NSString *)method
                           requestId:(NSString *)requestId;
```

Both blocks must call `sendJSONObject:requestId:toMethod:`.

- [ ] **Step 2: Add request IDs to helper selectors**

Add a final `requestId:(NSString *)requestId` selector component to every one-shot helper:

```text
getProducts:type:requestId:
getStorefrontWithRequestId:
purchaseProduct:signedDiscountTimestamp:requestId:
purchasePackage:presentedOfferingContext:signedDiscountTimestamp:requestId:
restorePurchasesWithRequestId:
syncPurchasesWithRequestId:
logInWithAppUserID:requestId:
logOutWithRequestId:
getOfferingsWithRequestId:
getCurrentOfferingForPlacement:requestId:
syncAttributesAndOfferingsIfNeededWithRequestId:
getCustomerInfoWithRequestId:
checkTrialOrIntroductoryPriceEligibility:requestId:
recordPurchase:requestId:
canMakePaymentsWithFeatures:requestId:
promotionalOfferForProductIdentifier:discount:requestId:
parseAsWebPurchaseRedemption:requestId:
redeemWebPurchase:requestId:
getVirtualCurrenciesWithRequestId:
getEligibleWinBackOffersForProduct:requestId:
getEligibleWinBackOffersForPackage:requestId:
purchaseProductWithWinBackOffer:winBackOfferIdentifier:requestId:
purchasePackageWithWinBackOffer:presentedOfferingContextJson:winBackOfferIdentifier:requestId:
```

Capture the ID in every completion and call only `sendJSONObject:requestId:toMethod:`. Leave `purchases:receivedUpdatedCustomerInfo:` and `setLogHandler` on the uncorrelated helper because they are broadcasts.

- [ ] **Step 3: Add request IDs to C ABI functions**

Append `const char *requestId` to every callback-producing exported function and pass `convertCString(requestId)` into the selector from Step 2.

Example:

```objective-c
void _RCGetOfferings(const char *requestId) {
    [_RCUnityHelperShared()
        getOfferingsWithRequestId:convertCString(requestId)];
}

void _RCPurchasePackage(
        const char *packageIdentifier,
        const char *presentedOfferingContextJSON,
        const char *signedDiscountTimestamp,
        const char *requestId) {
    // Keep the existing context parsing.
    [_RCUnityHelperShared()
        purchasePackage:convertCString(packageIdentifier)
        presentedOfferingContext:presentedOfferingContext
        signedDiscountTimestamp:convertCString(signedDiscountTimestamp)
        requestId:convertCString(requestId)];
}
```

Apply this to:

```text
_RCGetStorefront
_RCGetProducts
_RCPurchaseProduct
_RCPurchasePackage
_RCRestorePurchases
_RCSyncPurchases
_RCLogIn
_RCLogOut
_RCGetOfferings
_RCGetCurrentOfferingForPlacement
_RCSyncAttributesAndOfferingsIfNeeded
_RCRecordPurchase
_RCGetCustomerInfo
_RCCheckTrialOrIntroductoryPriceEligibility
_RCCanMakePayments
_RCGetPromotionalOffer
_RCParseAsWebPurchaseRedemption
_RCRedeemWebPurchase
_RCGetVirtualCurrencies
_RCGetEligibleWinBackOffersForProduct
_RCGetEligibleWinBackOffersForPackage
_RCPurchaseProductWithWinBackOffer
_RCPurchasePackageWithWinBackOffer
```

- [ ] **Step 4: Verify no one-shot iOS response uses the uncorrelated helper**

Run:

```bash
rg -n 'sendJSONObject:.*toMethod:' \
  RevenueCat/Plugins/iOS/PurchasesUnityHelper.m
```

Expected: callback paths use the three-argument request-aware selector. Only log handling and updated-customer-info broadcasting use `sendJSONObject:toMethod:`.

- [ ] **Step 5: Import and compile managed iOS bindings**

Run:

```bash
"/Applications/Unity/Hub/Editor/6000.2.6f2/Unity.app/Contents/MacOS/Unity" \
  -batchmode -nographics -quit \
  -projectPath "$PWD/IntegrationTests" \
  -buildTarget iOS \
  -logFile -
```

Expected: Unity exits `0` with no managed binding or Objective-C import errors. Confirm the Objective-C symbols in a generated Xcode build during final verification.

- [ ] **Step 6: Commit iOS correlation**

```bash
git add RevenueCat/Plugins/iOS/PurchasesUnityHelper.m
git commit -m "fix(ios): return Unity callback request IDs"
```

---

### Task 6: Full Regression and Package Verification

**Files:**

- Modify only if verification exposes a defect in a file already listed above.

**Interfaces:**

- Consumes: the complete managed, Android, and iOS request-correlation path.
- Produces: test evidence that concurrent callbacks remain matched and the Unity packages still build.

- [ ] **Step 1: Run all EditMode tests**

Run:

```bash
"/Applications/Unity/Hub/Editor/6000.2.6f2/Unity.app/Contents/MacOS/Unity" \
  -batchmode -nographics -quit \
  -projectPath "$PWD/IntegrationTests" \
  -runTests -testPlatform EditMode \
  -testResults "$PWD/.context/callback-tests.xml" \
  -logFile -
```

Expected: all callback tests pass with zero failed tests.

- [ ] **Step 2: Verify Unity asset metadata**

Run:

```bash
./scripts/check-meta-files.sh
```

Expected:

```text
✅ All asset files have corresponding .meta files.
```

- [ ] **Step 3: Build both distributable packages**

Run:

```bash
./scripts/create-unity-package.sh \
  -u /Applications/Unity/Hub/Editor/6000.2.6f2/Unity.app/Contents/MacOS/Unity
```

Expected: exit `0`, with `Purchases.unitypackage` and `PurchasesUI.unitypackage` created.

- [ ] **Step 4: Perform device-level concurrency smoke tests**

On one Android API 21+ device and one iOS 13+ device, call `GetStorefront` twice and `GetOfferings` twice without awaiting either result. Add distinct markers to each callback, force the second native request to complete first using network conditioning or a debugger breakpoint, and verify:

```text
second response -> second callback exactly once
first response  -> first callback exactly once
```

Repeat with:

```text
GetCustomerInfo + GetCustomerInfo
GetEligibleWinBackOffersForProduct + GetEligibleWinBackOffersForProduct
PurchaseProduct + PurchasePackage
```

For purchase overlap, the stores may reject one operation; verify that each store result or error reaches the callback belonging to the request that produced it.

- [ ] **Step 5: Inspect the final diff for public API stability**

Run:

```bash
git diff --check origin/main...
git diff --stat origin/main...
git diff origin/main... -- RevenueCat/Scripts/Purchases.cs
git diff origin/main... -- RevenueCat/Scripts/PurchasesWrapper.cs
```

Expected:

- No whitespace errors.
- Public `Purchases` method signatures are byte-for-byte unchanged.
- The mutable one-shot callback properties are gone.
- `LogHandler`, `listener`, paywall presentation, and Customer Center presentation are unchanged.

- [ ] **Step 6: Remove local test-project symlinks if package creation has not already removed them**

Only remove the two exact symlinks created in Task 1:

```bash
if [ -L "$PWD/IntegrationTests/Assets/RevenueCat" ]; then
  unlink "$PWD/IntegrationTests/Assets/RevenueCat"
fi
if [ -L "$PWD/IntegrationTests/Assets/RevenueCatUI" ]; then
  unlink "$PWD/IntegrationTests/Assets/RevenueCatUI"
fi
```

Expected: both ignored symlinks are removed; repository source directories remain untouched.

- [ ] **Step 7: Commit any verification-only corrections**

If verification required changes:

```bash
git add \
  RevenueCat \
  IntegrationTests/Assets/CallbackTests
git commit -m "test: verify concurrent Unity callbacks"
```

If verification required no changes, do not create an empty commit.

---

## Deferred Follow-Ups

These are intentionally separate because they solve different callback semantics and can be reviewed independently:

- Add a multi-subscriber `CustomerInfoUpdated` C# event while preserving the serialized `listener` field.
- Fix `_handleLog` to guard `LogHandler` rather than `listener`.
- Add task-based `GetOfferingsAsync`, `GetCustomerInfoAsync`, and related APIs on top of the callback registry.
- Decide whether purchase APIs should reject a second in-flight purchase before entering the native store SDK.
