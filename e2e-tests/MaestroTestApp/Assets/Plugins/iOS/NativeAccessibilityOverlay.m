//
//  NativeAccessibilityOverlay.m
//
//  Created by Antonio Pallares on 7/29/26.
//

#import <UIKit/UIKit.h>

// Unity renders all of uGUI into a single Metal view, so none of the in-game text
// reaches the iOS accessibility tree and UI automation has nothing to match against.
// This is the iOS counterpart of Plugins/Android/NativeAccessibilityOverlay.java:
// invisible UILabels are placed over Unity's view at the same screen coordinates,
// purely so XCUITest, and therefore Maestro, can find and tap the UI.

static UIView *overlayContainer = nil;
static NSMutableDictionary<NSString *, UILabel *> *overlayElements = nil;

static UIWindow *FindOverlayWindow(void)
{
    for (UIScene *scene in UIApplication.sharedApplication.connectedScenes) {
        if (![scene isKindOfClass:UIWindowScene.class]) {
            continue;
        }

        UIWindowScene *windowScene = (UIWindowScene *)scene;
        for (UIWindow *window in windowScene.windows) {
            if (window.isKeyWindow) {
                return window;
            }
        }

        if (windowScene.windows.count > 0) {
            return windowScene.windows.firstObject;
        }
    }

    return nil;
}

void NativeAccessibilityOverlayInit(void)
{
    dispatch_async(dispatch_get_main_queue(), ^{
        if (overlayContainer != nil) {
            return;
        }

        UIWindow *window = FindOverlayWindow();
        if (window == nil) {
            NSLog(@"NativeAccessibilityOverlay: found no window to attach to");
            return;
        }

        // Hosted in the root view controller's view rather than the window so that
        // anything presented on top of Unity, such as a paywall, stays above it.
        UIView *host = window.rootViewController.view ?: window;

        overlayContainer = [[UIView alloc] initWithFrame:host.bounds];
        overlayContainer.backgroundColor = UIColor.clearColor;
        // The overlay only exists to be read; taps have to reach Unity underneath.
        overlayContainer.userInteractionEnabled = NO;
        overlayContainer.autoresizingMask = UIViewAutoresizingFlexibleWidth | UIViewAutoresizingFlexibleHeight;
        overlayElements = [NSMutableDictionary dictionary];
        [host addSubview:overlayContainer];
    });
}

void NativeAccessibilityOverlaySetElement(const char *identifier, const char *text,
                                          int left, int top, int right, int bottom)
{
    NSString *elementId = identifier != NULL ? @(identifier) : @"";
    NSString *elementText = text != NULL ? @(text) : @"";

    dispatch_async(dispatch_get_main_queue(), ^{
        if (overlayContainer == nil) {
            return;
        }

        UILabel *label = overlayElements[elementId];
        if (label == nil) {
            label = [[UILabel alloc] initWithFrame:CGRectZero];
            label.textColor = UIColor.clearColor;
            label.backgroundColor = UIColor.clearColor;
            label.isAccessibilityElement = YES;
            [overlayContainer addSubview:label];
            overlayElements[elementId] = label;
        }

        label.text = elementText;
        label.accessibilityLabel = elementText;
        label.accessibilityIdentifier = elementId;

        // Unity reports these bounds in pixels, UIKit lays out in points.
        CGFloat scale = overlayContainer.window.screen.scale;
        if (scale <= 0) {
            scale = UIScreen.mainScreen.scale;
        }

        label.frame = CGRectMake(left / scale,
                                 top / scale,
                                 MAX((right - left) / scale, 1),
                                 MAX((bottom - top) / scale, 1));
    });
}

void NativeAccessibilityOverlayClear(void)
{
    dispatch_async(dispatch_get_main_queue(), ^{
        if (overlayContainer == nil) {
            return;
        }

        for (UILabel *label in overlayElements.allValues) {
            [label removeFromSuperview];
        }

        [overlayElements removeAllObjects];
    });
}
