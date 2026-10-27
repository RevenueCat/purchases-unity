import Foundation
import PurchasesHybridCommon
@_spi(Internal) import RevenueCat
@_spi(InviteOnlyCheckpointsApi) import RevenueCatUI

// Copy every C string before returning to C#. The SDK may finish long after this call returns.
@_cdecl("rcui_checkpoint")
func rcui_checkpoint(
    _ identifier: UnsafePointer<CChar>?,
    _ customVariablesJson: UnsafePointer<CChar>?,
    _ presentationMode: UnsafePointer<CChar>?,
    _ callId: UnsafePointer<CChar>?,
    _ callback: (@convention(c) (UnsafePointer<CChar>, UnsafePointer<CChar>) -> Void)?
) -> Bool {
    guard #available(iOS 15.0, *), Purchases.isConfigured,
          let identifier, let callId, let callback else {
        NSLog("[RevenueCatUI] Checkpoints require iOS 15 or later and configured Purchases.")
        return false
    }
    let checkpointIdentifier = String(cString: identifier)
    let invocationId = String(cString: callId)
    let mode = presentationMode.map { String(cString: $0) } ?? "DEFAULT"
    let variablesJson = customVariablesJson.map { String(cString: $0) }
    DispatchQueue.main.async {
        let rawVariables = variablesJson.flatMap { $0.data(using: .utf8) }
            .flatMap { try? JSONSerialization.jsonObject(with: $0) as? [String: Any] } ?? [:]
        let variables = rawVariables.compactMapValues { value -> CustomVariableValue? in
            if let number = value as? NSNumber {
                return CFGetTypeID(number) == CFBooleanGetTypeID()
                    ? .bool(number.boolValue) : .number(number.doubleValue)
            }
            if let string = value as? String { return .string(string) }
            return nil
        }
        let presentation: FlowPresentationMode
        switch mode {
        case "MODAL_FULL_SCREEN": presentation = .modalFullScreen
        case "MODAL_SHEET": presentation = .modalSheet
        default: presentation = .default
        }
        Purchases.shared.checkpoint(checkpointIdentifier, customVariables: variables, presentationMode: presentation) { result in
            let customer = result.customer.dictionary
            let entitlements = (customer["entitlements"] as? [String: Any])?["all"] as? [String: Any] ?? [:]
            let flow: Any = result.flow.map { flow -> [String: Any] in
                [
                    "obtainedEntitlements": flow.obtainedEntitlements.sorted {
                        $0.entitlementInfo.identifier < $1.entitlementInfo.identifier
                    }.map { ["entitlementInfo": entitlements[$0.entitlementInfo.identifier] ?? NSNull()] },
                    "obtainedProducts": flow.obtainedProducts.sorted { $0.product.id < $1.product.id }.map {
                        // Use PHC's product serializer, preserving the original product's context and billing plan.
                        let package = Package(identifier: $0.product.id, packageType: .custom,
                                              storeProduct: $0.product, offeringIdentifier: "", webCheckoutUrl: nil)
                        return ["product": package.dictionary["product"] ?? NSNull()]
                    }
                ]
            } ?? NSNull()
            do {
                let data = try JSONSerialization.data(withJSONObject: ["customer": customer, "flow": flow])
                let json = String(decoding: data, as: UTF8.self)
                invocationId.withCString { callId in
                    json.withCString { payload in callback(callId, payload) }
                }
            } catch {
                NSLog("[RevenueCatUI] Failed to serialize checkpoint result: %@", String(describing: error))
            }
        }
    }
    return true
}
