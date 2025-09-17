#import <Foundation/Foundation.h>

// Minimal native stubs for iOS bridging

typedef void (*RCUIPaywallResultCallback)(const char* result);

void rcui_presentPaywall(const char* offeringIdentifier, bool displayCloseButton, RCUIPaywallResultCallback callback) {
    NSLog(@"[RevenueCatUI][iOS] presentPaywall(offering=%@, closeButton=%@)",
          offeringIdentifier ? [NSString stringWithUTF8String:offeringIdentifier] : @"<null>",
          displayCloseButton ? @"true" : @"false");
    if (callback) {
        callback("CANCELLED|Stub: no native UI");
    }
}

void rcui_presentPaywallIfNeeded(const char* requiredEntitlementIdentifier, const char* offeringIdentifier, bool displayCloseButton, RCUIPaywallResultCallback callback) {
    NSLog(@"[RevenueCatUI][iOS] presentPaywallIfNeeded(entitlement=%@, offering=%@, closeButton=%@)",
          requiredEntitlementIdentifier ? [NSString stringWithUTF8String:requiredEntitlementIdentifier] : @"<null>",
          offeringIdentifier ? [NSString stringWithUTF8String:offeringIdentifier] : @"<null>",
          displayCloseButton ? @"true" : @"false");
    if (callback) {
        callback("NOT_PRESENTED|Stub: no native UI");
    }
}

// No Customer Center in this stub

bool rcui_isSupported() {
    NSLog(@"[RevenueCatUI][iOS] isSupported() -> true (stub)");
    return true;
}
