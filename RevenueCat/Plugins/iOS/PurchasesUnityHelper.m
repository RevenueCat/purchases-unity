//
//  PurchasesUnityHelper.m
//
//  Created by RevenueCat, Inc. on 5/30/18.
//

#import <Foundation/Foundation.h>
#import <StoreKit/StoreKit.h>
#import <AdSupport/AdSupport.h>
#import <PurchasesHybridCommon/PurchasesHybridCommon.h>

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
static NSString *const CAN_MAKE_PAYMENTS = @"_canMakePayments";

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

- (void)setupPurchases:(NSString *)apiKey
             appUserID:(nullable NSString *)appUserID
            gameObject:(NSString *)gameObject
          observerMode:(BOOL)observerMode
 userDefaultsSuiteName:(nullable NSString *)userDefaultsSuiteName
             useAmazon:(BOOL)useAmazon {
    [[RCPurchases sharedPurchases] setDelegate:nil];
    self.products = nil;
    self.gameObject = nil;

    [RCPurchases configureWithAPIKey:apiKey
                           appUserID:appUserID
                        observerMode:observerMode
               userDefaultsSuiteName:userDefaultsSuiteName
                      platformFlavor:self.platformFlavor
               platformFlavorVersion:self.platformFlavorVersion];
    
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
    [RCCommonFunctionality purchaseProduct:productIdentifier
                   signedDiscountTimestamp:nil
                           completionBlock:^(NSDictionary *_Nullable responseDictionary, RCErrorContainer *_Nullable error) {
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
    [RCCommonFunctionality purchasePackage:packageIdentifier
                                  offering:offeringIdentifier
                   signedDiscountTimestamp:nil
                           completionBlock:^(NSDictionary *_Nullable responseDictionary, RCErrorContainer *_Nullable error) {
        NSMutableDictionary *response;
        if (error) {
            response = [NSMutableDictionary new];
            response[@"error"] = error.info;
            response[@"userCancelled"] = error.info[@"userCancelled"];
            [self sendJSONObject:response toMethod:MAKE_PURCHASE];
        } else {
            response = [NSMutableDictionary dictionaryWithDictionary:responseDictionary];
            response[@"userCancelled"] = @NO;
        }
        [self sendJSONObject:response toMethod:MAKE_PURCHASE];
    }];
}

- (void)restoreTransactions {
    [RCCommonFunctionality restoreTransactionsWithCompletionBlock:[self getPurchaserInfoCompletionBlockFor:RESTORE_TRANSACTIONS]];
}

- (void)syncPurchases {
    // on Android, syncPurchases doesn't have a completion block. So instead of
    // calling getPurchaserInfoCompletionBlockFor:SYNC_PURCHASES, we just
    // print the response, to match Android behavior. 
    [RCCommonFunctionality syncPurchasesWithCompletionBlock:^(NSDictionary *_Nullable responseDictionary, RCErrorContainer *_Nullable error) {
        NSLog(@"received syncPurchases response: \n purchaserInfo: %@ \n error:%@", responseDictionary, error);
    }];
}

- (void)addAttributionData:(NSString *)dataJSON network:(int)network networkUserId:(NSString * _Nullable)networkUserId {
    NSError *error = nil;
    NSDictionary *data = [NSJSONSerialization JSONObjectWithData:[dataJSON dataUsingEncoding:NSUTF8StringEncoding]
                                                         options:0
                                                           error:&error];

    if (error) {
        NSLog(@"Error reading attribution data: %@", error.localizedDescription);
        return;
    }

#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Wdeprecated-declarations"
    [RCCommonFunctionality addAttributionData:data network:network networkUserId:networkUserId];
#pragma clang diagnostic pop
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

- (void)setProxyURLString:(nullable NSString *)proxyURLString {
    [RCCommonFunctionality setProxyURLString:proxyURLString];
}

- (void)getPurchaserInfo {
    [RCCommonFunctionality getPurchaserInfoWithCompletionBlock:[self getPurchaserInfoCompletionBlockFor:GET_PURCHASER_INFO]];
}

-  (void)setFinishTransactions:(BOOL)finishTransactions {
    [RCCommonFunctionality setFinishTransactions:finishTransactions];
}

- (void)setAutomaticAppleSearchAdsAttributionCollection:(BOOL)enabled {
    [RCCommonFunctionality setAutomaticAppleSearchAdsAttributionCollection:enabled];
}

- (void)purchases:(RCPurchases *)purchases didReceiveUpdatedPurchaserInfo:(RCPurchaserInfo *)purchaserInfo {
    NSMutableDictionary *response = [NSMutableDictionary new];
    response[@"purchaserInfo"] = purchaserInfo.dictionary;
    [self sendJSONObject:response toMethod:RECEIVE_PURCHASER_INFO];
}

- (char *)getAppUserID {
    return makeStringCopy([RCCommonFunctionality appUserID]);
}

- (BOOL)isAnonymous {
    return @([RCCommonFunctionality isAnonymous]);
}

- (void)checkTrialOrIntroductoryPriceEligibility:(NSArray *)productIdentifiers {
    [RCCommonFunctionality checkTrialOrIntroductoryPriceEligibility:productIdentifiers
                                                    completionBlock:^(NSDictionary<NSString *,NSDictionary *> * _Nonnull responseDictionary) {
        [self sendJSONObject:responseDictionary toMethod:CHECK_ELIGIBILITY];
    }];
}

- (void)invalidatePurchaserInfoCache {
    [RCCommonFunctionality invalidatePurchaserInfoCache];
}

- (void)presentCodeRedemptionSheet {
    if (@available(iOS 14.0, *)) {
         [RCCommonFunctionality presentCodeRedemptionSheet];
     } else {
         NSLog(@"[Purchases] Warning: tried to present codeRedemptionSheet, but it's only available on iOS 14.0 or greater.");
     }
}

- (void)setSimulatesAskToBuyInSandbox:(BOOL)enabled {
     [RCCommonFunctionality setSimulatesAskToBuyInSandbox:enabled];
}

- (void)canMakePaymentsWithFeatures:(NSArray<NSNumber *> *)features {
    BOOL canMakePayments = [RCCommonFunctionality canMakePaymentsWithFeatures:features];
    
    NSDictionary *response = @{
        @"canMakePayments": @(canMakePayments)
    };
    
    [self sendJSONObject:response toMethod:CAN_MAKE_PAYMENTS];
}
#pragma mark - Subcriber Attributes

- (void)setAttributes:(NSDictionary<NSString *, NSString *> *)attributes {
    [RCCommonFunctionality setAttributes:attributes];
}

- (void)setEmail:(nullable NSString *)email {
    [RCCommonFunctionality setEmail:email];
}

- (void)setPhoneNumber:(nullable NSString *)phoneNumber {
    [RCCommonFunctionality setPhoneNumber:phoneNumber];
}

- (void)setDisplayName:(nullable NSString *)displayName {
    [RCCommonFunctionality setDisplayName:displayName];
}

- (void)setPushToken:(nullable NSString *)pushToken {
    [RCCommonFunctionality setPushToken:pushToken];
}

#pragma mark Attribution IDs

- (void)collectDeviceIdentifiers {
    [RCCommonFunctionality collectDeviceIdentifiers];
}

- (void)setAdjustID:(nullable NSString *)adjustID {
    [RCCommonFunctionality setAdjustID:adjustID];
}

- (void)setAppsflyerID:(nullable NSString *)appsflyerID {
    [RCCommonFunctionality setAppsflyerID:appsflyerID];
}

- (void)setFBAnonymousID:(nullable NSString *)fbAnonymousID {
    [RCCommonFunctionality setFBAnonymousID:fbAnonymousID];
}

- (void)setMparticleID:(nullable NSString *)mparticleID {
    [RCCommonFunctionality setMparticleID:mparticleID];
}

- (void)setOnesignalID:(nullable NSString *)onesignalID {
    [RCCommonFunctionality setOnesignalID:onesignalID];
}

#pragma mark Campaign parameters

- (void)setMediaSource:(nullable NSString *)mediaSource {
    [RCCommonFunctionality setMediaSource:mediaSource];
}

- (void)setCampaign:(nullable NSString *)campaign {
    [RCCommonFunctionality setCampaign:campaign];
}

- (void)setAdGroup:(nullable NSString *)adGroup {
    [RCCommonFunctionality setAdGroup:adGroup];
}

- (void)setAd:(nullable NSString *)ad {
    [RCCommonFunctionality setAd:ad];
}

- (void)setKeyword:(nullable NSString *)keyword {
    [RCCommonFunctionality setKeyword:keyword];
}

- (void)setCreative:(nullable NSString *)creative {
    [RCCommonFunctionality setCreative:creative];
}

#pragma mark Helper Methods

- (void)sendJSONObject:(NSDictionary *)jsonObject toMethod:(NSString *)methodName {
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

- (NSString *)platformFlavor { 
    return @"unity";
}

- (NSString *)platformFlavorVersion { 
    return @"4.0.0-amazon.alpha.uiap";
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

void _RCSetupPurchases(const char *gameObject,
                       const char *apiKey,
                       const char *appUserID,
                       const BOOL observerMode,
                       const char *userDefaultsSuiteName) {
    [_RCUnityHelperShared() setupPurchases:convertCString(apiKey)
                                 appUserID:convertCString(appUserID)
                                gameObject:convertCString(gameObject)
                              observerMode:observerMode
                     userDefaultsSuiteName:convertCString(userDefaultsSuiteName)];
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

void _RCPurchaseProduct(const char *productIdentifier) {
    [_RCUnityHelperShared() purchaseProduct:convertCString(productIdentifier)];
}

void _RCPurchasePackage(const char *packageIdentifier, const char *offeringIdentifier) {
    [_RCUnityHelperShared() purchasePackage:convertCString(packageIdentifier) offeringIdentifier:convertCString(offeringIdentifier)];
}

void _RCRestoreTransactions() {
    [_RCUnityHelperShared() restoreTransactions];
}

void _RCSyncPurchases() {
    [_RCUnityHelperShared() syncPurchases];
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

void _RCGetOfferings() {
    [_RCUnityHelperShared() getOfferings];
}

void _RCSetDebugLogsEnabled(const BOOL enabled) {
    [_RCUnityHelperShared() setDebugLogsEnabled:enabled];
}

void _RCSetProxyURLString(const char *proxyURLString) {
    [_RCUnityHelperShared() setProxyURLString:convertCString(proxyURLString)];
}

void _RCSetSimulatesAskToBuyInSandbox(const BOOL enabled) {
    [_RCUnityHelperShared() setSimulatesAskToBuyInSandbox:enabled];
}

void _RCGetPurchaserInfo() {
    [_RCUnityHelperShared() getPurchaserInfo];
}

char * _RCGetAppUserID() {
    return [_RCUnityHelperShared() getAppUserID];
}

void _RCSetAutomaticAppleSearchAdsAttributionCollection(const BOOL enabled) {
    [_RCUnityHelperShared() setAutomaticAppleSearchAdsAttributionCollection:enabled];
}

void _RCIsAnonymous() {
    [_RCUnityHelperShared() isAnonymous];
}

void _RCCheckTrialOrIntroductoryPriceEligibility(const char *productIdentifiersJSON) {
    NSError *error = nil;
    NSDictionary *productsRequest = [NSJSONSerialization JSONObjectWithData:[convertCString(productIdentifiersJSON) dataUsingEncoding:NSUTF8StringEncoding] options:0 error:&error];

    if (error) {
        NSLog(@"Error parsing productIdentifiers JSON: %s %@", productIdentifiersJSON, error.localizedDescription);
        return;
    }

    [_RCUnityHelperShared() checkTrialOrIntroductoryPriceEligibility:productsRequest[@"productIdentifiers"]];
}

void _RCInvalidatePurchaserInfoCache() {
    [_RCUnityHelperShared() invalidatePurchaserInfoCache];
}

void _RCPresentCodeRedemptionSheet() {
    [_RCUnityHelperShared() presentCodeRedemptionSheet];
}

void _RCSetAttributes(const char* attributesJSON) {
    NSError *error = nil;
    NSData *attributesAsData = [convertCString(attributesJSON) dataUsingEncoding:NSUTF8StringEncoding];
    NSDictionary *attributes = [NSJSONSerialization JSONObjectWithData:attributesAsData
                                                               options:0
                                                                 error:&error];
    
    if (error) {
        NSLog(@"Error parsing attributes JSON: %s %@", attributesJSON, error.localizedDescription);
        return;
    }
    
    [_RCUnityHelperShared() setAttributes:attributes];
}

void _RCSetEmail(const char *email) {
    [_RCUnityHelperShared() setEmail:convertCString(email)];
}

void _RCSetPhoneNumber(const char *phoneNumber) {
    [_RCUnityHelperShared() setPhoneNumber:convertCString(phoneNumber)];
}

void _RCSetDisplayName(const char *displayName) {
    [_RCUnityHelperShared() setDisplayName:convertCString(displayName)];
}

void _RCSetPushToken(const char *pushToken) {
    [_RCUnityHelperShared() setPushToken:convertCString(pushToken)];
}

void _RCSetAdjustID(const char *adjustID) {
    [_RCUnityHelperShared() setAdjustID:convertCString(adjustID)];
}

void _RCSetAppsflyerID(const char *appsflyerID) {
    [_RCUnityHelperShared() setAppsflyerID:convertCString(appsflyerID)];
}

void _RCSetFBAnonymousID(const char *fbAnonymousID) {
    [_RCUnityHelperShared() setFBAnonymousID:convertCString(fbAnonymousID)];
}

void _RCSetMparticleID(const char *mparticleID) {
    [_RCUnityHelperShared() setMparticleID:convertCString(mparticleID)];
}

void _RCSetOnesignalID(const char *onesignalID) {
    [_RCUnityHelperShared() setOnesignalID:convertCString(onesignalID)];
}

void _RCSetMediaSource(const char *mediaSource) {
    [_RCUnityHelperShared() setMediaSource:convertCString(mediaSource)];
}

void _RCSetCampaign(const char *campaign) {
    [_RCUnityHelperShared() setCampaign:convertCString(campaign)];
}

void _RCSetAdGroup(const char *adGroup) {
    [_RCUnityHelperShared() setAdGroup:convertCString(adGroup)];
}

void _RCSetAd(const char *ad) {
    [_RCUnityHelperShared() setAd:convertCString(ad)];
}

void _RCSetKeyword(const char *keyword) {
    [_RCUnityHelperShared() setKeyword:convertCString(keyword)];
}

void _RCSetCreative(const char *creative) {
    [_RCUnityHelperShared() setCreative:convertCString(creative)];
}

void _RCCollectDeviceIdentifiers() {
    [_RCUnityHelperShared() collectDeviceIdentifiers];
}

void _RCCanMakePayments(const char *featuresJSON) {
    NSError *error = nil;
    
    NSData *data = [convertCString(featuresJSON) dataUsingEncoding:NSUTF8StringEncoding];
    NSDictionary *canMakePaymentsRequest = [NSJSONSerialization JSONObjectWithData:data
                                                                           options:0
                                                                             error:&error];

    if (error) {
        NSLog(@"Error parsing features JSON: %s %@", featuresJSON, error.localizedDescription);
        return;
    }

    [_RCUnityHelperShared() canMakePaymentsWithFeatures:canMakePaymentsRequest[@"features"]];
}

