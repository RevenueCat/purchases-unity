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

    NSMutableArray *allExpirationsMillisKeys = [NSMutableArray new];
    NSMutableArray *allExpirationsMillisValues = [NSMutableArray new];
    for (NSString *productIdentifier in sorted) {
        [allExpirationsMillisKeys addObject:productIdentifier];
        NSDate *date = [self expirationDateForProductIdentifier:productIdentifier];
        NSDate *value = date ? @(date.timeIntervalSince1970) : [NSNull null];
        [allExpirationsMillisValues addObject:value];
    }

    NSMutableArray *allPurchaseDatesMillisKeys = [NSMutableArray new];
    NSMutableArray *allPurchaseDatesMillisValues = [NSMutableArray new];
    for (NSString *productIdentifier in sorted) {
        [allPurchaseDatesMillisKeys addObject:productIdentifier];
        NSDate *date = [self purchaseDateForProductIdentifier:productIdentifier];
        NSDate *value = date ? @(date.timeIntervalSince1970) : [NSNull null];
        [allPurchaseDatesMillisValues addObject:value];
    }

    id latestExpiration = self.latestExpirationDate.formattedAsISO8601 ?: [NSNull null];

    return @{
            @"entitlements": self.entitlements.dictionary,
            @"activeSubscriptions": self.activeSubscriptions.allObjects,
            @"allPurchasedProductIdentifiers": self.allPurchasedProductIdentifiers.allObjects,
            @"latestExpirationDateMillis": self.latestExpirationDate ? @(self.latestExpirationDate.timeIntervalSince1970) : [NSNull null],
            @"firstSeenMillis": @(self.firstSeen.timeIntervalSince1970),
            @"originalAppUserId": self.originalAppUserId,
            @"requestDateMillis": @(self.requestDate.timeIntervalSince1970),
            @"allExpirationDatesMillisKeys": allExpirationsMillisKeys,
            @"allExpirationDatesMillisValues": allExpirationsMillisValues,
            @"allPurchaseDatesMillisKeys": allPurchaseDatesMillisKeys,
            @"allPurchaseDatesMillisValues": allPurchaseDatesMillisValues,
            @"originalApplicationVersion": self.originalApplicationVersion ?: [NSNull null],
    };
}

@end
