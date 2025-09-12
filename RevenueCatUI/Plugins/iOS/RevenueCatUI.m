#import <Foundation/Foundation.h>

// Minimal native stubs for iOS bridging

typedef void (*RCUIPaywallResultCallback)(const char* result);
typedef void (*RCUICustomerCenterCallback)(void);

void rcui_presentPaywall(const char* offeringIdentifier, bool displayCloseButton, RCUIPaywallResultCallback callback) {
    if (callback) {
        callback("CANCELLED|Stub: no native UI");
    }
}

void rcui_presentPaywallIfNeeded(const char* requiredEntitlementIdentifier, const char* offeringIdentifier, bool displayCloseButton, RCUIPaywallResultCallback callback) {
    if (callback) {
        callback("NOT_PRESENTED|Stub: no native UI");
    }
}

void rcui_presentCustomerCenter(RCUICustomerCenterCallback callback) {
    if (callback) {
        callback();
    }
}

bool rcui_isSupported() {
    return true;
}

