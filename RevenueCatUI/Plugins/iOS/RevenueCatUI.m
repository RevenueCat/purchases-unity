#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
@import RevenueCat;
@import PurchasesHybridCommonUI;
#import <PurchasesHybridCommonUI/PurchasesHybridCommonUI-Swift.h>

typedef void (*RCUIPaywallResultCallback)(const char *result);

static NSString *const kRCUIOptionRequiredEntitlementIdentifier = @"requiredEntitlementIdentifier";
static NSString *const kRCUIOptionOfferingIdentifier = @"offeringIdentifier";
static NSString *const kRCUIOptionPresentedOfferingContext = @"presentedOfferingContext";
static NSString *const kRCUIOptionDisplayCloseButton = @"displayCloseButton";
static NSString *const kRCUIPresentedOfferingIdentifierKey = @"offeringIdentifier";

static NSString *RCUIStringFromCString(const char *string) {
    if (string == NULL) {
        return nil;
    }
    return [NSString stringWithUTF8String:string];
}

static NSString *RCUINormalizedResultToken(NSString *resultName) {
    if (resultName.length == 0) {
        return @"ERROR";
    }

    NSString *normalized = [[[resultName stringByReplacingOccurrencesOfString:@"_" withString:@""]
                                       stringByReplacingOccurrencesOfString:@"-" withString:@""]
                                      lowercaseString];

    if ([normalized isEqualToString:@"purchased"]) {
        return @"PURCHASED";
    }
    if ([normalized isEqualToString:@"restored"]) {
        return @"RESTORED";
    }
    if ([normalized isEqualToString:@"cancelled"] || [normalized isEqualToString:@"usercancelled"]) {
        return @"CANCELLED";
    }
    if ([normalized isEqualToString:@"notpresented"] || [normalized isEqualToString:@"notshown"]) {
        return @"NOT_PRESENTED";
    }
    if ([normalized isEqualToString:@"error"] || [normalized isEqualToString:@"failed"]) {
        return @"ERROR";
    }

    return [resultName uppercaseString];
}

static BOOL RCUIRuntimeSupportsPaywalls(void) {
    if (@available(iOS 15.0, *)) {
        return YES;
    }
    return NO;
}

static void RCUIInvokeCallback(RCUIPaywallResultCallback callback, NSString *token, NSString *message) {
    if (callback == NULL) {
        return;
    }

    NSString *payload = token ?: @"ERROR";
    if (message.length > 0) {
        payload = [payload stringByAppendingFormat:@"|%@", message];
    }

    dispatch_async(dispatch_get_main_queue(), ^{
        callback([payload UTF8String]);
    });
}

static BOOL RCUIEnsureReady(RCUIPaywallResultCallback callback) {
    if (!RCUIRuntimeSupportsPaywalls()) {
        RCUIInvokeCallback(callback, @"NOT_PRESENTED", @"Requires iOS 15.0+");
        return NO;
    }

    if (!RCPurchases.isConfigured) {
        RCUIInvokeCallback(callback, @"ERROR", @"PurchasesNotConfigured");
        return NO;
    }

    return YES;
}

static NSMutableDictionary *RCUICreateOptionsDictionary(NSString *offeringIdentifier, BOOL displayCloseButton) {
    NSMutableDictionary *options = [NSMutableDictionary new];
    options[kRCUIOptionDisplayCloseButton] = @(displayCloseButton);

    if (offeringIdentifier.length > 0) {
        options[kRCUIOptionOfferingIdentifier] = offeringIdentifier;
        options[kRCUIOptionPresentedOfferingContext] = @{ kRCUIPresentedOfferingIdentifierKey: offeringIdentifier };
    }

    return options;
}

static void RCUIPresentPaywallInternal(NSString *offeringIdentifier, BOOL displayCloseButton, RCUIPaywallResultCallback callback) {
    dispatch_async(dispatch_get_main_queue(), ^{
        if (@available(iOS 15.0, *)) {
            __block PaywallProxy *proxy = [[PaywallProxy alloc] init];

            NSMutableDictionary *options = RCUICreateOptionsDictionary(offeringIdentifier, displayCloseButton);

            [proxy presentPaywallWithOptions:options
                         paywallResultHandler:^(NSString * _Nonnull resultName) {
                NSString *token = RCUINormalizedResultToken(resultName);
                RCUIInvokeCallback(callback, token, nil);
                proxy = nil;
            }];
        } else {
            RCUIInvokeCallback(callback, @"NOT_PRESENTED", @"Requires iOS 15.0+");
        }
    });
}

static void RCUIPresentPaywallIfNeededInternal(NSString *requiredEntitlementIdentifier,
                                               NSString *offeringIdentifier,
                                               BOOL displayCloseButton,
                                               RCUIPaywallResultCallback callback) {
    dispatch_async(dispatch_get_main_queue(), ^{
        if (@available(iOS 15.0, *)) {
            __block PaywallProxy *proxy = [[PaywallProxy alloc] init];

            NSMutableDictionary *options = RCUICreateOptionsDictionary(offeringIdentifier, displayCloseButton);
            options[kRCUIOptionRequiredEntitlementIdentifier] = requiredEntitlementIdentifier;

            [proxy presentPaywallIfNeededWithOptions:options
                                  paywallResultHandler:^(NSString * _Nonnull resultName) {
                NSString *token = RCUINormalizedResultToken(resultName);
                RCUIInvokeCallback(callback, token, nil);
                proxy = nil;
            }];
        } else {
            RCUIInvokeCallback(callback, @"NOT_PRESENTED", @"Requires iOS 15.0+");
        }
    });
}

void rcui_presentPaywall(const char *offeringIdentifier, bool displayCloseButton, RCUIPaywallResultCallback callback) {
    if (!RCUIEnsureReady(callback)) {
        return;
    }

    NSString *offering = RCUIStringFromCString(offeringIdentifier);
    RCUIPresentPaywallInternal(offering, displayCloseButton ? YES : NO, callback);
}

void rcui_presentPaywallIfNeeded(const char *requiredEntitlementIdentifier,
                                 const char *offeringIdentifier,
                                 bool displayCloseButton,
                                 RCUIPaywallResultCallback callback) {
    if (!RCUIEnsureReady(callback)) {
        return;
    }

    NSString *entitlement = RCUIStringFromCString(requiredEntitlementIdentifier);
    NSString *offering = RCUIStringFromCString(offeringIdentifier);

    if (entitlement.length == 0) {
        RCUIPresentPaywallInternal(offering, displayCloseButton ? YES : NO, callback);
        return;
    }

    RCUIPresentPaywallIfNeededInternal(entitlement, offering, displayCloseButton ? YES : NO, callback);
}

bool rcui_isSupported(void) {
    return RCUIRuntimeSupportsPaywalls();
}
