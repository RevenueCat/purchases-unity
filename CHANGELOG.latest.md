## RevenueCat SDK
### 📦 Dependency Updates
* [AUTOMATIC BUMP] Updates purchases-hybrid-common to 18.29.0 (#1033) via RevenueCat Git Bot (@RCGitBot)
  * [Android 10.16.0](https://github.com/RevenueCat/purchases-android/releases/tag/10.16.0)
  * [iOS 5.83.0](https://github.com/RevenueCat/purchases-ios/releases/tag/5.83.0)

## RevenueCatUI SDK
### ✨ New Features
* Enables support for multipage paywalls
* Add onWebCheckoutOpened and onUrlOpened callbacks to PaywallListener (#1016) via Álvaro Brey (@AlvaroBrey)

### 🔄 Other Changes
* Skip test cases list in maestro tests using launch arguments (#897)

## Summary
- Pass `e2e_test_flow` as a Maestro `launchApp` argument so the app
navigates directly to the target test case screen, bypassing the Test
Cases list
- On iOS, uses a native Objective-C++ plugin (`LaunchArgs.mm`) to read
from UserDefaults via `DllImport`
- On Android, uses `AndroidJavaObject` to read from the activity's
intent extras
- Makes maestro tests faster by skipping the list navigation step
- The Test Cases list is preserved for manual/local usage

## Related PRs
- https://github.com/RevenueCat/react-native-purchases/pull/1722
- https://github.com/RevenueCat/purchases-kmp/pull/796
- https://github.com/RevenueCat/purchases-flutter/pull/1714
- https://github.com/RevenueCat/purchases-capacitor/pull/757
- https://github.com/RevenueCat/cordova-plugin-purchases/pull/919

Follows the same pattern as the iOS SDK's maestro app
(`purchases-ios/Examples/rc-maestro`).

<!-- CURSOR_SUMMARY -->
---

> [!NOTE]
> **Low Risk**
> Changes are confined to the Maestro e2e test app and YAML; production
Unity SDK code is untouched.
> 
> **Overview**
> Maestro can pass **`e2e_test_flow`** at launch so the Unity test app
opens the right screen instead of the Test Cases list, which shortens
automated flows while manual runs still use the list.
> 
> **MaestroTestApp** now reads that value on startup (iOS via a new
**`LaunchArgs.mm`** / `GetLaunchTestFlow` from `NSUserDefaults`; Android
from the activity intent extra) and uses a **`TestFlowScreenMap`** to
jump straight to the mapped screen (e.g. **`purchase_through_paywall`**
→ purchase screen). Unknown or missing values still show Test Cases.
> 
> The **`purchase_through_paywall`** Maestro YAML launches with
**`e2e_test_flow: purchase_through_paywall`** and waits on
**`Entitlements: none`** (30s), dropping the steps that opened the app
and tapped through the list.
> 
> <sup>Reviewed by [Cursor Bugbot](https://cursor.com/bugbot) for commit
56cae3bf756171ff89c4611570fa703e84727818. Bugbot is set up for automated
code reviews on this repo. Configure
[here](https://www.cursor.com/dashboard/bugbot).</sup>
<!-- /CURSOR_SUMMARY -->

---------

Co-authored-by: Cursor <cursoragent@cursor.com> via Antonio Pallares
* Add CircleCI job for maestro E2E tests (#838)

## Summary

Runs the Maestro flows on CI for iOS and Android, on the
`maestro_e2e_tests` schedule and in `deploy-check`.

- Unity builds run in Linux GameCI containers, leaving macOS to do only
Xcode, the simulator and Maestro. Building Unity on macOS stalled for
~24 minutes in audio initialisation.
- Swift package resource bundles are copied into the `.app`: Xcode
builds them but embeds them nowhere, and `RevenueCatUI` traps on the
missing bundle as a paywall opens.
- API key injection is a shared command that fails when the placeholder
survives, rather than testing with a bogus key.
- Both jobs store diagnostics on failure: crash reports on iOS, logcat
on Android.

Depends on #837.

<!-- CURSOR_SUMMARY -->
---

> [!NOTE]
> **Medium Risk**
> Changes release gating and adds flaky-prone emulator/simulator E2E
infrastructure, but does not alter SDK runtime code—risk is mainly CI
reliability and blocking releases on external test-store flows.
> 
> **Overview**
> Adds **Maestro end-to-end** coverage on CircleCI for the Unity
`MaestroTestApp`, wired into **`deploy-check`** (release approval now
waits on both Maestro jobs) and into **scheduled** `maestro_e2e_tests`
workflows for iOS and Android.
> 
> **Build vs run split:** Unity Android/iOS exports run on **GameCI
Linux** executors; macOS/Android machine jobs only
Xcode/simulator/emulator work plus Maestro. A shared
**`replace-maestro-api-key`** step injects the production test-store key
and **fails the job** if the placeholder remains.
> 
> **Fastlane:** New lanes build the Unity-generated Xcode project for
the simulator, **copy Swift package resource bundles into**
`MaestroTestApp.app` (needed so RevenueCatUI paywalls don’t crash),
install the app/APK, and invoke existing `run_maestro_e2e_tests` against
`e2e-tests/maestro/`. Android uses the **CircleCI android orb** for
AVD/emulator setup.
> 
> **Failure artifacts:** iOS captures simulator crash reports and app
logs; Android dumps logcat. Maestro steps use a 15-minute no-output
timeout.
> 
> <sup>Reviewed by [Cursor Bugbot](https://cursor.com/bugbot) for commit
97e035af305c71f79c32bd0ccfdf37f3cc4b982e. Bugbot is set up for automated
code reviews on this repo. Configure
[here](https://www.cursor.com/dashboard/bugbot).</sup>
<!-- /CURSOR_SUMMARY -->

---------

Co-authored-by: Cursor <cursoragent@cursor.com> via Antonio Pallares
* Add maestro E2E test for purchase through paywall (#837)

## Summary

Adds the "purchase through paywall" flow, plus the native overlay
Maestro needs in order to see the app at all.

- Unity draws all of uGUI into a single native view, so no in-game text
reaches the iOS or Android accessibility tree. Invisible native labels
are placed over Unity's view purely so UI automation can find and tap
the UI.
- Flow: launch, open the purchase screen, assert no entitlements,
present the paywall, buy Yearly, assert `pro`.
- `utils/confirm_purchase.yaml` matches the test store confirmation
dialog on both platforms.

Depends on #836.

<!-- CURSOR_SUMMARY -->
---

> [!NOTE]
> **Low Risk**
> Changes are confined to the Maestro test app and Maestro YAML under
e2e-tests; no production SDK or payment logic is modified.
> 
> **Overview**
> Adds Maestro E2E coverage for **purchase through paywall** and the
**native accessibility overlay** needed so Maestro can see Unity uGUI on
Android and iOS.
> 
> **Maestro flows:** `config.yaml` registers `e2e_tests/*`;
`purchase_through_paywall.yaml` launches the app, navigates to purchase,
asserts `Entitlements: none`, presents the paywall, buys Yearly,
confirms via the test store (`confirm_purchase.yaml`), and waits for
`Entitlements: pro`.
> 
> **Native overlay:** New `NativeAccessibilityOverlay` on Android
(transparent `TextViews`) and iOS (transparent `UILabels`) mirror uGUI
bounds in the platform accessibility tree. `MaestroTestApp` initializes
the overlay on device builds, refreshes labels per screen (`testCases` /
`purchase`), and only exposes entitlements on the purchase screen so
assertions cannot pass on the wrong screen.
> 
> Overlay views are non-interactive so taps still hit Unity; iOS hosts
the overlay under the root view controller so paywalls stay above it.
> 
> <sup>Reviewed by [Cursor Bugbot](https://cursor.com/bugbot) for commit
a6d9befcb95cd9cf6bfb73f5194dcf22d273f024. Bugbot is set up for automated
code reviews on this repo. Configure
[here](https://www.cursor.com/dashboard/bugbot).</sup>
<!-- /CURSOR_SUMMARY -->

---------

Co-authored-by: Cursor <cursoragent@cursor.com> via Antonio Pallares
* Add maestro E2E test app (#836) via Antonio Pallares (@ajpallares)
* Remove workflows dangerous setting (#1030) via Cesar de la Vega (@vegaro)
* Don't run `deploy-check` on `bump` and `phc-upgrade` pipelines (#1028) via Cesar de la Vega (@vegaro)
