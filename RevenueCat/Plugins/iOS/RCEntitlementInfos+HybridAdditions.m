//
//  Created by RevenueCat.
//  Copyright © 2019 RevenueCat. All rights reserved.
//

#import "RCEntitlementInfo+HybridAdditions.h"
#import "RCEntitlementInfos+HybridAdditions.h"

@implementation RCEntitlementInfos (HybridAdditions)

- (NSDictionary *)dictionary
{
    NSMutableDictionary *jsonDict = [NSMutableDictionary new];

    NSMutableArray *allKeys = [NSMutableArray new];
    NSMutableArray *allValues = [NSMutableArray new];
    for (NSString *entId in self.all) {
        [allKeys addObject:entId];
        [allValues addObject:self.all[entId].dictionary];
    }
    jsonDict[@"allKeys"] = allKeys;
    jsonDict[@"allValues"] = allValues;

    NSMutableArray *activeKeys = [NSMutableArray new];
    NSMutableArray *activeValues = [NSMutableArray new];
    for (NSString *entId in self.active) {
        [activeKeys addObject:entId];
        [activeValues addObject:self.active[entId].dictionary];
    }
    jsonDict[@"activeKeys"] = activeKeys;
    jsonDict[@"activeValues"] = activeValues;

    return [NSDictionary dictionaryWithDictionary:jsonDict];
}

@end
