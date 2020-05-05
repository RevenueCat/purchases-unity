//
//  Created by RevenueCat.
//  Copyright © 2019 RevenueCat. All rights reserved.
//

#import "RCEntitlementInfo+HybridAdditions.h"
#import "SKProduct+HybridAdditions.h"
#import "NSDate+HybridAdditions.h"

@implementation RCEntitlementInfo (HybridAdditions)

- (NSDictionary *)dictionary
{
    NSMutableDictionary *jsonDict = [NSMutableDictionary new];
    jsonDict[@"identifier"] = self.identifier;
    jsonDict[@"isActive"] = @(self.isActive);
    jsonDict[@"willRenew"] = @(self.willRenew);

    switch (self.periodType) {
        case RCIntro:
            jsonDict[@"periodType"] = @"INTRO";
            break;
        case RCNormal:
            jsonDict[@"periodType"] = @"NORMAL";
            break;
        case RCTrial:
            jsonDict[@"periodType"] = @"TRIAL";
            break;
    }

    jsonDict[@"latestPurchaseDate"] = self.latestPurchaseDate.formattedAsISO8601;
    jsonDict[@"latestPurchaseDateMillis"] = @(self.latestPurchaseDate.timeIntervalSince1970);
    jsonDict[@"originalPurchaseDate"] = self.originalPurchaseDate.formattedAsISO8601;
    jsonDict[@"originalPurchaseDateMillis"] = @(self.originalPurchaseDate.timeIntervalSince1970);
    jsonDict[@"expirationDate"] = self.expirationDate.formattedAsISO8601 ?: [NSNull null];
    jsonDict[@"expirationDateMillis"] = self.expirationDate ? @(self.expirationDate.timeIntervalSince1970) : [NSNull null];

    switch (self.store) {
        case RCAppStore:
            jsonDict[@"store"] = @"APP_STORE";
            break;
        case RCMacAppStore:
            jsonDict[@"store"] = @"MAC_APP_STORE";
            break;
        case RCPlayStore:
            jsonDict[@"store"] = @"PLAY_STORE";
            break;
        case RCStripe:
            jsonDict[@"store"] = @"STRIPE";
            break;
        case RCPromotional:
            jsonDict[@"store"] = @"PROMOTIONAL";
            break;
        case RCUnknownStore:
            jsonDict[@"store"] = @"UNKNOWN_STORE";
            break;
    }
    
    jsonDict[@"productIdentifier"] = self.productIdentifier;
    jsonDict[@"isSandbox"] = @(self.isSandbox);
    jsonDict[@"unsubscribeDetectedAt"] = self.unsubscribeDetectedAt.formattedAsISO8601 ?: [NSNull null];
    jsonDict[@"unsubscribeDetectedAtMillis"] = self.unsubscribeDetectedAt ? @(self.unsubscribeDetectedAt.timeIntervalSince1970) : [NSNull null];
    jsonDict[@"billingIssueDetectedAt"] = self.billingIssueDetectedAt.formattedAsISO8601 ?: [NSNull null];
    jsonDict[@"billingIssueDetectedAtMillis"] = self.billingIssueDetectedAt ? @(self.billingIssueDetectedAt.timeIntervalSince1970) : [NSNull null];
    
    return [NSDictionary dictionaryWithDictionary:jsonDict];
}

@end
