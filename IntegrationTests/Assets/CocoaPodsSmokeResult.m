#import <UIKit/UIKit.h>

// A native label lets Maestro observe the C# -> native Purchases assertion.
void CocoaPodsSmokeShowResult(const char *result) {
    NSString *text = [NSString stringWithUTF8String:result];
    dispatch_async(dispatch_get_main_queue(), ^{
        for (UIScene *scene in UIApplication.sharedApplication.connectedScenes) {
            if (![scene isKindOfClass:UIWindowScene.class]) continue;
            for (UIWindow *window in ((UIWindowScene *)scene).windows) {
                if (!window.isKeyWindow) continue;
                UILabel *label = [[UILabel alloc] initWithFrame:CGRectMake(20, 100, 350, 100)];
                label.text = text;
                label.numberOfLines = 0;
                label.backgroundColor = UIColor.whiteColor;
                label.textColor = UIColor.blackColor;
                label.accessibilityIdentifier = @"cocoapods-smoke-result";
                [window.rootViewController.view addSubview:label];
                return;
            }
        }
    });
}
