// RevenueCat UI iOS bridge implemented via PurchasesHybridCommonUI
#import <Foundation/Foundation.h>
@import PurchasesHybridCommonUI;
// Import Swift interface for direct calls into PaywallProxy
#import <PurchasesHybridCommonUI/PurchasesHybridCommonUI-Swift.h>

typedef void (*RCUIPaywallResultCallback)(const char* result);
typedef void (*RCUICustomerCenterCallback)(void);

static NSString *RCUINormalizeResult(NSString *resultName) {
    if (resultName == nil) {
        return @"ERROR";
    }
    NSString *lower = [[resultName lowercaseString] stringByReplacingOccurrencesOfString:@"_" withString:@""];
    if ([lower isEqualToString:@"purchased"]) return @"PURCHASED";
    if ([lower isEqualToString:@"restored"]) return @"RESTORED";
    if ([lower isEqualToString:@"cancelled"]) return @"CANCELLED";
    if ([lower isEqualToString:@"error"]) return @"ERROR";
    if ([lower isEqualToString:@"notpresented"]) return @"NOT_PRESENTED";
    return @"ERROR";
}

static PaywallProxy *s_rcuiPaywallProxy = nil;

void rcui_presentPaywall(const char* offeringIdentifier, bool displayCloseButton, RCUIPaywallResultCallback callback) {
    if (@available(iOS 15.0, *)) {
        s_rcuiPaywallProxy = [[PaywallProxy alloc] init];
        NSMutableDictionary *options = [NSMutableDictionary new];
        options[@"displayCloseButton"] = @(displayCloseButton);
        if (offeringIdentifier != NULL) {
            NSString *offering = [NSString stringWithUTF8String:offeringIdentifier];
            if (offering.length > 0) {
                options[@"offeringIdentifier"] = offering;
            }
        }
        dispatch_async(dispatch_get_main_queue(), ^{
            [s_rcuiPaywallProxy presentPaywallWithOptions:options paywallResultHandler:^(NSString * _Nonnull resultName) {
                if (callback) {
                    NSString *normalized = RCUINormalizeResult(resultName);
                    callback([normalized UTF8String]);
                }
                s_rcuiPaywallProxy = nil;
            }];
        });
    } else {
        if (callback) { callback("NOT_PRESENTED"); }
    }
}

void rcui_presentPaywallIfNeeded(const char* requiredEntitlementIdentifier, const char* offeringIdentifier, bool displayCloseButton, RCUIPaywallResultCallback callback) {
    // TODO: entitlement check is not wired; return NOT_PRESENTED
    if (callback) {
        callback("NOT_PRESENTED|TODO: entitlement check not implemented");
    }
}

// Customer Center not implemented yet in this bridge

bool rcui_isSupported() {
    if (@available(iOS 15.0, *)) {
        return true;
    }
    return false;
}
