// Created by Antonio Pallares. Copyright (c) 2026 RevenueCat, Inc.

#import <UIKit/UIKit.h>


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

        UIView *host = window.rootViewController.view ?: window;

        overlayContainer = [[UIView alloc] initWithFrame:host.bounds];
        overlayContainer.backgroundColor = UIColor.clearColor;
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
