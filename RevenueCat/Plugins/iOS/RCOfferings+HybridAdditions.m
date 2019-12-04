//
//  Created by RevenueCat.
//  Copyright © 2019 RevenueCat. All rights reserved.
//

#import "RCOffering+HybridAdditions.h"
#import "RCOfferings+HybridAdditions.h"

@implementation RCOfferings (HybridAdditions)

- (NSDictionary *)dictionary
{
    NSMutableDictionary *jsonDict = [NSMutableDictionary new];

    NSMutableArray *allKeys = [NSMutableArray new];
    NSMutableArray *allValues = [NSMutableArray new];
    for (NSString *offeringId in self.all) {
        [allKeys addObject:offeringId];
        RCOffering *offering = self.all[offeringId];
        [allValues addObject:offering.dictionary];
    }

    jsonDict[@"allKeys"] = allKeys;
    jsonDict[@"allValues"] = allValues;
    jsonDict[@"current"] = self.current.dictionary;

    return [NSDictionary dictionaryWithDictionary:jsonDict];
}

@end
