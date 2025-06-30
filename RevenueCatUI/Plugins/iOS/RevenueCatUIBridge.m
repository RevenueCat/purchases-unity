//
//  RevenueCatUIBridge.m
//  RevenueCat Unity UI Plugin
//
//  Created for Unity SDK
//

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
@import PurchasesHybridCommonUI;

// Global callback storage
static void (*paywallResultCallback)(const char* result) = NULL;
static void (*customerCenterCallback)(void) = NULL;

// Global proxy instances
static PaywallProxy *paywallProxy = nil;
static CustomerCenterProxy *customerCenterProxy = nil;

#pragma mark - External Utility Methods (from core RevenueCat plugin)

// These utility functions are already defined in PurchasesUnityHelper.m
// We declare them as external to use the existing implementations
extern NSString *convertCString(const char *string);
extern char *makeStringCopy(NSString *nstring);

#pragma mark - Initialization

void initializeRevenueCatUI() {
    if (@available(iOS 15.0, *)) {
        if (!paywallProxy) {
            paywallProxy = [[PaywallProxy alloc] init];
        }
        if (!customerCenterProxy) {
            customerCenterProxy = [[CustomerCenterProxy alloc] init];
        }
        NSLog(@"RevenueCat UI initialized successfully");
    } else {
        NSLog(@"RevenueCat UI requires iOS 15.0 or later");
    }
}

#pragma mark - Paywall Methods

void presentPaywall(const char* offeringIdentifier, bool displayCloseButton, void (*callback)(const char*)) {
    if (@available(iOS 15.0, *)) {
        if (!paywallProxy) {
            initializeRevenueCatUI();
        }
        
        // Store the callback
        paywallResultCallback = callback;
        
        // Create options dictionary
        NSMutableDictionary *options = [[NSMutableDictionary alloc] init];
        
        if (offeringIdentifier && strlen(offeringIdentifier) > 0) {
            options[@"offeringIdentifier"] = convertCString(offeringIdentifier);
        }
        
        options[@"displayCloseButton"] = @(displayCloseButton);
        options[@"shouldBlockTouchEvents"] = @(YES); // Needed for Unity integration
        
        // Present paywall with result handler
        [paywallProxy presentPaywallWithOptions:options paywallResultHandler:^(NSString *result) {
            if (paywallResultCallback) {
                paywallResultCallback([result UTF8String]);
                paywallResultCallback = NULL;
            }
        }];
        
        NSLog(@"Presenting paywall with offering: %s, displayCloseButton: %d", 
              offeringIdentifier ? offeringIdentifier : "default", displayCloseButton);
    } else {
        NSLog(@"Presenting paywall requires iOS 15.0 or later");
        if (callback) {
            callback("{\"error\": \"iOS 15.0 required\"}");
        }
    }
}

void presentPaywallIfNeeded(const char* requiredEntitlementIdentifier, const char* offeringIdentifier, bool displayCloseButton, void (*callback)(const char*)) {
    if (@available(iOS 15.0, *)) {
        if (!paywallProxy) {
            initializeRevenueCatUI();
        }
        
        // Store the callback
        paywallResultCallback = callback;
        
        // Create options dictionary
        NSMutableDictionary *options = [[NSMutableDictionary alloc] init];
        
        if (requiredEntitlementIdentifier && strlen(requiredEntitlementIdentifier) > 0) {
            options[@"requiredEntitlementIdentifier"] = convertCString(requiredEntitlementIdentifier);
        } else {
            NSLog(@"Error: requiredEntitlementIdentifier is required for presentPaywallIfNeeded");
            if (callback) {
                callback("{\"error\": \"requiredEntitlementIdentifier required\"}");
            }
            return;
        }
        
        if (offeringIdentifier && strlen(offeringIdentifier) > 0) {
            options[@"offeringIdentifier"] = convertCString(offeringIdentifier);
        }
        
        options[@"displayCloseButton"] = @(displayCloseButton);
        options[@"shouldBlockTouchEvents"] = @(YES); // Needed for Unity integration
        
        // Present paywall if needed with result handler
        [paywallProxy presentPaywallIfNeededWithOptions:options paywallResultHandler:^(NSString *result) {
            if (paywallResultCallback) {
                paywallResultCallback([result UTF8String]);
                paywallResultCallback = NULL;
            }
        }];
        
        NSLog(@"Presenting paywall if needed for entitlement: %s, offering: %s, displayCloseButton: %d", 
              requiredEntitlementIdentifier, 
              offeringIdentifier ? offeringIdentifier : "default", 
              displayCloseButton);
    } else {
        NSLog(@"Presenting paywall if needed requires iOS 15.0 or later");
        if (callback) {
            callback("{\"error\": \"iOS 15.0 required\"}");
        }
    }
}

#pragma mark - Customer Center Methods

void presentCustomerCenter(void (*callback)(void)) {
    if (@available(iOS 15.0, *)) {
        if (!customerCenterProxy) {
            initializeRevenueCatUI();
        }
        
        // Store the callback
        customerCenterCallback = callback;
        
        // Present customer center with result handler
        [customerCenterProxy presentWithResultHandler:^{
            if (customerCenterCallback) {
                customerCenterCallback();
                customerCenterCallback = NULL;
            }
        }];
        
        NSLog(@"Presenting customer center");
    } else {
        NSLog(@"Presenting customer center requires iOS 15.0 or later");
        if (callback) {
            callback();
        }
    }
}

#pragma mark - Support Methods

bool isRevenueCatUISupported() {
    if (@available(iOS 15.0, *)) {
        return [PaywallProxy class] != nil && [CustomerCenterProxy class] != nil;
    }
    return false;
} 