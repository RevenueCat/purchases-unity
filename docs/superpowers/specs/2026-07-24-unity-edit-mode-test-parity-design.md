# Unity Edit Mode Test Parity Design

## Goal

Extend the Unity SDK's executable Edit Mode coverage for behaviors already exercised by purchases-flutter and react-native-purchases, without changing the public API or adding concurrency coverage.

## Scope

Add tests in `IntegrationTests/Assets/Tests/EditMode` for five areas:

1. Purchase forwarding and response parsing: discounted purchases, subscription options, win-back purchases, and success/cancellation/error results.
2. Single-request callback flows: customer info, login, logout, restore, sync, offerings, eligibility, storefront, and promotional offers.
3. Tracking: custom paywall parameter resolution and ad-tracker forwarding/JSON serialization.
4. Models: representative full/minimal payload parsing, optional fields, and web-purchase redemption result variants.
5. Low-risk wrapper pass-through calls: message display, locale override, cache calls, attributes, and Amazon forwarding.

## Non-goals

- Do not add tests for overlapping requests or concurrent callback handling; that work belongs to the separate concurrency branch.
- Do not modify production SDK behavior unless a new test exposes a clear defect that cannot be represented accurately otherwise.
- Do not add or change public APIs, platform requirements, or dependencies.

## Design

Tests remain Edit Mode NUnit tests and use the existing `PurchasesWrapperSpy` to verify calls from `Purchases` to `IPurchasesWrapper`. Native response receivers are invoked through the existing reflection helper, which validates public-call argument forwarding and callback parsing in the same test.

The suite will be split by responsibility instead of growing `PurchasesCallTests.cs` into a catch-all file:

- `PurchaseCallTests.cs` for purchase variants and purchase result responses.
- `CallbackResponseTests.cs` for non-purchase callback methods.
- `TrackingTests.cs` for custom paywall and ad tracker behavior.
- `WrapperPassthroughTests.cs` for simple forwarding/serialization calls.
- `JsonModelTests.cs` gains representative nested model and redemption-result cases.

All tests create a `Purchases` component and inject `PurchasesWrapperSpy`, preserving the existing test setup. Callback tests execute one request per callback slot; they may check that a duplicate native response does not invoke a completed callback again, but they do not issue two outstanding requests of the same kind.

## Verification

Run the Unity 6000.2 Edit Mode suite after importing the packages as CircleCI does, and run `scripts/check-meta-files.sh`. Every new Unity C# asset receives a matching `.meta` file.
