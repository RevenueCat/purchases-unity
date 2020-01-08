//
//  PurchasesUnityHelper.h
//  Purchases
//
//  Created by César de la Vega  on 1/8/20.
//  Copyright © 2020 RevenueCat. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "RCPurchases.h"


@interface RCUnityHelperDelegate : NSObject <RCPurchasesDelegate>
@property(nonatomic) NSDictionary *products;
@property(nonatomic) NSString *gameObject;

- (void)setupPurchases:(NSString *)apiKey appUserID:(NSString *)appUserID gameObject:(NSString *)gameObject observerMode:(BOOL)observerMode;

- (void)getProducts:(NSArray *)productIdentifiers
               type:(NSString *)type;

- (void)purchaseProduct:(NSString *)productIdentifier;

- (void)purchasePackage:(NSString *)packageIdentifier
                        offeringIdentifier:(NSString *)offeringIdentifier;

- (void)restoreTransactions;

- (void)addAttributionData:(NSString *)dataJSON network:(int)network networkUserId:(NSString * _Nullable)networkUserId;

- (void)createAlias:(NSString *)newAppUserID;

- (void)identify:(NSString *)appUserID;

- (void)reset;

- (void)setAllowSharingStoreAccount:(BOOL)allow;

- (void)getOfferings;

- (void)setDebugLogsEnabled:(BOOL)enabled;

- (void)getPurchaserInfo;

-  (void)setFinishTransactions:(BOOL)finishTransactions;

- (void)setAutomaticAppleSearchAdsAttributionCollection:(BOOL)enabled;

- (void)purchases:(RCPurchases *)purchases didReceiveUpdatedPurchaserInfo:(RCPurchaserInfo *)purchaserInfo;

- (char *)getAppUserID;

- (BOOL)isAnonymous;

- (void)checkTrialOrIntroductoryPriceEligibility:(NSArray *)productIdentifiers;

@end
