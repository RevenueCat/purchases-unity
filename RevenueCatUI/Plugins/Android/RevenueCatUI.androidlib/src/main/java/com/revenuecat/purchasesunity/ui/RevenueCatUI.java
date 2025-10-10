package com.revenuecat.purchasesunity.ui;

import android.app.Activity;
import android.util.Log;

public class RevenueCatUI {
    public interface PaywallCallbacks { void onPaywallResult(String result); }
    public interface CustomerCenterCallbacks { void onCustomerCenterResult(String result); }

    private static final String TAG = "RevenueCatUI";
    private static volatile PaywallCallbacks paywallCallbacks;
    private static volatile CustomerCenterCallbacks customerCenterCallbacks;

    public static void registerPaywallCallbacks(PaywallCallbacks cb) { paywallCallbacks = cb; }
    public static void unregisterPaywallCallbacks() { paywallCallbacks = null; }

    public static void registerCustomerCenterCallbacks(CustomerCenterCallbacks cb) { customerCenterCallbacks = cb; }
    public static void unregisterCustomerCenterCallbacks() { customerCenterCallbacks = null; }

    public static void presentPaywall(Activity activity, String offeringIdentifier, String presentedOfferingContextJson, boolean displayCloseButton) {
        PaywallTrampolineActivity.presentPaywall(activity, offeringIdentifier, presentedOfferingContextJson, displayCloseButton);
    }

    public static void presentPaywallIfNeeded(Activity activity, String requiredEntitlementIdentifier, String offeringIdentifier, String presentedOfferingContextJson, boolean displayCloseButton) {
        PaywallTrampolineActivity.presentPaywallIfNeeded(activity, requiredEntitlementIdentifier, offeringIdentifier, presentedOfferingContextJson, displayCloseButton);
    }

    public static void presentCustomerCenter(Activity activity) {
        CustomerCenterTrampolineActivity.presentCustomerCenter(activity);
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

    public static void sendCustomerCenterResult(String result) {
        try {
            CustomerCenterCallbacks cb = customerCenterCallbacks;
            if (cb != null) {
                cb.onCustomerCenterResult(result);
            } else {
                Log.w(TAG, "No callback registered to receive customer center result: " + result);
            }
        } catch (Throwable e) {
            Log.e(TAG, "Error sending customer center result: " + e.getMessage());
        }
    }
}
