# Remove Workflows Dangerous Setting Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Remove the internal Unity workflows dangerous setting so purchases-unity can compile against Purchases Hybrid Common 18.28.0.

**Architecture:** Semantically revert the workflows-specific portions of PR #996 while preserving later edits and the Subtester runtime-setup harness. Dangerous settings continue to cross the C#/native boundary as JSON, but the payload and native construction return to the single supported `AutoSyncPurchases` value.

**Tech Stack:** C#, NUnit/Unity Test Framework, Java, Objective-C, Swift plugin assets, Unity YAML serialization

## Global Constraints

- Preserve `DangerousSettings(bool autoSyncPurchases)` and `SetDangerousSettings` behavior.
- The internal-only `UseWorkflows` field and two-argument constructor may be removed even though this is source-breaking.
- Do not change other public APIs, defaults, minimum Unity 2021.3, iOS 13.0, Android API 21, or Play Billing 8.0.0 requirements.
- Keep the diff limited to the workflows setting and its test/sample infrastructure; retain historical changelog entries.

---

### Task 1: Lock in the workflows-free C# payload

**Files:**
- Modify: `IntegrationTests/Assets/Tests/EditMode/PurchasesCallTests.cs:38-59`
- Modify: `RevenueCat/Scripts/DangerousSettings.cs:1-50`
- Modify: `RevenueCat/Scripts/Purchases.cs:48-52,153`

**Interfaces:**
- Consumes: `Purchases.DangerousSettings(bool autoSyncPurchases)` and `DangerousSettings.Serialize()`
- Produces: dangerous-settings JSON containing `AutoSyncPurchases` and no `UseWorkflows` key

- [ ] **Step 1: Write the failing serialization assertion**

Change the setup in `ConfigureForwardsEveryConfigurationValue` to the surviving constructor and assert key absence:

```csharp
.SetDangerousSettings(new Purchases.DangerousSettings(false))
```

```csharp
Assert.That(dangerousSettings["AutoSyncPurchases"].AsBool, Is.False);
Assert.That(dangerousSettings.HasKey("UseWorkflows"), Is.False);
```

- [ ] **Step 2: Export/import the current packages and verify the test fails**

Export and import both packages, restoring hidden test sources through the shell trap:

```bash
UNITY_BIN=/Applications/Unity/Hub/Editor/6000.2.6f2/Unity.app/Contents/MacOS/Unity
./scripts/create-unity-package.sh -u "$UNITY_BIN" -v
(
  set -euo pipefail
  restore_test_sources() {
    test ! -f IntegrationTests/Assets/Editor/CIEditorScript.cs.break || mv IntegrationTests/Assets/Editor/CIEditorScript.cs.break IntegrationTests/Assets/Editor/CIEditorScript.cs
    test ! -f IntegrationTests/Assets/Main.cs.break || mv IntegrationTests/Assets/Main.cs.break IntegrationTests/Assets/Main.cs
    find IntegrationTests/Assets/APITests IntegrationTests/Assets/Tests -type f -name "*.break" \
      -exec sh -c 'for file do mv "$file" "${file%.break}"; done' sh {} +
  }
  trap restore_test_sources EXIT
  mv IntegrationTests/Assets/Editor/CIEditorScript.cs IntegrationTests/Assets/Editor/CIEditorScript.cs.break
  mv IntegrationTests/Assets/Main.cs IntegrationTests/Assets/Main.cs.break
  find IntegrationTests/Assets/APITests IntegrationTests/Assets/Tests \
    -type f \( -name "*.cs" -o -name "*.asmdef" \) \
    -exec sh -c 'for file do mv "$file" "$file.break"; done' sh {} +
  "$UNITY_BIN" -projectPath IntegrationTests -quit -batchmode -nographics \
    -disable-assembly-updater -importPackage ../Purchases.unitypackage -logFile .context/import-purchases-red.log
  "$UNITY_BIN" -projectPath IntegrationTests -quit -batchmode -nographics \
    -disable-assembly-updater -importPackage ../PurchasesUI.unitypackage -logFile .context/import-purchases-ui-red.log
)
```

Then run:

```bash
/Applications/Unity/Hub/Editor/6000.2.6f2/Unity.app/Contents/MacOS/Unity \
  -projectPath IntegrationTests -quit -batchmode -nographics \
  -disable-assembly-updater -runTests -testPlatform editmode \
  -testFilter RevenueCat.Tests.PurchasesCallTests.ConfigureForwardsEveryConfigurationValue \
  -testResults .context/workflows-red.xml -logFile .context/workflows-red.log
```

Expected: FAIL because the serialized object still contains `UseWorkflows`.

- [ ] **Step 3: Remove the C# workflows surface**

Reduce `DangerousSettings` to the supported state and serialization:

```csharp
public readonly bool AutoSyncPurchases;

public DangerousSettings(bool autoSyncPurchases)
{
    AutoSyncPurchases = autoSyncPurchases;
}

public JSONNode Serialize()
{
    var n = new JSONObject();
    n["AutoSyncPurchases"] = AutoSyncPurchases;
    return n;
}

public override string ToString()
{
    return $"{nameof(AutoSyncPurchases)}: {AutoSyncPurchases}";
}
```

Remove the unused `System.ComponentModel` import. Remove `experimentalUseWorkflows` from `Purchases` and construct settings with:

```csharp
var dangerousSettings = new DangerousSettings(autoSyncPurchases);
```

- [ ] **Step 4: Re-export/import and verify the focused test passes**

Export and import the packages after the production change:

```bash
UNITY_BIN=/Applications/Unity/Hub/Editor/6000.2.6f2/Unity.app/Contents/MacOS/Unity
./scripts/create-unity-package.sh -u "$UNITY_BIN" -v
(
  set -euo pipefail
  restore_test_sources() {
    test ! -f IntegrationTests/Assets/Editor/CIEditorScript.cs.break || mv IntegrationTests/Assets/Editor/CIEditorScript.cs.break IntegrationTests/Assets/Editor/CIEditorScript.cs
    test ! -f IntegrationTests/Assets/Main.cs.break || mv IntegrationTests/Assets/Main.cs.break IntegrationTests/Assets/Main.cs
    find IntegrationTests/Assets/APITests IntegrationTests/Assets/Tests -type f -name "*.break" \
      -exec sh -c 'for file do mv "$file" "${file%.break}"; done' sh {} +
  }
  trap restore_test_sources EXIT
  mv IntegrationTests/Assets/Editor/CIEditorScript.cs IntegrationTests/Assets/Editor/CIEditorScript.cs.break
  mv IntegrationTests/Assets/Main.cs IntegrationTests/Assets/Main.cs.break
  find IntegrationTests/Assets/APITests IntegrationTests/Assets/Tests \
    -type f \( -name "*.cs" -o -name "*.asmdef" \) \
    -exec sh -c 'for file do mv "$file" "$file.break"; done' sh {} +
  "$UNITY_BIN" -projectPath IntegrationTests -quit -batchmode -nographics \
    -disable-assembly-updater -importPackage ../Purchases.unitypackage -logFile .context/import-purchases-green.log
  "$UNITY_BIN" -projectPath IntegrationTests -quit -batchmode -nographics \
    -disable-assembly-updater -importPackage ../PurchasesUI.unitypackage -logFile .context/import-purchases-ui-green.log
)
/Applications/Unity/Hub/Editor/6000.2.6f2/Unity.app/Contents/MacOS/Unity \
  -projectPath IntegrationTests -quit -batchmode -nographics \
  -disable-assembly-updater -runTests -testPlatform editmode \
  -testFilter RevenueCat.Tests.PurchasesCallTests.ConfigureForwardsEveryConfigurationValue \
  -testResults .context/workflows-green.xml -logFile .context/workflows-green.log
```

Expected: PASS with one executed test and zero failures.

- [ ] **Step 5: Commit the C# behavior and regression test**

```bash
git add RevenueCat/Scripts/DangerousSettings.cs RevenueCat/Scripts/Purchases.cs IntegrationTests/Assets/Tests/EditMode/PurchasesCallTests.cs
git commit -m "fix: remove workflows dangerous setting"
```

---

### Task 2: Remove workflows-specific native and Subtester plumbing

**Files:**
- Modify: `RevenueCat/Plugins/Android/PurchasesWrapper.java:932-943`
- Modify: `RevenueCat/Plugins/iOS/PurchasesUnityHelper.m:10-20,104-113`
- Delete: `RevenueCat/Plugins/iOS/RCPurchasesUnityDangerousSettingsFactory.swift`
- Rename: `RevenueCat/Plugins/iOS/RCPurchasesUnityDangerousSettingsFactory.swift.meta` to `RevenueCat/Plugins/iOS/PurchasesDummy.swift.meta`
- Create: `RevenueCat/Plugins/iOS/PurchasesDummy.swift`
- Modify: `Subtester/Assets/Tester/Scripts/TesterApp.cs:22-24,103-110`
- Modify: `Subtester/Assets/Scenes/Main.unity:330`

**Interfaces:**
- Consumes: dangerous-settings JSON containing `AutoSyncPurchases`
- Produces: Android `new DangerousSettings(autoSyncPurchases)` and iOS `[[RCDangerousSettings alloc] initWithAutoSyncPurchases:autoSyncPurchases]`

- [ ] **Step 1: Simplify Android dangerous-settings construction**

Replace workflows parsing and branching with:

```java
boolean autoSyncPurchases = jsonObject.getBoolean("AutoSyncPurchases");
dangerousSettings = new DangerousSettings(autoSyncPurchases);
```

- [ ] **Step 2: Restore direct Objective-C dangerous-settings construction**

Remove the generated Swift-header import block and replace the workflows factory call with:

```objective-c
BOOL autoSyncPurchases = dangerousSettingsDict[@"AutoSyncPurchases"];
dangerousSettings = [[RCDangerousSettings alloc] initWithAutoSyncPurchases:autoSyncPurchases];
```

- [ ] **Step 3: Restore the pre-#996 iOS Swift asset layout**

Delete `RCPurchasesUnityDangerousSettingsFactory.swift`, create the empty `PurchasesDummy.swift`, and rename its `.meta` file back to `PurchasesDummy.swift.meta`. Preserve GUID `0d624db8e887f46739d8501cf2a765c9` and all importer settings.

- [ ] **Step 4: Remove workflows from the retained Subtester runtime setup**

Remove the `useWorkflows` field. Let the builder use its default dangerous settings and make the logs generic:

```csharp
var configuration = Purchases.PurchasesConfiguration.Builder.Init(apiKey)
    .SetAppUserId(string.IsNullOrEmpty(_purchases.appUserID) ? null : _purchases.appUserID)
    .Build();

_logConsole?.Log("[RuntimeSetup] Configuring at runtime");
Debug.Log("[RuntimeSetup] Configuring Purchases at runtime.");
```

Remove only the `useWorkflows: 0` entry from the `TesterApp` component in `Main.unity`; retain `configureAtRuntime` and unrelated scene values.

- [ ] **Step 5: Verify active source has no workflows bridge references**

Run:

```bash
if rg -n "UseWorkflows|useWorkflows|experimentalUseWorkflows|forWorkflows|RCPurchasesUnityDangerousSettingsFactory" \
  RevenueCat Subtester IntegrationTests/Assets/Tests; then
  exit 1
fi
```

Expected: exit 0 with no matches. Historical `CHANGELOG.md` matches are intentionally outside this check.

- [ ] **Step 6: Commit native and sample cleanup**

```bash
git add RevenueCat/Plugins/Android/PurchasesWrapper.java RevenueCat/Plugins/iOS \
  Subtester/Assets/Tester/Scripts/TesterApp.cs Subtester/Assets/Scenes/Main.unity
git commit -m "fix: remove workflows native bridge"
```

---

### Task 3: Run full verification

**Files:**
- Verify: all files changed by Tasks 1 and 2

**Interfaces:**
- Consumes: the completed semantic revert
- Produces: evidence that Unity tests pass, serialized/native references are gone, and the patch is clean against `origin/main`

- [ ] **Step 1: Export and import the final packages**

Run:

```bash
./scripts/create-unity-package.sh \
  -u /Applications/Unity/Hub/Editor/6000.2.6f2/Unity.app/Contents/MacOS/Unity -v
```

Import `Purchases.unitypackage` and `PurchasesUI.unitypackage` into `IntegrationTests`:

```bash
UNITY_BIN=/Applications/Unity/Hub/Editor/6000.2.6f2/Unity.app/Contents/MacOS/Unity
(
  set -euo pipefail
  restore_test_sources() {
    test ! -f IntegrationTests/Assets/Editor/CIEditorScript.cs.break || mv IntegrationTests/Assets/Editor/CIEditorScript.cs.break IntegrationTests/Assets/Editor/CIEditorScript.cs
    test ! -f IntegrationTests/Assets/Main.cs.break || mv IntegrationTests/Assets/Main.cs.break IntegrationTests/Assets/Main.cs
    find IntegrationTests/Assets/APITests IntegrationTests/Assets/Tests -type f -name "*.break" \
      -exec sh -c 'for file do mv "$file" "${file%.break}"; done' sh {} +
  }
  trap restore_test_sources EXIT
  mv IntegrationTests/Assets/Editor/CIEditorScript.cs IntegrationTests/Assets/Editor/CIEditorScript.cs.break
  mv IntegrationTests/Assets/Main.cs IntegrationTests/Assets/Main.cs.break
  find IntegrationTests/Assets/APITests IntegrationTests/Assets/Tests \
    -type f \( -name "*.cs" -o -name "*.asmdef" \) \
    -exec sh -c 'for file do mv "$file" "$file.break"; done' sh {} +
  "$UNITY_BIN" -projectPath IntegrationTests -quit -batchmode -nographics \
    -disable-assembly-updater -importPackage ../Purchases.unitypackage -logFile .context/import-purchases-final.log
  "$UNITY_BIN" -projectPath IntegrationTests -quit -batchmode -nographics \
    -disable-assembly-updater -importPackage ../PurchasesUI.unitypackage -logFile .context/import-purchases-ui-final.log
)
```

- [ ] **Step 2: Run the complete Edit Mode test suite**

```bash
/Applications/Unity/Hub/Editor/6000.2.6f2/Unity.app/Contents/MacOS/Unity \
  -projectPath IntegrationTests -quit -batchmode -nographics \
  -disable-assembly-updater -runTests -testPlatform editmode \
  -testResults .context/editmode-results.xml -logFile .context/editmode.log
```

Expected: Unity exits 0 and the results contain zero failures.

- [ ] **Step 3: Verify repository invariants and patch hygiene**

Run:

```bash
if rg -n "UseWorkflows|useWorkflows|experimentalUseWorkflows|forWorkflows|RCPurchasesUnityDangerousSettingsFactory" \
  RevenueCat Subtester IntegrationTests/Assets/Tests; then
  exit 1
fi
git diff --check origin/main...
git status --short
git diff --stat origin/main...
git diff origin/main... -- RevenueCat Subtester IntegrationTests/Assets/Tests
```

Expected: the source scan and whitespace check exit 0; the diff contains only the design/plan documentation and the semantic revert described above.
