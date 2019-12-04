//
//  Created by RevenueCat.
//  Copyright © 2019 RevenueCat. All rights reserved.
//

#import "SKProduct+HybridAdditions.h"

@implementation SKProduct (RCPurchases)

- (nullable NSString *)rc_currencyCode {
    if(@available(iOS 10.0, *)) {
        return self.priceLocale.currencyCode;
    } else {
        return [self.priceLocale objectForKey:NSLocaleCurrencyCode];
    }
}

- (NSDictionary *)dictionary
{
    NSNumberFormatter *formatter = [[NSNumberFormatter alloc] init];
    formatter.numberStyle = NSNumberFormatterCurrencyStyle;
    formatter.locale = self.priceLocale;
    NSMutableDictionary *d = [NSMutableDictionary dictionaryWithDictionary:@{
                        @"identifier": self.productIdentifier ?: @"",
                        @"description": self.localizedDescription ?: @"",
                        @"title": self.localizedTitle ?: @"",
                        @"price": @(self.price.floatValue),
                        @"priceString": [formatter stringFromNumber:self.price],
                        @"currencyCode": (self.rc_currencyCode) ? self.rc_currencyCode : [NSNull null]
                        }];
    
    if (@available(iOS 11.2, *)) {
        if (self.introductoryPrice) {
            d[@"introPrice"] = @(self.introductoryPrice.price.floatValue);
            d[@"introPriceString"] = [formatter stringFromNumber:self.introductoryPrice.price];
            d[@"introPricePeriod"] = [self normalizeSubscriptionPeriod:self.introductoryPrice.subscriptionPeriod];
            d[@"introPricePeriodUnit"] = [self normalizeSubscriptionPeriodUnit:self.introductoryPrice.subscriptionPeriod.unit];
            d[@"introPricePeriodNumberOfUnits"] = @(self.introductoryPrice.subscriptionPeriod.numberOfUnits);
            d[@"introPriceCycles"] = @(self.introductoryPrice.numberOfPeriods);
            return d;
        }
    }

    d[@"introPrice"] = [NSNull null];
    d[@"introPriceString"] = [NSNull null];
    d[@"introPricePeriod"] = [NSNull null];
    d[@"introPricePeriodUnit"] = [NSNull null];
    d[@"introPricePeriodNumberOfUnits"] = [NSNull null];
    d[@"introPriceCycles"] = [NSNull null];
    
    return d;
}

- (NSString *)normalizeSubscriptionPeriod:(SKProductSubscriptionPeriod *)subscriptionPeriod API_AVAILABLE(ios(11.2)){
    NSString *unit;
    switch (subscriptionPeriod.unit) {
        case SKProductPeriodUnitDay:
            unit = @"D";
            break;
        case SKProductPeriodUnitWeek:
            unit = @"W";
            break;
        case SKProductPeriodUnitMonth:
            unit = @"M";
            break;
        case SKProductPeriodUnitYear:
            unit = @"Y";
            break;
    }
    return [NSString stringWithFormat:@"%@%@%@", @"P", @(subscriptionPeriod.numberOfUnits), unit];
}

- (NSString *)normalizeSubscriptionPeriodUnit:(SKProductPeriodUnit)subscriptionPeriodUnit API_AVAILABLE(ios(11.2)){
    switch (subscriptionPeriodUnit) {
        case SKProductPeriodUnitDay:
            return @"DAY";
        case SKProductPeriodUnitWeek:
            return @"WEEK";
        case SKProductPeriodUnitMonth:
            return @"MONTH";
        case SKProductPeriodUnitYear:
            return @"YEAR";
    }
}

@end
