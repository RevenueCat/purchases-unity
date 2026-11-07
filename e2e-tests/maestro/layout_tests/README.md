# Android paywall navigation margins

These flows and the Python runner protect the gutter below **Restore Purchases**
(portrait) and beside **Continue** (landscape). They use Maestro's rendered bounds
and the physical `navigationBars` source frame from `dumpsys window displays`.
The same gutter is required while navigation controls are hidden. Footer checks
measure the visible Restore text; Compose may enlarge its invisible accessibility
touch target beyond that text. CTA checks measure the nearest clickable button
ancestor, so centered button text cannot conceal an over-wide button.

Install the Maestro test APK built by `build-maestro-test-app-android`, start an
Android API 34 or 36 emulator, and run from the repository root:

```sh
adb -s emulator-5554 install -r e2e-tests/MaestroTestApp/build/android/MaestroTestApp.apk
python3 scripts/test-android-paywall-margins.py --device emulator-5554
```

The runner forces three-button navigation, saves/restores the prior navigation
overlay, system rotation settings, and WindowManager rotation/fixed-rotation
settings. On the dedicated emulator it forces display rotation with
`wm fixed-to-user-rotation enabled` and `wm user-rotation lock 0/1`, waits
for the display dimensions to match, and tests portrait and landscape. This
allows landscape coverage even when Unity or the launcher requests portrait. For each it
captures the initial hidden state, uses `adb input swipe` from the last physical
pixel of the navigation edge, and captures again after a verified reveal/hide
cycle. WindowManager must report the navigation source becoming visible within
5 seconds, then hidden within 15 seconds. Both dumps are saved, including any
optional transient-state diagnostics reported by that Android version. Maestro captures screenshots and
asserts the paywall UI before the geometry checks. A wrong orientation,
visible controls, missing labels, unsafe geometry, or insufficient gutter fails
the run. The paywall must support rotation and hide system navigation controls.

For an already open local reproduction host, check its current orientation.
Reopen it after rotating before checking the other orientation, since Android
can letterbox a dialog that locks its orientation:

```sh
python3 scripts/test-android-paywall-margins.py --device emulator-5554 \
  --app-id com.example.host --already-open --orientations portrait \
  --footer-text 'Restore Purchases' --cta-text Continue \
  --output-dir /tmp/paywall-margins
```

Without `--already-open`, the runner clears and launches the Unity Maestro app
using its existing `purchase_through_paywall` entry point. It never purchases.
API keys belong in the app's normal test build configuration, never these flows.

Outputs include screenshots, hierarchy JSON, display/insets dumps, geometry JSON,
Maestro diagnostics, `commands.log`, and `junit.xml`. By default they are stored
in `fastlane/test_output/android-paywall-margins`. CircleCI runs functional tests
and layout checks on API 34, and layout checks alone on API 36.

The geometry checker has no emulator or SDK dependencies:

```sh
python3 scripts/test-assert-android-paywall-margins.py
python3 scripts/test-android-paywall-runner.py
python3 scripts/assert-android-paywall-margins.py \
  --hierarchy hierarchy.json --insets insets.txt --report geometry.json
```
