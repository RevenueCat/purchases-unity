#import <Foundation/Foundation.h>

extern "C" {
    const char* GetLaunchTestFlow() {
        NSString* testFlow = [[NSUserDefaults standardUserDefaults] stringForKey:@"e2e_test_flow"];
        if (testFlow == nil) return NULL;
        const char* utf8 = [testFlow UTF8String];
        char* result = (char*)malloc(strlen(utf8) + 1);
        strcpy(result, utf8);
        return result;
    }
}
