//
//  Created by RevenueCat.
//  Copyright © 2019 RevenueCat. All rights reserved.
//

#import "RCPurchaserInfo+HybridAdditions.h"
#import "RCEntitlementInfos+HybridAdditions.h"
#import "NSDate+HybridAdditions.h"

@implementation RCPurchaserInfo (HybridAdditions)

- (NSDictionary *)dictionary
{
    NSArray *productIdentifiers = self.allPurchasedProductIdentifiers.allObjects;
    NSArray *sorted = [productIdentifiers sortedArrayUsingSelector:@selector(compare:)];

    NSMutableDictionary *allExpirations = [NSMutableDictionary new];
    NSMutableDictionary *allExpirationsMillis = [NSMutableDictionary new];
    for (NSString *productIdentifier in sorted) {
        NSDate *date = [self expirationDateForProductIdentifier:productIdentifier];
        allExpirations[productIdentifier] = date.formattedAsISO8601 ?: [NSNull null];
        allExpirationsMillis[productIdentifier] = date ? @(date.timeIntervalSince1970) : [NSNull null];
    }

    NSMutableDictionary *allPurchases = [NSMutableDictionary new];
    NSMutableDictionary *allPurchasesMillis = [NSMutableDictionary new];
    for (NSString *productIdentifier in sorted) {
        NSDate *date = [self purchaseDateForProductIdentifier:productIdentifier];
        allPurchases[productIdentifier] = date.formattedAsISO8601 ?: [NSNull null];
        allPurchasesMillis[productIdentifier] = date ? @(date.timeIntervalSince1970) : [NSNull null];
    }

    id latestExpiration = self.latestExpirationDate.formattedAsISO8601 ?: [NSNull null];

    return @{
            @"entitlements": self.entitlements.dictionary,
            @"activeSubscriptions": self.activeSubscriptions.allObjects,
            @"allPurchasedProductIdentifiers": self.allPurchasedProductIdentifiers.allObjects,
            @"latestExpirationDate": latestExpiration,
            @"latestExpirationDateMillis": self.latestExpirationDate ? @(self.latestExpirationDate.timeIntervalSince1970) : [NSNull null],
            @"firstSeen": self.firstSeen.formattedAsISO8601,
            @"firstSeenMillis": @(self.firstSeen.timeIntervalSince1970),
            @"originalAppUserId": self.originalAppUserId,
            @"requestDate": self.requestDate.formattedAsISO8601,
            @"requestDateMillis": @(self.requestDate.timeIntervalSince1970),
            @"allExpirationDates": allExpirations,
            @"allExpirationDatesMillis": allExpirationsMillis,
            @"allPurchaseDates": allPurchases,
            @"allPurchaseDatesMillis": allPurchasesMillis,
            @"originalApplicationVersion": self.originalApplicationVersion ?: [NSNull null],
    };
}

@end
