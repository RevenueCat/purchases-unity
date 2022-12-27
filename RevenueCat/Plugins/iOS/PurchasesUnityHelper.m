//
//  PurchasesUnityHelper.m
//
//  Created by RevenueCat, Inc. on 5/30/18.
//

#import <Foundation/Foundation.h>
#import <StoreKit/StoreKit.h>
#import <AdSupport/AdSupport.h>
@import PurchasesHybridCommon;
@import RevenueCat;

static NSString *const RECEIVE_PRODUCTS = @"_receiveProducts";
static NSString *const RECEIVE_CUSTOMER_INFO = @"_receiveCustomerInfo";
static NSString *const RESTORE_PURCHASES = @"_restorePurchases";
static NSString *const LOG_IN = @"_logIn";
static NSString *const LOG_OUT = @"_logOut";
static NSString *const MAKE_PURCHASE = @"_makePurchase";
static NSString *const GET_OFFERINGS = @"_getOfferings";
static NSString *const GET_CUSTOMER_INFO = @"_getCustomerInfo";
static NSString *const CHECK_ELIGIBILITY = @"_checkTrialOrIntroductoryPriceEligibility";
static NSString *const CAN_MAKE_PAYMENTS = @"_canMakePayments";
static NSString *const GET_PROMOTIONAL_OFFER = @"_getPromotionalOffer";

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
usesStoreKit2IfAvailable:(BOOL)usesStoreKit2IfAvailable
 userDefaultsSuiteName:(nullable NSString *)userDefaultsSuiteName
 dangerousSettingsJson:(NSString *)dangerousSettingsJson {
    self.products = nil;
    self.gameObject = nil;

    NSError *error = nil;
    NSDictionary *dangerousSettingsDict = [NSJSONSerialization JSONObjectWithData:[dangerousSettingsJson dataUsingEncoding:NSUTF8StringEncoding]
                                                                          options:0
                                                                            error:&error];

    RCDangerousSettings *dangerousSettings = nil;

    if (error) {
        NSLog(@"Error parsing dangerousSettings JSON: %@ %@", dangerousSettingsJson, error.localizedDescription);
    } else {
        BOOL autoSyncPurchases = dangerousSettingsDict[@"AutoSyncPurchases"];
        dangerousSettings = [[RCDangerousSettings alloc] initWithAutoSyncPurchases:autoSyncPurchases];
    }

    [RCPurchases configureWithAPIKey:apiKey
                           appUserID:appUserID
                        observerMode:observerMode
               userDefaultsSuiteName:userDefaultsSuiteName
                      platformFlavor:self.platformFlavor
               platformFlavorVersion:self.platformFlavorVersion
            usesStoreKit2IfAvailable:usesStoreKit2IfAvailable
                   dangerousSettings:dangerousSettings];

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

- (void)purchaseProduct:(NSString *)productIdentifier
signedDiscountTimestamp:(NSString *)signedDiscountTimestamp {
    [RCCommonFunctionality purchaseProduct:productIdentifier
                   signedDiscountTimestamp:signedDiscountTimestamp
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
     offeringIdentifier:(NSString *)offeringIdentifier
signedDiscountTimestamp:(NSString *)signedDiscountTimestamp {
    [RCCommonFunctionality purchasePackage:packageIdentifier
                                  offering:offeringIdentifier
                   signedDiscountTimestamp:signedDiscountTimestamp
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

- (void)restorePurchases {
    [RCCommonFunctionality restorePurchasesWithCompletionBlock:[self getCustomerInfoCompletionBlockFor:RESTORE_PURCHASES]];
}

- (void)syncPurchases {
    // on Android, syncPurchases doesn't have a completion block. So instead of
    // calling getCustomerInfoCompletionBlockFor:SYNC_PURCHASES, we just
    // print the response, to match Android behavior.
    [RCCommonFunctionality syncPurchasesWithCompletionBlock:^(NSDictionary *_Nullable responseDictionary, RCErrorContainer *_Nullable error) {
        NSLog(@"received syncPurchases response: \n customerInfo: %@ \n error:%@", responseDictionary, error);
    }];
}

- (void)logInWithAppUserID:(NSString *)appUserID {
    [RCCommonFunctionality logInWithAppUserID:appUserID completionBlock:[self getLogInCompletionBlockForMethod:LOG_IN]];
}

- (void)logOut {
    [RCCommonFunctionality logOutWithCompletionBlock:[self getCustomerInfoCompletionBlockFor:LOG_OUT]];
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

- (void)getCustomerInfo {
    [RCCommonFunctionality getCustomerInfoWithCompletionBlock:[self getCustomerInfoCompletionBlockFor:GET_CUSTOMER_INFO]];
}

-  (void)setFinishTransactions:(BOOL)finishTransactions {
    [RCCommonFunctionality setFinishTransactions:finishTransactions];
}

- (void)setAutomaticAppleSearchAdsAttributionCollection:(BOOL)enabled {
    [RCCommonFunctionality setAutomaticAppleSearchAdsAttributionCollection:enabled];
}

- (void)enableAdServicesAttributionTokenCollection {
    if (@available(iOS 14.3, macOS 11.1, macCatalyst 14.3, *)) {
        [RCCommonFunctionality enableAdServicesAttributionTokenCollection];
    } else {
        NSLog(@"[Purchases] Warning: tried to enable AdServices attribution token collection, but it's only available on iOS 14.3 or greater or macOS 11.1 or greater.");
    }
}

- (void)purchases:(RCPurchases *)purchases receivedUpdatedCustomerInfo:(RCCustomerInfo *)customerInfo {
    NSMutableDictionary *response = [NSMutableDictionary new];
    response[@"customerInfo"] = customerInfo.dictionary;
    [self sendJSONObject:response toMethod:RECEIVE_CUSTOMER_INFO];
}

- (char *)getAppUserID {
    return makeStringCopy([RCCommonFunctionality appUserID]);
}

- (BOOL)isAnonymous {
    return RCCommonFunctionality.isAnonymous;
}

- (void)checkTrialOrIntroductoryPriceEligibility:(NSArray *)productIdentifiers {
    [RCCommonFunctionality checkTrialOrIntroductoryPriceEligibility:productIdentifiers
                                                    completionBlock:^(NSDictionary<NSString *,NSDictionary *> * _Nonnull responseDictionary) {
        [self sendJSONObject:responseDictionary toMethod:CHECK_ELIGIBILITY];
    }];
}

- (void)invalidateCustomerInfoCache {
    [RCCommonFunctionality invalidateCustomerInfoCache];
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


- (void)promotionalOfferForProductIdentifier:(NSString *)productIdentifier
                                    discount:(NSString *)discountIdentifier {
    [RCCommonFunctionality promotionalOfferForProductIdentifier:productIdentifier
                                                      discount:discountIdentifier
                                               completionBlock:^(NSDictionary *_Nullable responseDictionary, RCErrorContainer *_Nullable error) {
        [self sendJSONObject:responseDictionary toMethod:GET_PROMOTIONAL_OFFER];
    }];
}

#pragma mark - Subscriber Attributes

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

- (void)setAirshipChannelID:(nullable NSString *)airshipChannelID {
    [RCCommonFunctionality setAirshipChannelID:airshipChannelID];
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

- (void (^)(NSDictionary *, RCErrorContainer *))getCustomerInfoCompletionBlockFor:(NSString *)method {
    return ^(NSDictionary *_Nullable responseDictionary, RCErrorContainer *_Nullable error) {
        NSMutableDictionary *response = [NSMutableDictionary new];

        if (error) {
            response[@"error"] = error.info;
        } else {
            response[@"customerInfo"] = responseDictionary;
        }
        [self sendJSONObject:response toMethod:method];
    };
}

- (void (^)(NSDictionary *, RCErrorContainer *))getLogInCompletionBlockForMethod:(NSString *)method {
    return ^(NSDictionary *_Nullable responseDictionary, RCErrorContainer *_Nullable error) {
        NSMutableDictionary *response = [NSMutableDictionary new];

        if (error) {
            response[@"error"] = error.info;
        } else {
            response[@"customerInfo"] = responseDictionary[@"customerInfo"];
            response[@"created"] = responseDictionary[@"created"];
        }
        [self sendJSONObject:response toMethod:method];
    };
}

- (NSString *)platformFlavor {
    return @"unity";
}

- (NSString *)platformFlavorVersion {
    return @"4.6.4";
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
                       const BOOL usesStoreKit2IfAvailable,
                       const char *userDefaultsSuiteName,
                       const char *dangerousSettingsJson) {
    [_RCUnityHelperShared() setupPurchases:convertCString(apiKey)
                                 appUserID:convertCString(appUserID)
                                gameObject:convertCString(gameObject)
                              observerMode:observerMode
                  usesStoreKit2IfAvailable:usesStoreKit2IfAvailable
                     userDefaultsSuiteName:convertCString(userDefaultsSuiteName)
                     dangerousSettingsJson:convertCString(dangerousSettingsJson)];
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

void _RCPurchaseProduct(const char *productIdentifier, const char *signedDiscountTimestamp) {
    [_RCUnityHelperShared() purchaseProduct:convertCString(productIdentifier)
                    signedDiscountTimestamp:convertCString(signedDiscountTimestamp)];
}

void _RCPurchasePackage(const char *packageIdentifier, const char *offeringIdentifier, const char *signedDiscountTimestamp) {
    [_RCUnityHelperShared() purchasePackage:convertCString(packageIdentifier)
                         offeringIdentifier:convertCString(offeringIdentifier)
                    signedDiscountTimestamp:convertCString(signedDiscountTimestamp)];
}

void _RCRestorePurchases() {
    [_RCUnityHelperShared() restorePurchases];
}

void _RCSyncPurchases() {
    [_RCUnityHelperShared() syncPurchases];
}

void _RCLogIn(const char *appUserID) {
    [_RCUnityHelperShared() logInWithAppUserID:convertCString(appUserID)];
}

void _RCLogOut() {
    [_RCUnityHelperShared() logOut];
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

void _RCGetCustomerInfo() {
    [_RCUnityHelperShared() getCustomerInfo];
}

char * _RCGetAppUserID() {
    return [_RCUnityHelperShared() getAppUserID];
}

void _RCSetAutomaticAppleSearchAdsAttributionCollection(const BOOL enabled) {
    [_RCUnityHelperShared() setAutomaticAppleSearchAdsAttributionCollection:enabled];
}

void _RCEnableAdServicesAttributionTokenCollection() {
    [_RCUnityHelperShared() enableAdServicesAttributionTokenCollection];
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

void _RCInvalidateCustomerInfoCache() {
    [_RCUnityHelperShared() invalidateCustomerInfoCache];
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

void _RCSetAirshipChannelID(const char *airshipChannelID) {
    [_RCUnityHelperShared() setAirshipChannelID:convertCString(airshipChannelID)];
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


void _RCGetPromotionalOffer(const char *productIdentifier, const char *discountIdentifier) {
    [_RCUnityHelperShared() promotionalOfferForProductIdentifier:convertCString(productIdentifier)
                                                        discount:convertCString(discountIdentifier)];
}

