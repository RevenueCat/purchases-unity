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
        // TODO: Remove debug log before shipping
        Log.d(TAG, "presentPaywall(offering=" + offeringIdentifier + ", displayCloseButton=" + displayCloseButton + ")");
        PaywallTrampolineActivity.presentPaywall(activity, offeringIdentifier, presentedOfferingContextJson, displayCloseButton);
    }

    public static void presentPaywallIfNeeded(Activity activity, String requiredEntitlementIdentifier, String offeringIdentifier, String presentedOfferingContextJson, boolean displayCloseButton) {
        // TODO: Remove debug log before shipping
        Log.d(TAG, "presentPaywallIfNeeded(entitlement=" + requiredEntitlementIdentifier + ", offering=" + offeringIdentifier + ", displayCloseButton=" + displayCloseButton + ")");
        PaywallTrampolineActivity.presentPaywallIfNeeded(activity, requiredEntitlementIdentifier, offeringIdentifier, presentedOfferingContextJson, displayCloseButton);
    }

    public static boolean isSupported() { return true; }

    public static void sendPaywallResult(String result) {
        try {
            PaywallCallbacks cb = paywallCallbacks;
            if (cb != null) {
                // TODO: Remove debug log before shipping
                Log.d(TAG, "Forwarding result to registered callback: " + result);
                cb.onPaywallResult(result);
            } else {
                // TODO: Review - keep this warning or remove before shipping?
                Log.w(TAG, "No callback registered to receive paywall result: " + result);
            }
        } catch (Throwable e) {
            Log.e(TAG, "Error sending paywall result: " + e.getMessage());
        }
    }
}