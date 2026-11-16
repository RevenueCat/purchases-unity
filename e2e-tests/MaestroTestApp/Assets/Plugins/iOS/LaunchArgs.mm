#import <Foundation/Foundation.h>

static const char* CopyLaunchArgument(NSString* key) {
    NSString* value = [[NSUserDefaults standardUserDefaults] stringForKey:key];
    if (value == nil) return NULL;
    const char* utf8 = [value UTF8String];
    char* result = (char*)malloc(strlen(utf8) + 1);
    strcpy(result, utf8);
    return result;
}

extern "C" {
    const char* GetLaunchTestFlow() {
        return CopyLaunchArgument(@"e2e_test_flow");
    }

    const char* GetLaunchTestApiKey() {
        return CopyLaunchArgument(@"rc_test_api_key");
    }
}
