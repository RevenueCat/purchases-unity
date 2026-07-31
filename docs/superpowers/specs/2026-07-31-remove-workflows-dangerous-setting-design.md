# Remove Workflows Dangerous Setting Design

## Context

Purchases Hybrid Common 18.28.0 removes the internal native APIs used to enable RevenueCat Workflows through `DangerousSettings`. Unity added its corresponding bridge and configuration surface in purchases-unity PR #996. The Unity removal must land before the PHC 18.28.0 dependency bump in purchases-unity PR #1029 can compile and merge.

The workflows members were documented as internal-only and slated for removal. Source compatibility for those members is therefore intentionally not preserved.

## Approach

Apply a semantic revert of PR #996 against the current tree. This avoids a literal Git revert overwriting later changes while restoring the pre-workflows behavior: Unity passes only `AutoSyncPurchases` through `DangerousSettings`, and each native bridge constructs the ordinary native dangerous-settings object.

## Changes

- Remove `UseWorkflows`, the two-argument constructor, its JSON key, and its string representation from `RevenueCat/Scripts/DangerousSettings.cs`.
- Remove the serialized workflows field from `RevenueCat/Scripts/Purchases.cs` and construct `DangerousSettings` with only `autoSyncPurchases`.
- Remove workflows JSON parsing and workflows-specific construction from the Android and iOS bridges.
- Delete the iOS Swift workflows factory, restore the pre-#996 empty `PurchasesDummy.swift` asset, and rename the existing Unity metadata back to `PurchasesDummy.swift.meta` while preserving its GUID.
- Remove the workflows runtime-setup option, logging, and serialized scene data from Subtester while retaining runtime setup itself.
- Update edit-mode tests added after #996 so configuration forwarding still verifies `AutoSyncPurchases` and verifies that no `UseWorkflows` key is serialized.
- Leave historical changelog entries unchanged.

## Compatibility and Behavior

`DangerousSettings(bool autoSyncPurchases)` and `SetDangerousSettings` remain unchanged. The internal-only `UseWorkflows` field and `DangerousSettings(bool, bool)` constructor are removed, which is an accepted source-breaking change. No other public API, default, platform minimum, or purchase behavior changes.

## Verification

Use a red-green regression test around serialized dangerous settings: before implementation, assert that configuration forwarding omits `UseWorkflows` and observe the assertion fail against the current serializer; after implementation, rerun it and the surrounding edit-mode suite. Also inspect the final diff, search active source and serialized assets for workflows-specific references, and run the repository's available native/package validation checks without requiring the PHC 18.28.0 bump to be present on this branch.
