//
//  Bridge.cpp
//  Purchases
//
//  Created by César de la Vega  on 1/7/20.
//  Copyright © 2020 RevenueCat. All rights reserved.
//

#include "Bridge.h"
#import <Foundation/Foundation.h>
#import "PurchasesUnityHelper.h"

static UnityCommandCallback lastCallback = NULL;

void ConnectCallback(UnityCommandCallback callback)
{
    lastCallback = callback;
}

void CallMethod(const char *objectName, const char *commandName, const char *commandData)
{
    if (lastCallback != NULL) {
        lastCallback(objectName, commandName, commandData);
    }
}

NSString *convertCString(const char *string) {
    if (string)
        return [NSString stringWithUTF8String:string];
    else
        return nil;
}

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

void _RCPurchaseProduct(const char *productIdentifier) {
    [_RCUnityHelperShared() purchaseProduct:convertCString(productIdentifier)];
}

void _RCPurchasePackage(const char *packageIdentifier, const char *offeringIdentifier) {
    [_RCUnityHelperShared() purchasePackage:convertCString(packageIdentifier) offeringIdentifier:convertCString(offeringIdentifier)];
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

void _RCGetOfferings() {
    [_RCUnityHelperShared() getOfferings];
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
