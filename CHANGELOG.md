## 1.0.2

- Updates iOS SDK to 2.1.1

## 1.0.1

- Fixing crash on iOS when missing an underlying error

## 1.0.0

- Updates SDKs to 2.1.0. This means there is new functions added:
- Changes the SDK to use callback functions instead of delegates. There is a UpdatedPurchaserInfoListener that sends a purchaser info object. This listener is used to listen to changes in the purchaser info.
- Added setDebugLogsEnabled to display debug logs.
- Added getPurchaserInfo function to get the latest purchaser info known by the SDK.
- Added getEntitlements
- Added getAppUserId

## 0.6.1

- Adds setFinishTransactions for iOS
- Adds more attribution networks

## 0.6.0

- Updates iOS SDK to 1.20 and Android SDK to 1.4.0.
- Adds identify, create alias and reset call

## 0.5.4

- Fixes onRestoreTransactions never being called.

## 0.5.3

- Fixes onRestoreTransactions not being called if there are no tokens.

## 0.5.2

- Fixes crash due to not able to find Kotlin dependency.

## 0.5.1

- Adds requestDate to the purchaser info to avoid edge cases.

## 0.5.0

- Enhance the PurchasesListener protocol to include methods for restore succeeded and failed.

## 0.4.2

- Add Android support for Adjust

## 0.4.1

- Add support for idfa data in Adjust
