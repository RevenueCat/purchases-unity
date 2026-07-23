#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
@import RevenueCat;
@import PurchasesHybridCommonUI;

typedef void (*RCUIPaywallResultCallback)(const char *result);
typedef void (*RCUIPurchaseLogicPurchaseCallback)(const char *requestId, const char *packageJson);
typedef void (*RCUIPurchaseLogicRestoreCallback)(const char *requestId);
typedef void (*RCUIPaywallEventCallback)(const char *eventName, const char *payloadJson);
typedef void (*RCUICustomerCenterDismissedCallback)(void);
typedef void (*RCUICustomerCenterErrorCallback)(void);
typedef void (*RCUICustomerCenterEventCallback)(const char *eventName, const char *payload);

static NSString *const kRCUIOptionRequiredEntitlementIdentifier = @"requiredEntitlementIdentifier";
static NSString *const kRCUIOptionOfferingIdentifier = @"offeringIdentifier";
static NSString *const kRCUIOptionDisplayCloseButton = @"displayCloseButton";
static NSString *const kRCUIOptionPresentedOfferingContext = @"presentedOfferingContext";
static NSString *const kRCUIOptionCustomVariables = @"customVariables";

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

static void RCUIEmitPaywallEvent(RCUIPaywallEventCallback callback, const char *eventName, NSDictionary *payload) {
    if (callback == NULL) {
        return;
    }

    NSString *jsonString = nil;
    if (payload != nil) {
        // dataWithJSONObject: raises NSInvalidArgumentException (rather than returning an
        // error) for non-JSON-serializable values, so validate first.
        if (![NSJSONSerialization isValidJSONObject:payload]) {
            NSLog(@"[RevenueCatUI] Skipping paywall event %s: payload is not JSON-serializable", eventName);
            return;
        }
        NSError *error = nil;
        NSData *jsonData = [NSJSONSerialization dataWithJSONObject:payload options:0 error:&error];
        if (!jsonData || error) {
            return;
        }
        jsonString = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
    }

    // Same queue as RCUIInvokeCallback so events keep FIFO ordering with the final result.
    dispatch_async(dispatch_get_main_queue(), ^{
        callback(eventName, jsonString != nil ? jsonString.UTF8String : "");
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

static NSMutableDictionary *RCUICreateOptionsDictionary(NSString *offeringIdentifier, NSString *presentedOfferingContextJson, BOOL displayCloseButton, BOOL useFullScreenPresentation, NSString *customVariablesJson) {
    NSMutableDictionary *options = [NSMutableDictionary new];
    options[kRCUIOptionDisplayCloseButton] = @(displayCloseButton);
    options[@"useFullScreenPresentation"] = @(useFullScreenPresentation);

    if (offeringIdentifier.length > 0) {
        options[kRCUIOptionOfferingIdentifier] = offeringIdentifier;
    }

    if (presentedOfferingContextJson.length > 0) {
        id presentedOfferingContext = RCUIJSONObjectFromJSONString(presentedOfferingContextJson);
        if (presentedOfferingContext) {
            options[kRCUIOptionPresentedOfferingContext] = presentedOfferingContext;
        }
    }

    if (customVariablesJson.length > 0) {
        id customVariables = RCUIJSONObjectFromJSONString(customVariablesJson);
        if (customVariables) {
            options[kRCUIOptionCustomVariables] = customVariables;
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

// Strongly retained by the presentation functions for the duration of a presentation,
// since PaywallProxy.delegate is weak.
@interface RCUIPaywallDelegate : NSObject <RCPaywallViewControllerDelegateWrapper>
@property (nonatomic, assign, nullable) RCUIPaywallEventCallback eventCallback;
@end

@implementation RCUIPaywallDelegate

- (void)paywallViewController:(RCPaywallViewController *)controller
  didStartPurchaseWithPackage:(NSDictionary<NSString *, id> *)packageDictionary API_AVAILABLE(ios(15.0)) {
    RCUIEmitPaywallEvent(self.eventCallback, "onPurchaseStarted", @{@"package": packageDictionary ?: @{}});
}

// Only the transaction variant is implemented: PaywallProxy forwards both
// didFinishPurchasingWith variants, so implementing both would fire the event twice.
- (void)paywallViewController:(RCPaywallViewController *)controller
didFinishPurchasingWithCustomerInfoDictionary:(NSDictionary<NSString *, id> *)customerInfoDictionary
        transactionDictionary:(NSDictionary<NSString *, id> *)transactionDictionary API_AVAILABLE(ios(15.0)) {
    NSMutableDictionary *payload = [NSMutableDictionary new];
    payload[@"customerInfo"] = customerInfoDictionary ?: @{};
    if (transactionDictionary != nil) {
        payload[@"storeTransaction"] = transactionDictionary;
    }
    RCUIEmitPaywallEvent(self.eventCallback, "onPurchaseCompleted", payload);
}

- (void)paywallViewControllerDidCancelPurchase:(RCPaywallViewController *)controller API_AVAILABLE(ios(15.0)) {
    RCUIEmitPaywallEvent(self.eventCallback, "onPurchaseCancelled", nil);
}

- (void)paywallViewController:(RCPaywallViewController *)controller
didFailPurchasingWithErrorDictionary:(NSDictionary<NSString *, id> *)errorDictionary API_AVAILABLE(ios(15.0)) {
    RCUIEmitPaywallEvent(self.eventCallback, "onPurchaseError", @{@"error": errorDictionary ?: @{}});
}

- (void)paywallViewControllerDidStartRestore:(RCPaywallViewController *)controller API_AVAILABLE(ios(15.0)) {
    RCUIEmitPaywallEvent(self.eventCallback, "onRestoreStarted", nil);
}

- (void)paywallViewController:(RCPaywallViewController *)controller
didFinishRestoringWithCustomerInfoDictionary:(NSDictionary<NSString *, id> *)customerInfoDictionary API_AVAILABLE(ios(15.0)) {
    RCUIEmitPaywallEvent(self.eventCallback, "onRestoreCompleted", @{@"customerInfo": customerInfoDictionary ?: @{}});
}

- (void)paywallViewController:(RCPaywallViewController *)controller
didFailRestoringWithErrorDictionary:(NSDictionary<NSString *, id> *)errorDictionary API_AVAILABLE(ios(15.0)) {
    RCUIEmitPaywallEvent(self.eventCallback, "onRestoreError", @{@"error": errorDictionary ?: @{}});
}

@end

static void RCUIPresentPaywallInternal(NSString *offeringIdentifier,
                                       NSString *presentedOfferingContextJson,
                                       BOOL displayCloseButton,
                                       BOOL useFullScreenPresentation,
                                       NSString *customVariablesJson,
                                       BOOL hasPaywallListener,
                                       RCUIPaywallEventCallback eventCallback,
                                       RCUIPaywallResultCallback callback) {
    dispatch_async(dispatch_get_main_queue(), ^{
        if (@available(iOS 15.0, *)) {
            __block PaywallProxy *proxy = [[PaywallProxy alloc] init];
            // Retained via __block for the duration of the presentation; PaywallProxy.delegate is weak.
            __block RCUIPaywallDelegate *paywallDelegate = nil;
            if (hasPaywallListener && eventCallback != NULL) {
                paywallDelegate = [[RCUIPaywallDelegate alloc] init];
                paywallDelegate.eventCallback = eventCallback;
                proxy.delegate = paywallDelegate;
            }

            NSMutableDictionary *options = RCUICreateOptionsDictionary(offeringIdentifier, presentedOfferingContextJson, displayCloseButton, useFullScreenPresentation, customVariablesJson);

            [proxy presentPaywallWithOptions:options
                        purchaseLogicBridge:nil
                        paywallResultHandler:^(NSString * _Nonnull resultName) {
                NSString *token = RCUINormalizedResultToken(resultName);
                RCUIInvokeCallback(callback, token, nil);
                proxy.delegate = nil;
                paywallDelegate = nil;
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
                                               BOOL useFullScreenPresentation,
                                               NSString *customVariablesJson,
                                               BOOL hasPaywallListener,
                                               RCUIPaywallEventCallback eventCallback,
                                               RCUIPaywallResultCallback callback) {
    dispatch_async(dispatch_get_main_queue(), ^{
        if (@available(iOS 15.0, *)) {
            __block PaywallProxy *proxy = [[PaywallProxy alloc] init];
            __block RCUIPaywallDelegate *paywallDelegate = nil;
            if (hasPaywallListener && eventCallback != NULL) {
                paywallDelegate = [[RCUIPaywallDelegate alloc] init];
                paywallDelegate.eventCallback = eventCallback;
                proxy.delegate = paywallDelegate;
            }

            NSMutableDictionary *options = RCUICreateOptionsDictionary(offeringIdentifier, presentedOfferingContextJson, displayCloseButton, useFullScreenPresentation, customVariablesJson);
            options[kRCUIOptionRequiredEntitlementIdentifier] = requiredEntitlementIdentifier;

            [proxy presentPaywallIfNeededWithOptions:options
                                purchaseLogicBridge:nil
                                paywallResultHandler:^(NSString * _Nonnull resultName) {
                NSString *token = RCUINormalizedResultToken(resultName);
                RCUIInvokeCallback(callback, token, nil);
                proxy.delegate = nil;
                paywallDelegate = nil;
                proxy = nil;
            }];
        } else {
            RCUIInvokeCallback(callback, @"NOT_PRESENTED", @"Requires iOS 15.0+");
        }
    });
}

void rcui_presentPaywall(const char *offeringIdentifier, const char *presentedOfferingContextJson, bool displayCloseButton, bool useFullScreenPresentation, const char *customVariablesJson, bool hasPaywallListener, RCUIPaywallEventCallback eventCallback, RCUIPaywallResultCallback callback) {
    if (!RCUIEnsureReady(callback)) {
        return;
    }

    NSString *offering = RCUIStringFromCString(offeringIdentifier);
    NSString *contextJson = RCUIStringFromCString(presentedOfferingContextJson);
    NSString *customVarsJson = RCUIStringFromCString(customVariablesJson);
    RCUIPresentPaywallInternal(offering, contextJson, displayCloseButton ? YES : NO, useFullScreenPresentation ? YES : NO, customVarsJson, hasPaywallListener ? YES : NO, eventCallback, callback);
}

void rcui_presentPaywallIfNeeded(const char *requiredEntitlementIdentifier,
                                 const char *offeringIdentifier,
                                 const char *presentedOfferingContextJson,
                                 bool displayCloseButton,
                                 bool useFullScreenPresentation,
                                 const char *customVariablesJson,
                                 bool hasPaywallListener,
                                 RCUIPaywallEventCallback eventCallback,
                                 RCUIPaywallResultCallback callback) {
    if (!RCUIEnsureReady(callback)) {
        return;
    }

    NSString *entitlement = RCUIStringFromCString(requiredEntitlementIdentifier);
    NSString *offering = RCUIStringFromCString(offeringIdentifier);
    NSString *contextJson = RCUIStringFromCString(presentedOfferingContextJson);
    NSString *customVarsJson = RCUIStringFromCString(customVariablesJson);

    if (entitlement.length == 0) {
        RCUIPresentPaywallInternal(offering, contextJson, displayCloseButton ? YES : NO, useFullScreenPresentation ? YES : NO, customVarsJson, hasPaywallListener ? YES : NO, eventCallback, callback);
        return;
    }

    RCUIPresentPaywallIfNeededInternal(entitlement, offering, contextJson, displayCloseButton ? YES : NO, useFullScreenPresentation ? YES : NO, customVarsJson, hasPaywallListener ? YES : NO, eventCallback, callback);
}

// MARK: - Purchase Logic Support

static NSString *RCUISerializePackageEventData(NSDictionary *eventData) API_AVAILABLE(ios(15.0)) {
    id packageDict = eventData[HybridPurchaseLogicBridge.eventKeyPackageBeingPurchased];
    if (!packageDict) {
        return @"{}";
    }
    NSError *error = nil;
    NSData *jsonData = [NSJSONSerialization dataWithJSONObject:packageDict options:0 error:&error];
    if (error || !jsonData) {
        return @"{}";
    }
    return [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding] ?: @"{}";
}

static HybridPurchaseLogicBridge *RCUICreatePurchaseLogicBridge(
    RCUIPurchaseLogicPurchaseCallback purchaseCallback,
    RCUIPurchaseLogicRestoreCallback restoreCallback
) API_AVAILABLE(ios(15.0)) {
    return [[HybridPurchaseLogicBridge alloc]
        initOnPerformPurchase:^(NSDictionary<NSString *, id> * _Nonnull eventData) {
            NSString *requestId = eventData[HybridPurchaseLogicBridge.eventKeyRequestId];
            NSString *packageJson = RCUISerializePackageEventData(eventData);
            if (purchaseCallback != NULL && requestId != nil) {
                purchaseCallback(requestId.UTF8String, packageJson.UTF8String);
            }
        }
        onPerformRestore:^(NSDictionary<NSString *, id> * _Nonnull eventData) {
            NSString *requestId = eventData[HybridPurchaseLogicBridge.eventKeyRequestId];
            if (restoreCallback != NULL && requestId != nil) {
                restoreCallback(requestId.UTF8String);
            }
        }];
}

void rcui_presentPaywallWithPurchaseLogic(const char *offeringIdentifier,
                                          const char *presentedOfferingContextJson,
                                          bool displayCloseButton,
                                          bool useFullScreenPresentation,
                                          const char *customVariablesJson,
                                          RCUIPurchaseLogicPurchaseCallback purchaseCallback,
                                          RCUIPurchaseLogicRestoreCallback restoreCallback,
                                          bool hasPaywallListener,
                                          RCUIPaywallEventCallback eventCallback,
                                          RCUIPaywallResultCallback resultCallback) {
    if (!RCUIEnsureReady(resultCallback)) {
        return;
    }

    NSString *offering = RCUIStringFromCString(offeringIdentifier);
    NSString *contextJson = RCUIStringFromCString(presentedOfferingContextJson);
    NSString *customVarsJson = RCUIStringFromCString(customVariablesJson);

    dispatch_async(dispatch_get_main_queue(), ^{
        if (@available(iOS 15.0, *)) {
            __block PaywallProxy *proxy = [[PaywallProxy alloc] init];
            __block HybridPurchaseLogicBridge *bridge = RCUICreatePurchaseLogicBridge(purchaseCallback, restoreCallback);
            __block RCUIPaywallDelegate *paywallDelegate = nil;
            if (hasPaywallListener && eventCallback != NULL) {
                paywallDelegate = [[RCUIPaywallDelegate alloc] init];
                paywallDelegate.eventCallback = eventCallback;
                proxy.delegate = paywallDelegate;
            }

            NSMutableDictionary *options = RCUICreateOptionsDictionary(offering, contextJson, displayCloseButton ? YES : NO, useFullScreenPresentation ? YES : NO, customVarsJson);

            [proxy presentPaywallWithOptions:options
                        purchaseLogicBridge:bridge
                        paywallResultHandler:^(NSString * _Nonnull resultName) {
                NSString *token = RCUINormalizedResultToken(resultName);
                RCUIInvokeCallback(resultCallback, token, nil);
                proxy.delegate = nil;
                paywallDelegate = nil;
                proxy = nil;
                bridge = nil;
            }];
        } else {
            RCUIInvokeCallback(resultCallback, @"NOT_PRESENTED", @"Requires iOS 15.0+");
        }
    });
}

void rcui_presentPaywallIfNeededWithPurchaseLogic(const char *requiredEntitlementIdentifier,
                                                   const char *offeringIdentifier,
                                                   const char *presentedOfferingContextJson,
                                                   bool displayCloseButton,
                                                   bool useFullScreenPresentation,
                                                   const char *customVariablesJson,
                                                   RCUIPurchaseLogicPurchaseCallback purchaseCallback,
                                                   RCUIPurchaseLogicRestoreCallback restoreCallback,
                                                   bool hasPaywallListener,
                                                   RCUIPaywallEventCallback eventCallback,
                                                   RCUIPaywallResultCallback resultCallback) {
    if (!RCUIEnsureReady(resultCallback)) {
        return;
    }

    NSString *entitlement = RCUIStringFromCString(requiredEntitlementIdentifier);
    NSString *offering = RCUIStringFromCString(offeringIdentifier);
    NSString *contextJson = RCUIStringFromCString(presentedOfferingContextJson);
    NSString *customVarsJson = RCUIStringFromCString(customVariablesJson);

    if (entitlement.length == 0) {
        rcui_presentPaywallWithPurchaseLogic(offeringIdentifier, presentedOfferingContextJson,
                                             displayCloseButton, useFullScreenPresentation, customVariablesJson, purchaseCallback, restoreCallback, hasPaywallListener, eventCallback, resultCallback);
        return;
    }

    dispatch_async(dispatch_get_main_queue(), ^{
        if (@available(iOS 15.0, *)) {
            __block PaywallProxy *proxy = [[PaywallProxy alloc] init];
            __block HybridPurchaseLogicBridge *bridge = RCUICreatePurchaseLogicBridge(purchaseCallback, restoreCallback);
            __block RCUIPaywallDelegate *paywallDelegate = nil;
            if (hasPaywallListener && eventCallback != NULL) {
                paywallDelegate = [[RCUIPaywallDelegate alloc] init];
                paywallDelegate.eventCallback = eventCallback;
                proxy.delegate = paywallDelegate;
            }

            NSMutableDictionary *options = RCUICreateOptionsDictionary(offering, contextJson, displayCloseButton ? YES : NO, useFullScreenPresentation ? YES : NO, customVarsJson);
            options[kRCUIOptionRequiredEntitlementIdentifier] = entitlement;

            [proxy presentPaywallIfNeededWithOptions:options
                                purchaseLogicBridge:bridge
                                paywallResultHandler:^(NSString * _Nonnull resultName) {
                NSString *token = RCUINormalizedResultToken(resultName);
                RCUIInvokeCallback(resultCallback, token, nil);
                proxy.delegate = nil;
                paywallDelegate = nil;
                proxy = nil;
                bridge = nil;
            }];
        } else {
            RCUIInvokeCallback(resultCallback, @"NOT_PRESENTED", @"Requires iOS 15.0+");
        }
    });
}

void rcui_resolvePurchaseLogicResult(const char *requestId, const char *resultString, const char *errorMessage) {
    if (@available(iOS 15.0, *)) {
        NSString *reqId = RCUIStringFromCString(requestId);
        NSString *result = RCUIStringFromCString(resultString);
        NSString *error = RCUIStringFromCString(errorMessage);

        if (reqId == nil || result == nil) {
            return;
        }

        [HybridPurchaseLogicBridge resolveResultWithRequestId:reqId resultString:result errorMessage:error];
    }
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

- (void)customerCenterViewController:(CustomerCenterUIViewController *)controller
       didSucceedWithPromotionalOffer:(NSString *)offerId
               customerInfoDictionary:(NSDictionary<NSString *, id> *)customerInfoDictionary
               transactionDictionary:(NSDictionary<NSString *, id> *)transactionDictionary API_AVAILABLE(ios(15.0)) {
    if (self.eventCallback) {
        NSDictionary *payload = @{
            @"customerInfo": customerInfoDictionary ?: [NSNull null],
            @"transaction": transactionDictionary ?: [NSNull null],
            @"offerId": offerId ?: @""
        };
        NSError *error = nil;
        NSData *jsonData = [NSJSONSerialization dataWithJSONObject:payload options:0 error:&error];
        if (jsonData && !error) {
            NSString *jsonString = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
            dispatch_async(dispatch_get_main_queue(), ^{
                self.eventCallback("onPromotionalOfferSucceeded", jsonString.UTF8String);
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
