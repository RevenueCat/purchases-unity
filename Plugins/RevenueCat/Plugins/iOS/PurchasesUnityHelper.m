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
#import "RCOffering+Protected.h"

static NSString *const RECEIVE_PRODUCTS = @"_receiveProducts";
static NSString *const CREATE_ALIAS = @"_createAlias";
static NSString *const RECEIVE_PURCHASER_INFO = @"_receivePurchaserInfo";
static NSString *const RESTORE_TRANSACTIONS = @"_restoreTransactions";
static NSString *const IDENTIFY = @"_identify";
static NSString *const RESET = @"_reset";
static NSString *const MAKE_PURCHASE = @"_makePurchase";
static NSString *const GET_ENTITLEMENTS = @"_getEntitlements";
static NSString *const GET_PURCHASER_INFO = @"_getPurchaserInfo";
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

@interface RCUnityHelperDelegate : NSObject <RCPurchasesDelegate>
@property(nonatomic) NSDictionary *products;
@property(nonatomic) NSString *gameObject;
@end

@implementation RCUnityHelperDelegate

- (void)setupPurchases:(NSString *)apiKey appUserID:(NSString *)appUserID gameObject:(NSString *)gameObject observerMode:(BOOL)observerMode {
    [[RCPurchases sharedPurchases] setDelegate:nil];
    self.products = nil;
    self.gameObject = nil;

    [RCPurchases configureWithAPIKey:apiKey appUserID:appUserID observerMode:observerMode];
    self.gameObject = gameObject;
    [[RCPurchases sharedPurchases] setDelegate:self];
}

- (NSDictionary *)productJSON:(SKProduct *)p {
    NSNumberFormatter *formatter = [[NSNumberFormatter alloc] init];
    formatter.numberStyle = NSNumberFormatterCurrencyStyle;
    formatter.locale = p.priceLocale;
    NSMutableDictionary *d = [NSMutableDictionary dictionaryWithDictionary:@{
            @"identifier": p.productIdentifier ?: @"",
            @"description": p.localizedDescription ?: @"",
            @"title": p.localizedTitle ?: @"",
            @"price": @(p.price.floatValue),
            @"priceString": [formatter stringFromNumber:p.price]
    }];
    
    NSString *currencyCode = nil;
    if(@available(iOS 10.0, *)) {
        currencyCode = p.priceLocale.currencyCode;
    } else {
        currencyCode = [p.priceLocale objectForKey:NSLocaleCurrencyCode];
    }
    d[@"currencyCode"] = currencyCode ? currencyCode : [NSNull null];

    if (@available(iOS 11.2, *)) {
        if (p.introductoryPrice) {
            d[@"introPrice"] = @(p.introductoryPrice.price.floatValue);
            if (p.introductoryPrice.price) {
                d[@"introPriceString"] = [formatter stringFromNumber:p.introductoryPrice.price];
            }
            d[@"introPricePeriod"] = [self normalizeSubscriptionPeriod:p.introductoryPrice.subscriptionPeriod];
            d[@"introPricePeriodUnit"] = [self normalizeSubscriptionPeriodUnit:p.introductoryPrice.subscriptionPeriod.unit];
            d[@"introPricePeriodNumberOfUnits"] = @(p.introductoryPrice.subscriptionPeriod.numberOfUnits);
            d[@"introPriceCycles"] = @(p.introductoryPrice.numberOfPeriods);
        }
    }
    
    return d;
}

- (NSString *)normalizeSubscriptionPeriod:(SKProductSubscriptionPeriod *)subscriptionPeriod API_AVAILABLE(ios(11.2)){
    NSString *unit;
    switch (subscriptionPeriod.unit) {
        case SKProductPeriodUnitDay:
            unit = @"D";
        case SKProductPeriodUnitWeek:
            unit = @"W";
        case SKProductPeriodUnitMonth:
            unit = @"M";
        case SKProductPeriodUnitYear:
            unit = @"Y";
    }
    return [NSString stringWithFormat:@"%@%@%@", @"P", @(subscriptionPeriod.numberOfUnits), unit];
}

- (NSString *)normalizeSubscriptionPeriodUnit:(SKProductPeriodUnit)subscriptionPeriodUnit API_AVAILABLE(ios(11.2)){
    switch (subscriptionPeriodUnit) {
        case SKProductPeriodUnitDay:
            return @"DAY";
        case SKProductPeriodUnitWeek:
            return @"WEEK";
        case SKProductPeriodUnitMonth:
            return @"MONTH";
        case SKProductPeriodUnitYear:
            return @"YEAR";
    }
}

- (NSDictionary *)errorJSON:(NSError *)error {
    NSMutableDictionary *dict = [NSMutableDictionary dictionaryWithDictionary:@{
                                                                                @"message": error.localizedDescription,
                                                                                @"code": @(error.code)
                                                                                }];
    if (error.userInfo[NSUnderlyingErrorKey]) {
        dict[@"underlyingErrorMessage"] = ((NSError *)error.userInfo[NSUnderlyingErrorKey]).localizedDescription;
    }
    return dict;
}

- (NSDictionary *)purchaserInfoJSON:(RCPurchaserInfo *)info {
    NSArray *productIdentifiers = info.allPurchasedProductIdentifiers.allObjects;
    NSArray *sorted = [productIdentifiers sortedArrayUsingSelector:@selector(compare:)];

    NSMutableArray *expirationDateKeys = [NSMutableArray new];
    NSMutableArray *expirationDateValues = [NSMutableArray new];

    for (NSString *productIdentifier in sorted) {
        NSDate *date = [info expirationDateForProductIdentifier:productIdentifier];
        if (date) {
            [expirationDateKeys addObject:productIdentifier];
            [expirationDateValues addObject:@(date.timeIntervalSince1970)];
        }
    }

    return @{
            @"activeSubscriptions": info.activeSubscriptions.allObjects,
            @"allPurchasedProductIdentifiers": info.allPurchasedProductIdentifiers.allObjects,
            @"latestExpirationDate": info.latestExpirationDate ? @(info.latestExpirationDate.timeIntervalSince1970) : [NSNull null],
            @"allExpirationDateKeys": expirationDateKeys,
            @"allExpirationDateValues": expirationDateValues
    };
}

- (NSMutableArray *)entitlementMapJSON:(RCEntitlements *)entitlements {
    NSMutableArray *entitlementsArray = [NSMutableArray new];
    NSMutableDictionary *productByID = [NSMutableDictionary new];

    for (NSString *entId in entitlements) {
        RCEntitlement *entitlement = entitlements[entId];
        NSMutableArray *offeringsArray = [NSMutableArray new];

        if (entitlement) {
            NSDictionary<NSString *, RCOffering *> *offerings = entitlement.offerings;
            if (offerings) {
                [offerings enumerateKeysAndObjectsUsingBlock:^(NSString *offeringId, RCOffering *offering, BOOL *stop) {
                    NSMutableDictionary *offeringJSON = [NSMutableDictionary new];
                    offeringJSON[@"offeringId"] = offeringId;

                    SKProduct *product = offering.activeProduct;
                    if (product != nil) {
                        productByID[product.productIdentifier] = product;
                        offeringJSON[@"product"] = [self productJSON:product];
                    }
                    [offeringsArray addObject:offeringJSON];
                }];
            }
        }
        [entitlementsArray addObject:@{
                @"entitlementId": entId,
                @"offerings": offeringsArray
        }];
    }

    self.products = [NSDictionary dictionaryWithDictionary:productByID];
    return entitlementsArray;
}

- (void)getProducts:(NSArray *)productIdentifiers
               type:(NSString *)type {
    [[RCPurchases sharedPurchases] productsWithIdentifiers:productIdentifiers completionBlock:^(NSArray<SKProduct *> *_Nonnull products) {
        NSMutableDictionary *productByID = [NSMutableDictionary new];
        NSMutableArray *productObjects = [NSMutableArray new];

        for (SKProduct *p in products) {
            productByID[p.productIdentifier] = p;
            [productObjects addObject:[self productJSON:p]];
        }

        self.products = [NSDictionary dictionaryWithDictionary:productByID];
        NSDictionary *response = @{
                @"products": productObjects
        };
        [self sendJSONObject:response toMethod:RECEIVE_PRODUCTS];
    }];
}

- (void)makePurchase:(NSString *)productIdentifier {
    SKProduct *product = self.products[productIdentifier];
    if (product == nil) {
        NSLog(@"No product found for identifier %@", productIdentifier);
        return;
    }

    [[RCPurchases sharedPurchases] makePurchase:product
                            withCompletionBlock:^(SKPaymentTransaction * _Nullable transaction, RCPurchaserInfo * _Nullable info, NSError * _Nullable error, BOOL userCancelled) {
                                NSMutableDictionary *response = [NSMutableDictionary new];
                                if (transaction) {
                                    response[@"productIdentifier"] = transaction.payment.productIdentifier;
                                }
                                if (info) {
                                    response[@"purchaserInfo"] = [self purchaserInfoJSON:info];
                                }
                                if (error) {
                                    response[@"error"] = [self errorJSON:error];
                                }
                                response[@"userCancelled"] = @(userCancelled);
                                [self sendJSONObject:response toMethod:MAKE_PURCHASE];
                            }];
}

- (void)restoreTransactions {
    [[RCPurchases sharedPurchases] restoreTransactionsWithCompletionBlock:[self getPurchaserInfoCompletionBlockFor:RESTORE_TRANSACTIONS]];
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

    [RCPurchases addAttributionData:data fromNetwork:network forNetworkUserId:networkUserId];
}

- (void)createAlias:(NSString *)newAppUserID {
    [[RCPurchases sharedPurchases] createAlias:newAppUserID completionBlock:[self getPurchaserInfoCompletionBlockFor:CREATE_ALIAS]];
}

- (void)identify:(NSString *)appUserID {
    [[RCPurchases sharedPurchases] identify:appUserID completionBlock:[self getPurchaserInfoCompletionBlockFor:IDENTIFY]];
}

- (void)reset {
    [[RCPurchases sharedPurchases] resetWithCompletionBlock:[self getPurchaserInfoCompletionBlockFor:RESET]];
}

- (void)setAllowSharingStoreAccount:(BOOL)allow {
    [[RCPurchases sharedPurchases] setAllowSharingAppStoreAccount:allow];
}

- (void)getEntitlements {
    [[RCPurchases sharedPurchases] entitlementsWithCompletionBlock:^(RCEntitlements * _Nullable entitlementMap, NSError * _Nullable error) {
        NSMutableDictionary *response = [NSMutableDictionary new];
        if (entitlementMap) {
            response[@"entitlements"] = [self entitlementMapJSON:entitlementMap];
        }
        if (error) {
            response[@"error"] = [self errorJSON:error];
        }
        [self sendJSONObject:response toMethod:GET_ENTITLEMENTS];
    }];
}

- (void)setDebugLogsEnabled:(BOOL)enabled {
    [RCPurchases setDebugLogsEnabled:enabled];
}

- (void)getPurchaserInfo {
    [[RCPurchases sharedPurchases] purchaserInfoWithCompletionBlock:[self getPurchaserInfoCompletionBlockFor:GET_PURCHASER_INFO]];
}

-  (void)setFinishTransactions:(BOOL)finishTransactions
{
    [RCPurchases sharedPurchases].finishTransactions = finishTransactions;
}

- (void)setAutomaticAppleSearchAdsAttributionCollection:(BOOL)enabled
{
    [RCPurchases setAutomaticAppleSearchAdsAttributionCollection:enabled];
}

- (void)purchases:(RCPurchases *)purchases didReceiveUpdatedPurchaserInfo:(RCPurchaserInfo *)purchaserInfo {
    [self sendJSONObject:[self purchaserInfoJSON:purchaserInfo] toMethod:RECEIVE_PURCHASER_INFO];
}

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
        UnitySendMessage(self.gameObject.UTF8String, methodName.UTF8String, json.UTF8String);
    }
}

- (void (^)(RCPurchaserInfo *, NSError *))getPurchaserInfoCompletionBlockFor:(NSString *)method {
    return ^(RCPurchaserInfo *info, NSError *error) {
        NSMutableDictionary *response = [NSMutableDictionary new];

        if (error) {
            response[@"error"] = [self errorJSON:error];
        } else {
            response[@"purchaserInfo"] = [self purchaserInfoJSON:info];
        }

        [self sendJSONObject:response toMethod:method];
    };
}

- (char *)getAppUserID
{
    return makeStringCopy([RCPurchases sharedPurchases].appUserID);
}

@end

#pragma mark Bridging Methods

static RCUnityHelperDelegate *_RCUnityHelper;

static RCUnityHelperDelegate *_RCUnityHelperShared() {
    if (_RCUnityHelper == nil) {
        _RCUnityHelper = [[RCUnityHelperDelegate alloc] init];
    }
    return _RCUnityHelper;
}

void _RCSetupPurchases(const char *gameObject, const char *apiKey, const char *appUserID, const BOOL observerMode) {
    [_RCUnityHelperShared() setupPurchases:convertCString(apiKey) appUserID:convertCString(appUserID) gameObject:convertCString(gameObject) observerMode:observerMode];
}

void _RCGetProducts(const char *productIdentifiersJSON, const char *type) {
    NSError *error = nil;
    NSDictionary *productsRequest = [NSJSONSerialization JSONObjectWithData:[convertCString(productIdentifiersJSON) dataUsingEncoding:NSUTF8StringEncoding] options:0 error:&error];

    if (error) {
        NSLog(@"Error parsing productIdentifiers JSON: %s %@", productIdentifiersJSON, error.localizedDescription);
        return;
    }

    [_RCUnityHelperShared() getProducts:productsRequest[@"productIdentifiers"] type:convertCString(type)];
}

void _RCMakePurchase(const char *productIdentifier, const char *type) {
    [_RCUnityHelperShared() makePurchase:convertCString(productIdentifier)];
}

void _RCRestoreTransactions() {
    [_RCUnityHelperShared() restoreTransactions];
}

void _RCAddAttributionData(const int network, const char *data, const char *networkUserId)
{
    [_RCUnityHelperShared() addAttributionData:convertCString(data) network:network networkUserId:convertCString(networkUserId)];
}

void _RCCreateAlias(const char *newAppUserID) {
    [_RCUnityHelperShared() createAlias:convertCString(newAppUserID)];
}

void _RCIdentify(const char *appUserID) {
    [_RCUnityHelperShared() identify:convertCString(appUserID)];
}

void _RCReset() {
    [_RCUnityHelperShared() reset];
}

void _RCSetFinishTransactions(const BOOL finishTransactions) {
    [_RCUnityHelperShared() setFinishTransactions:finishTransactions];
}
void _RCSetAllowSharingStoreAccount(const BOOL allow) {
    [_RCUnityHelperShared() setAllowSharingStoreAccount:allow];
}

void _RCGetEntitlements() {
    [_RCUnityHelperShared() getEntitlements];
}

void _RCSetDebugLogsEnabled(const BOOL enabled) {
    [_RCUnityHelperShared() setDebugLogsEnabled:enabled];
}

void _RCGetPurchaserInfo() {
    [_RCUnityHelperShared() getPurchaserInfo];
}

char * _RCGetAppUserID() {
    return [_RCUnityHelperShared() getAppUserID];
}

void _RCSyncPurchases() {
    // NOOP
}

void _RCSetAutomaticAppleSearchAdsAttributionCollection(const BOOL enabled) {
    [_RCUnityHelperShared() setAutomaticAppleSearchAdsAttributionCollection:enabled];
}
