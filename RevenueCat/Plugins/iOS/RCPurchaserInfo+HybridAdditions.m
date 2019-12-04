//
//  Created by RevenueCat.
//  Copyright © 2019 RevenueCat. All rights reserved.
//

#import "RCPurchaserInfo+HybridAdditions.h"
#import "RCEntitlementInfos+HybridAdditions.h"

static NSDateFormatter *formatter;
static dispatch_once_t onceToken;

@implementation RCPurchaserInfo (HybridAdditions)

- (NSDictionary *)dictionary
{
    NSArray *productIdentifiers = self.allPurchasedProductIdentifiers.allObjects;
    NSArray *sorted = [productIdentifiers sortedArrayUsingSelector:@selector(compare:)];

    NSMutableArray *expirationDateKeys = [NSMutableArray new];
    NSMutableArray *expirationDateValues = [NSMutableArray new];
    for (NSString *productIdentifier in sorted) {
        [expirationDateKeys addObject:productIdentifier];
        NSDate *date = [self expirationDateForProductIdentifier:productIdentifier];
        NSDate *value = date ? @(date.timeIntervalSince1970) : [NSNull null];
        [expirationDateValues addObject:value];
    }

    NSMutableArray *purchaseDateKeys = [NSMutableArray new];
    NSMutableArray *purchaseDateValues = [NSMutableArray new];
    for (NSString *productIdentifier in self.allPurchasedProductIdentifiers) {
        [purchaseDateKeys addObject:productIdentifier];
        NSDate *date = [self purchaseDateForProductIdentifier:productIdentifier];
        NSDate *value = date ? @(date.timeIntervalSince1970) : [NSNull null];
        [purchaseDateValues addObject:value];
    }

    return @{
            @"entitlements": self.entitlements.dictionary,
            @"activeSubscriptions": self.activeSubscriptions.allObjects,
            @"allPurchasedProductIdentifiers": self.allPurchasedProductIdentifiers.allObjects,
            @"latestExpirationDate": self.latestExpirationDate ? @(self.latestExpirationDate.timeIntervalSince1970) : [NSNull null],
            @"firstSeen": @(self.firstSeen.timeIntervalSince1970),
            @"originalAppUserId": self.originalAppUserId,
            @"requestDate": @(self.requestDate.timeIntervalSince1970),
            @"allExpirationDateKeys": expirationDateKeys,
            @"allExpirationDateValues": expirationDateValues,
            @"allPurchaseDateKeys": purchaseDateKeys,
            @"allPurchaseDateValues": purchaseDateValues,
            @"originalApplicationVersion": self.originalApplicationVersion ?: [NSNull null],
    };
}

@end
