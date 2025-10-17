#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
@import RevenueCat;
@import PurchasesHybridCommonUI;
#import <PurchasesHybridCommonUI/PurchasesHybridCommonUI-Swift.h>

typedef void (*RCUIPaywallResultCallback)(const char *result);
typedef void (*RCUICustomerCenterDismissedCallback)(void);
typedef void (*RCUICustomerCenterErrorCallback)(void);
typedef void (*RCUICustomerCenterEventCallback)(const char *eventName, const char *payload);

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

static void RCUIInvokeCallback(RCUIPaywallResultCallback callback, NSString *token, NSString *message) {
    if (callback == NULL) {
        return;
    }

    const char *payload = (token ?: @"ERROR").UTF8String;

    dispatch_async(dispatch_get_main_queue(), ^{
        callback(payload);
    });
}

static void RCUICustomerCenterInvokeDismissedCallback(RCUICustomerCenterDismissedCallback callback) {
    if (callback == NULL) {
        return;
    }

    dispatch_async(dispatch_get_main_queue(), ^{
        callback();
    });
}

static void RCUICustomerCenterInvokeErrorCallback(RCUICustomerCenterErrorCallback callback) {
    if (callback == NULL) {
        return;
    }

    dispatch_async(dispatch_get_main_queue(), ^{
        callback();
    });
}

static BOOL RCUIEnsureReady(RCUIPaywallResultCallback callback) {
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

static BOOL RCUICustomerCenterEnsureReady(RCUICustomerCenterErrorCallback errorCallback) {
    if (!RCPurchases.isConfigured) {
        RCUICustomerCenterInvokeErrorCallback(errorCallback);
        return NO;
    }

    return YES;
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

@interface RCUICustomerCenterDelegate : NSObject <RCCustomerCenterViewControllerDelegateWrapper>
@property (nonatomic, assign, nullable) RCUICustomerCenterEventCallback eventCallback;
@end

@implementation RCUICustomerCenterDelegate

- (void)customerCenterViewControllerWasDismissed:(CustomerCenterUIViewController *)controller API_AVAILABLE(ios(15.0)) {
    
}

- (void)customerCenterViewControllerDidStartRestore:(CustomerCenterUIViewController *)controller API_AVAILABLE(ios(15.0)) {
    if (self.eventCallback) {
        dispatch_async(dispatch_get_main_queue(), ^{
            self.eventCallback("onRestoreStarted", "");
        });
    }
}

- (void)customerCenterViewController:(CustomerCenterUIViewController *)controller
         didFinishRestoringWithCustomerInfoDictionary:(NSDictionary<NSString *, id> *)customerInfoDictionary API_AVAILABLE(ios(15.0)) {
    if (self.eventCallback) {
        NSError *error = nil;
        NSData *jsonData = [NSJSONSerialization dataWithJSONObject:customerInfoDictionary options:0 error:&error];
        if (jsonData && !error) {
            NSString *jsonString = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
            dispatch_async(dispatch_get_main_queue(), ^{
                self.eventCallback("onRestoreCompleted", jsonString.UTF8String);
            });
        }
    }
}

- (void)customerCenterViewController:(CustomerCenterUIViewController *)controller
              didFailRestoringWithErrorDictionary:(NSDictionary<NSString *, id> *)errorDictionary API_AVAILABLE(ios(15.0)) {
    if (self.eventCallback) {
        NSError *error = nil;
        NSData *jsonData = [NSJSONSerialization dataWithJSONObject:errorDictionary options:0 error:&error];
        if (jsonData && !error) {
            NSString *jsonString = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
            dispatch_async(dispatch_get_main_queue(), ^{
                self.eventCallback("onRestoreFailed", jsonString.UTF8String);
            });
        }
    }
}

- (void)customerCenterViewControllerDidShowManageSubscriptions:(CustomerCenterUIViewController *)controller API_AVAILABLE(ios(15.0)) {
    if (self.eventCallback) {
        dispatch_async(dispatch_get_main_queue(), ^{
            self.eventCallback("onShowingManageSubscriptions", "");
        });
    }
}

- (void)customerCenterViewController:(CustomerCenterUIViewController *)controller
didStartRefundRequestForProductWithID:(NSString *)productID API_AVAILABLE(ios(15.0)) {
    if (self.eventCallback) {
        dispatch_async(dispatch_get_main_queue(), ^{
            self.eventCallback("onRefundRequestStarted", productID.UTF8String ?: "");
        });
    }
}

- (void)customerCenterViewController:(CustomerCenterUIViewController *)controller
didCompleteRefundRequestForProductWithID:(NSString *)productID
                          withStatus:(NSString *)status API_AVAILABLE(ios(15.0)) {
    if (self.eventCallback) {
        NSDictionary *payload = @{
            @"productIdentifier": productID ?: @"",
            @"refundRequestStatus": status ?: @""
        };
        NSError *error = nil;
        NSData *jsonData = [NSJSONSerialization dataWithJSONObject:payload options:0 error:&error];
        if (jsonData && !error) {
            NSString *jsonString = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
            dispatch_async(dispatch_get_main_queue(), ^{
                self.eventCallback("onRefundRequestCompleted", jsonString.UTF8String);
            });
        }
    }
}

- (void)customerCenterViewController:(CustomerCenterUIViewController *)controller
didCompleteFeedbackSurveyWithOptionID:(NSString *)optionID API_AVAILABLE(ios(15.0)) {
    if (self.eventCallback) {
        dispatch_async(dispatch_get_main_queue(), ^{
            self.eventCallback("onFeedbackSurveyCompleted", optionID.UTF8String ?: "");
        });
    }
}

- (void)customerCenterViewController:(CustomerCenterUIViewController *)controller
didSelectCustomerCenterManagementOption:(NSString *)optionID
withURL:(NSString *)url API_AVAILABLE(ios(15.0)) {
    if (self.eventCallback) {
        NSDictionary *payload = @{
            @"option": optionID ?: @"",
            @"url": url ?: [NSNull null]
        };
        NSError *error = nil;
        NSData *jsonData = [NSJSONSerialization dataWithJSONObject:payload options:0 error:&error];
        if (jsonData && !error) {
            NSString *jsonString = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
            dispatch_async(dispatch_get_main_queue(), ^{
                self.eventCallback("onManagementOptionSelected", jsonString.UTF8String);
            });
        }
    }
}

- (void)customerCenterViewController:(CustomerCenterUIViewController *)controller
               didSelectCustomAction:(NSString *)actionID
              withPurchaseIdentifier:(NSString *)purchaseIdentifier API_AVAILABLE(ios(15.0)) {
    if (self.eventCallback) {
        NSDictionary *payload = @{
            @"actionId": actionID ?: @"",
            @"purchaseIdentifier": purchaseIdentifier ?: [NSNull null]
        };
        NSError *error = nil;
        NSData *jsonData = [NSJSONSerialization dataWithJSONObject:payload options:0 error:&error];
        if (jsonData && !error) {
            NSString *jsonString = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
            dispatch_async(dispatch_get_main_queue(), ^{
                self.eventCallback("onCustomActionSelected", jsonString.UTF8String);
            });
        }
    }
}

@end

void rcui_presentCustomerCenter(RCUICustomerCenterDismissedCallback dismissedCallback, 
                                RCUICustomerCenterErrorCallback errorCallback,
                                RCUICustomerCenterEventCallback eventCallback) {
    if (!RCUICustomerCenterEnsureReady(errorCallback)) {
        return;
    }

    dispatch_async(dispatch_get_main_queue(), ^{
        if (@available(iOS 15.0, *)) {
            __block CustomerCenterProxy *proxy = [[CustomerCenterProxy alloc] init];
            __block RCUICustomerCenterDelegate *delegate = [[RCUICustomerCenterDelegate alloc] init];
            delegate.eventCallback = eventCallback;
            
            proxy.shouldShowCloseButton = YES;
            [proxy setDelegate:delegate];

            [proxy presentWithResultHandler:^{
                RCUICustomerCenterInvokeDismissedCallback(dismissedCallback);
                [proxy setDelegate:nil];
                proxy = nil;
                delegate = nil;
            }];
        } else {
            RCUICustomerCenterInvokeErrorCallback(errorCallback);
        }
    });
}
