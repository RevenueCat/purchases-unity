#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
@import RevenueCat;
@import PurchasesHybridCommonUI;
#import <PurchasesHybridCommonUI/PurchasesHybridCommonUI-Swift.h>

typedef void (*RCUIPaywallResultCallback)(const char *result);

static NSString *const kRCUIOptionRequiredEntitlementIdentifier = @"requiredEntitlementIdentifier";
static NSString *const kRCUIOptionOfferingIdentifier = @"offeringIdentifier";
static NSString *const kRCUIOptionDisplayCloseButton = @"displayCloseButton";
static NSString *const kRCUIOptionPresentedOfferingContext = @"presentedOfferingContext";

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

    const char *payload = (token ?: @"ERROR").UTF8String;

    dispatch_async(dispatch_get_main_queue(), ^{
        callback(payload);
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

static id RCUIJSONObjectFromJSONString(NSString *jsonString) {
    if (jsonString.length == 0) {
        return nil;
    }
    NSData *data = [jsonString dataUsingEncoding:NSUTF8StringEncoding];
    if (!data) { return nil; }
    NSError *error = nil;
    id obj = [NSJSONSerialization JSONObjectWithData:data options:0 error:&error];
    if (error) { return nil; }
    return obj;
}

static NSMutableDictionary *RCUICreateOptionsDictionary(NSString *offeringIdentifier, NSString *presentedOfferingContextJson, BOOL displayCloseButton) {
    NSMutableDictionary *options = [NSMutableDictionary new];
    options[kRCUIOptionDisplayCloseButton] = @(displayCloseButton);

    if (offeringIdentifier.length > 0) {
        options[kRCUIOptionOfferingIdentifier] = offeringIdentifier;
    }

    if (presentedOfferingContextJson.length > 0) {
        id presentedOfferingContext = RCUIJSONObjectFromJSONString(presentedOfferingContextJson);
        if (presentedOfferingContext) {
            options[kRCUIOptionPresentedOfferingContext] = presentedOfferingContext;
        }
    }

    return options;
}

static void RCUIPresentPaywallInternal(NSString *offeringIdentifier,
                                       NSString *presentedOfferingContextJson,
                                       BOOL displayCloseButton,
                                       RCUIPaywallResultCallback callback) {
    dispatch_async(dispatch_get_main_queue(), ^{
        if (@available(iOS 15.0, *)) {
            __block PaywallProxy *proxy = [[PaywallProxy alloc] init];

            NSMutableDictionary *options = RCUICreateOptionsDictionary(offeringIdentifier, presentedOfferingContextJson, displayCloseButton);

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
                                               NSString *presentedOfferingContextJson,
                                               BOOL displayCloseButton,
                                               RCUIPaywallResultCallback callback) {
    dispatch_async(dispatch_get_main_queue(), ^{
        if (@available(iOS 15.0, *)) {
            __block PaywallProxy *proxy = [[PaywallProxy alloc] init];

            NSMutableDictionary *options = RCUICreateOptionsDictionary(offeringIdentifier, presentedOfferingContextJson, displayCloseButton);
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

void rcui_presentPaywall(const char *offeringIdentifier, const char *presentedOfferingContextJson, bool displayCloseButton, RCUIPaywallResultCallback callback) {
    if (!RCUIEnsureReady(callback)) {
        return;
    }

    NSString *offering = RCUIStringFromCString(offeringIdentifier);
    NSString *contextJson = RCUIStringFromCString(presentedOfferingContextJson);
    RCUIPresentPaywallInternal(offering, contextJson, displayCloseButton ? YES : NO, callback);
}

void rcui_presentPaywallIfNeeded(const char *requiredEntitlementIdentifier,
                                 const char *offeringIdentifier,
                                 const char *presentedOfferingContextJson,
                                 bool displayCloseButton,
                                 RCUIPaywallResultCallback callback) {
    if (!RCUIEnsureReady(callback)) {
        return;
    }

    NSString *entitlement = RCUIStringFromCString(requiredEntitlementIdentifier);
    NSString *offering = RCUIStringFromCString(offeringIdentifier);
    NSString *contextJson = RCUIStringFromCString(presentedOfferingContextJson);

    if (entitlement.length == 0) {
        RCUIPresentPaywallInternal(offering, contextJson, displayCloseButton ? YES : NO, callback);
        return;
    }

    RCUIPresentPaywallIfNeededInternal(entitlement, offering, contextJson, displayCloseButton ? YES : NO, callback);
}

bool rcui_isSupported(void) {
    return RCUIRuntimeSupportsPaywalls();
}
