package com.revenuecat.purchasesunity.ui;

import android.app.Activity;
import android.util.Log;

public class RevenueCatUI {
    public interface PaywallCallbacks { void onPaywallResult(String result); }

    private static final String TAG = "RevenueCatUI";
    private static volatile PaywallCallbacks paywallCallbacks;

    public static void registerPaywallCallbacks(PaywallCallbacks cb) { paywallCallbacks = cb; }
    public static void unregisterPaywallCallbacks() { paywallCallbacks = null; }

    public static void presentPaywall(Activity activity, String offeringIdentifier, String presentedOfferingContextJson, boolean displayCloseButton) {
        PaywallTrampolineActivity.presentPaywall(activity, offeringIdentifier, presentedOfferingContextJson, displayCloseButton);
    }

    public static void presentPaywallIfNeeded(Activity activity, String requiredEntitlementIdentifier, String offeringIdentifier, String presentedOfferingContextJson, boolean displayCloseButton) {
        PaywallTrampolineActivity.presentPaywallIfNeeded(activity, requiredEntitlementIdentifier, offeringIdentifier, presentedOfferingContextJson, displayCloseButton);
    }

    public static void sendPaywallResult(String result) {
        try {
            PaywallCallbacks cb = paywallCallbacks;
            if (cb != null) {
                cb.onPaywallResult(result);
            } else {
                Log.w(TAG, "No callback registered to receive paywall result: " + result);
            }
        } catch (Throwable e) {
            Log.e(TAG, "Error sending paywall result: " + e.getMessage());
        }
    }
}