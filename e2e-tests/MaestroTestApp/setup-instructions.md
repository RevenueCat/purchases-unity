# MaestroTestApp Unity Scene Setup

This app requires manual setup in the Unity Editor because `.unity` scene files
cannot be created outside the editor.

## Prerequisites

- Import the `RevenueCat` and `RevenueCatUI` packages into the project.

## Scene Setup (MainScene)

### 1. Create a Canvas

- GameObject > UI > Canvas
- Set Canvas Scaler to "Scale With Screen Size", Reference Resolution 375x812.

### 2. Add the Purchases component

- Create an empty GameObject named "Purchases".
- Add the `Purchases` MonoBehaviour component to it.
- Add the `MaestroTestApp` script to the same GameObject.
- In the Inspector, check "Use Runtime Setup" on the Purchases component.

### 3. Create testCasesScreen (child of Canvas)

- Create an empty GameObject named "TestCasesScreen" under the Canvas.
- Add a RectTransform that fills the entire Canvas.
- Add a child **Text** (UI > Legacy > Text):
  - Text content: `Test Cases`
  - Font size: 24, bold, centered horizontally, near the top.
- Add a child **Button** (UI > Legacy > Button):
  - Set the button's child Text to: `Purchase through paywall`
  - On Click: drag the Purchases GameObject, select `MaestroTestApp > ShowPurchaseScreen`.

### 4. Create purchaseScreen (child of Canvas)

- Create an empty GameObject named "PurchaseScreen" under the Canvas.
- Add a RectTransform that fills the entire Canvas.
- Set it to **inactive** by default (uncheck the checkbox at the top of Inspector).
- Add a child **Text** named "EntitlementsLabel" (UI > Legacy > Text):
  - Text content: `Entitlements: none`
  - Font size: 16, centered.
- Add a child **Button** (UI > Legacy > Button):
  - Set the button's child Text to: `Present Paywall`
  - On Click: drag the Purchases GameObject, select `MaestroTestApp > PresentPaywall`.
- Add a child **Button** for back navigation:
  - Set the button's child Text to: `Back`
  - On Click: drag the Purchases GameObject, select `MaestroTestApp > ShowTestCases`.

### 5. Wire references in MaestroTestApp Inspector

- Drag "TestCasesScreen" to the `testCasesScreen` field.
- Drag "PurchaseScreen" to the `purchaseScreen` field.
- Drag "EntitlementsLabel" Text to the `entitlementsLabel` field.

### 6. Project Settings

- Edit > Project Settings > Player:
  - Set Bundle Identifier to: `com.revenuecat.maestro.e2e`
  - Set Product Name to: `MaestroTestApp`

## CRITICAL: UI text must match exactly

- Title: `Test Cases`
- Test case button: `Purchase through paywall`
- Entitlements label: `Entitlements: none` (before purchase) / `Entitlements: pro` (after)
- Paywall button: `Present Paywall`
