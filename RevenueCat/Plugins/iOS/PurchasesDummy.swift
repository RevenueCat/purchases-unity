import Foundation
@_spi(Internal) import RevenueCat

/// Builds `DangerousSettings` for the Unity bridge. Lives in Swift because `useWorkflows`
/// is an internal `@_spi(Internal)` flag that isn't reachable from the Objective-C bridge.
@objc public class RCPurchasesUnityDangerousSettingsFactory: NSObject {

    @objc public static func make(autoSyncPurchases: Bool, useWorkflows: Bool) -> DangerousSettings {
        return useWorkflows
            ? DangerousSettings(useWorkflows: true)
            : DangerousSettings(autoSyncPurchases: autoSyncPurchases)
    }

}
