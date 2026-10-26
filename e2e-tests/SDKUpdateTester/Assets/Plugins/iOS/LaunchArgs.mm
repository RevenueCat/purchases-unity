// Created by Antonio Pallares. Copyright (c) 2026 RevenueCat, Inc.

#import <Foundation/Foundation.h>

extern "C" const char *SDKUpdateLoginUserId(void) {
    NSArray<NSString *> *arguments = NSProcessInfo.processInfo.arguments;
    NSUInteger index = [arguments indexOfObject:@"-app_user_id_to_log_in"];
    if (index == NSNotFound || index + 1 >= arguments.count) return NULL;
    return strdup(arguments[index + 1].UTF8String);
}
