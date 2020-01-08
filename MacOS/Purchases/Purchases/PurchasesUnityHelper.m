//
//  PurchasesUnityHelper.m
//
//  Created by RevenueCat, Inc. on 5/30/18.
//

#import <Foundation/Foundation.h>
#import <StoreKit/StoreKit.h>
#import <AdSupport/AdSupport.h>
#import "RCPurchases.h"
#import "RCPurchaserInfo.h"
#import "RCCommonFunctionality.h"
#import "RCPurchaserInfo+HybridAdditions.h"
#import "RCErrorContainer.h"
#import "PurchasesUnityHelper.h"

static NSString *const RECEIVE_PRODUCTS = @"_receiveProducts";
static NSString *const CREATE_ALIAS = @"_createAlias";
static NSString *const RECEIVE_PURCHASER_INFO = @"_receivePurchaserInfo";
static NSString *const RESTORE_TRANSACTIONS = @"_restoreTransactions";
static NSString *const IDENTIFY = @"_identify";
static NSString *const RESET = @"_reset";
static NSString *const MAKE_PURCHASE = @"_makePurchase";
static NSString *const GET_OFFERINGS = @"_getOfferings";
static NSString *const GET_PURCHASER_INFO = @"_getPurchaserInfo";
static NSString *const CHECK_ELIGIBILITY = @"_checkTrialOrIntroductoryPriceEligibility";

#pragma mark Utility Methods

NSString *convertCString(const char *string) {
    if (string)
        return [NSString stringWithUTF8String:string];
    else
        return nil;
}

char *makeStringCopy(NSString *nstring) {
    if ((!nstring) || (nil == nstring) || (nstring == (id) [NSNull null]) || (0 == nstring.length))
        return NULL;

    const char *string = [nstring UTF8String];

    if (string == NULL)
        return NULL;

    char *res = (char *) malloc(strlen(string) + 1);
    strcpy(res, string);

    return res;
}

#pragma mark RCPurchases Wrapper

@implementation RCUnityHelperDelegate

- (void)setupPurchases:(NSString *)apiKey appUserID:(NSString *)appUserID gameObject:(NSString *)gameObject observerMode:(BOOL)observerMode {
    [[RCPurchases sharedPurchases] setDelegate:nil];
    self.products = nil;
    self.gameObject = nil;

    [RCPurchases configureWithAPIKey:apiKey appUserID:appUserID observerMode:observerMode];
    self.gameObject = gameObject;
    [[RCPurchases sharedPurchases] setDelegate:self];
}

- (void)getProducts:(NSArray *)productIdentifiers
               type:(NSString *)type {
    [RCCommonFunctionality getProductInfo:productIdentifiers completionBlock:^(NSArray<NSDictionary *> *productObjects) {
        NSDictionary *response = @{
            @"products": productObjects
        };
        [self sendJSONObject:response toMethod:RECEIVE_PRODUCTS];
    }];
}

- (void)purchaseProduct:(NSString *)productIdentifier {
    [RCCommonFunctionality purchaseProduct:productIdentifier completionBlock:^(NSDictionary *_Nullable responseDictionary, RCErrorContainer *_Nullable error) {
        NSMutableDictionary *response;
        if (error) {
            response = [NSMutableDictionary new];
            response[@"error"] = error.info;
            response[@"userCancelled"] = error.info[@"userCancelled"];
            [self sendJSONObject:response toMethod:MAKE_PURCHASE];
        } else {
            response = [NSMutableDictionary dictionaryWithDictionary:responseDictionary];
            response[@"userCancelled"] = false;
        }
        [self sendJSONObject:response toMethod:MAKE_PURCHASE];
    }];
}

- (void)purchasePackage:(NSString *)packageIdentifier
     offeringIdentifier:(NSString *)offeringIdentifier {
    [RCCommonFunctionality purchasePackage:packageIdentifier offering:offeringIdentifier completionBlock:^(NSDictionary *_Nullable responseDictionary, RCErrorContainer *_Nullable error) {
        NSMutableDictionary *response;
        if (error) {
            response = [NSMutableDictionary new];
            response[@"error"] = error.info;
            response[@"userCancelled"] = error.info[@"userCancelled"];
            [self sendJSONObject:response toMethod:MAKE_PURCHASE];
        } else {
            response = [NSMutableDictionary dictionaryWithDictionary:responseDictionary];
            response[@"userCancelled"] = false;
        }
        [self sendJSONObject:response toMethod:MAKE_PURCHASE];
    }];
}

- (void)restoreTransactions {
    [RCCommonFunctionality restoreTransactionsWithCompletionBlock:[self getPurchaserInfoCompletionBlockFor:RESTORE_TRANSACTIONS]];
}

- (void)addAttributionData:(NSString *)dataJSON network:(int)network networkUserId:(NSString * _Nullable)networkUserId
{
    NSError *error = nil;
    NSDictionary *data = [NSJSONSerialization JSONObjectWithData:[dataJSON dataUsingEncoding:NSUTF8StringEncoding]
                                                         options:0
                                                           error:&error];

    if (error) {
        NSLog(@"Error reading attribution data: %@", error.localizedDescription);
        return;
    }
    
    [RCCommonFunctionality addAttributionData:data network:network networkUserId:networkUserId];
}

- (void)createAlias:(NSString *)newAppUserID {
    [RCCommonFunctionality createAlias:newAppUserID completionBlock:[self getPurchaserInfoCompletionBlockFor:CREATE_ALIAS]];
}

- (void)identify:(NSString *)appUserID {
    [RCCommonFunctionality identify:appUserID completionBlock:[self getPurchaserInfoCompletionBlockFor:IDENTIFY]];
}

- (void)reset {
    [RCCommonFunctionality resetWithCompletionBlock:[self getPurchaserInfoCompletionBlockFor:RESET]];
}

- (void)setAllowSharingStoreAccount:(BOOL)allow {
    [RCCommonFunctionality setAllowSharingStoreAccount:allow];
}

- (void)getOfferings {
    [RCCommonFunctionality getOfferingsWithCompletionBlock:^(NSDictionary *_Nullable responseDictionary, RCErrorContainer *_Nullable error) {
        NSMutableDictionary *response = [NSMutableDictionary new];
        if (error) {
            response[@"error"] = error.info;
        } else {
            response[@"offerings"] = responseDictionary;
        }
        
        [self sendJSONObject:response toMethod:GET_OFFERINGS];
    }];
}

- (void)setDebugLogsEnabled:(BOOL)enabled {
    [RCCommonFunctionality setDebugLogsEnabled:enabled];
}

- (void)getPurchaserInfo {
    [RCCommonFunctionality getPurchaserInfoWithCompletionBlock:[self getPurchaserInfoCompletionBlockFor:GET_PURCHASER_INFO]];
}

-  (void)setFinishTransactions:(BOOL)finishTransactions
{
    [RCCommonFunctionality setFinishTransactions:finishTransactions];
}

- (void)setAutomaticAppleSearchAdsAttributionCollection:(BOOL)enabled
{
    [RCCommonFunctionality setAutomaticAppleSearchAdsAttributionCollection:enabled];
}

- (void)purchases:(RCPurchases *)purchases didReceiveUpdatedPurchaserInfo:(RCPurchaserInfo *)purchaserInfo {
    [self sendJSONObject:purchaserInfo.dictionary toMethod:RECEIVE_PURCHASER_INFO];
}

- (char *)getAppUserID
{
    return makeStringCopy([RCCommonFunctionality appUserID]);
}

- (BOOL)isAnonymous
{
    return @([RCCommonFunctionality isAnonymous]);
}

- (void)checkTrialOrIntroductoryPriceEligibility:(NSArray *)productIdentifiers
{
    [RCCommonFunctionality checkTrialOrIntroductoryPriceEligibility:productIdentifiers completionBlock:^(NSDictionary<NSString *,NSDictionary *> * _Nonnull responseDictionary) {
        NSDictionary *response = @{
            @"keys": responseDictionary.allKeys,
            @"values": responseDictionary.allValues,
        };
        [self sendJSONObject:response toMethod:CHECK_ELIGIBILITY];
    }];
}

#pragma mark Helper Methods

- (void)sendJSONObject:(NSDictionary *)jsonObject toMethod:(NSString *)methodName
{
    NSError *error = nil;
    NSData *responseJSONData = [NSJSONSerialization dataWithJSONObject:jsonObject options:0 error:&error];

    if (error) {
        NSLog(@"Error serializing products: %@", error.localizedDescription);
        return;
    }

    if (responseJSONData) {
        NSString *json = [[NSString alloc] initWithData:responseJSONData encoding:NSUTF8StringEncoding];
        NSLog(@"json = %@", json);
//        UnitySendMessage(self.gameObject.UTF8String, methodName.UTF8String, json.UTF8String);
    }
}

- (void (^)(NSDictionary *, RCErrorContainer *))getPurchaserInfoCompletionBlockFor:(NSString *)method {
    return ^(NSDictionary *_Nullable responseDictionary, RCErrorContainer *_Nullable error) {
        NSMutableDictionary *response = [NSMutableDictionary new];

        if (error) {
            response[@"error"] = error.info;
        } else {
            response[@"purchaserInfo"] = responseDictionary;
        }
        [self sendJSONObject:response toMethod:method];
    };
}

@end
