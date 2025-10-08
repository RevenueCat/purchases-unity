#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
@import RevenueCat;
@import PurchasesHybridCommonUI;
#import <PurchasesHybridCommonUI/PurchasesHybridCommonUI-Swift.h>

typedef void (*RCUIPaywallResultCallback)(const char *result);
typedef void (*RCUICustomerCenterCallback)(const char *result);

static NSString *const kRCUIOptionRequiredEntitlementIdentifier = @"requiredEntitlementIdentifier";
static NSString *const kRCUIOptionOfferingIdentifier = @"offeringIdentifier";
static NSString *const kRCUIOptionDisplayCloseButton = @"displayCloseButton";

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

    NSString *token = [resultName uppercaseString];

    if ([token isEqualToString:@"PURCHASED"]) {
        return @"PURCHASED";
    }
    if ([token isEqualToString:@"RESTORED"]) {
        return @"RESTORED";
    }
    if ([token isEqualToString:@"CANCELLED"] || [token isEqualToString:@"USER_CANCELLED"]) {
        return @"CANCELLED";
    }
    if ([token isEqualToString:@"NOT_PRESENTED"]) {
        return @"NOT_PRESENTED";
    }
    if ([token isEqualToString:@"ERROR"] || [token isEqualToString:@"FAILED"]) {
        return @"ERROR";
    }

    return token;
}

static void RCUIInvokeCallback(RCUIPaywallResultCallback callback, NSString *token, NSString *message) {
    if (callback == NULL) {
        return;
    }

    const char *payload = (token ?: @"ERROR").UTF8String;

    dispatch_async(dispatch_get_main_queue(), ^{
        callback(payload);
    });
}

static void RCUICustomerCenterInvokeCallback(RCUICustomerCenterCallback callback, NSString *token) {
    if (callback == NULL) {
        return;
    }

    dispatch_async(dispatch_get_main_queue(), ^{
        callback((token ?: @"ERROR").UTF8String);
    });
}

static BOOL RCUIEnsureReady(RCUIPaywallResultCallback callback) {
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
    }

    return options;
}

static BOOL RCUICustomerCenterEnsureReady(RCUICustomerCenterCallback callback) {
    if (!RCPurchases.isConfigured) {
        RCUICustomerCenterInvokeCallback(callback, @"ERROR");
        return NO;
    }

    return YES;
}

static void RCUIPresentPaywallInternal(NSString *offeringIdentifier,
                                       BOOL displayCloseButton,
                                       RCUIPaywallResultCallback callback) {
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

void rcui_presentCustomerCenter(RCUICustomerCenterCallback callback) {
    if (!RCUICustomerCenterEnsureReady(callback)) {
        return;
    }

    dispatch_async(dispatch_get_main_queue(), ^{
        if (@available(iOS 15.0, *)) {
            __block CustomerCenterProxy *proxy = [[CustomerCenterProxy alloc] init];
            proxy.shouldShowCloseButton = YES;

            [proxy presentWithResultHandler:^{
                RCUICustomerCenterInvokeCallback(callback, @"DISMISSED");
                proxy = nil;
            }];
        } else {
            RCUICustomerCenterInvokeCallback(callback, @"NOT_PRESENTED");
        }
    });
}
