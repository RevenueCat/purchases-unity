package com.revenuecat.purchasesunity.ui;

import static com.revenuecat.purchasesunity.ui.PaywallTrampolineActivity.EXTRA_OFFERING_ID;
import static com.revenuecat.purchasesunity.ui.PaywallTrampolineActivity.EXTRA_SHOULD_DISPLAY_DISMISS_BUTTON;
import static com.revenuecat.purchasesunity.ui.PaywallTrampolineActivity.EXTRA_REQUIRED_ENTITLEMENT_IDENTIFIER;

import android.app.Activity;
import android.content.Intent;
import android.util.Log;

public class RevenueCatUI {
    public interface PaywallCallbacks { void onPaywallResult(String result); }

    private static final String TAG = "RevenueCatUI";
    private static volatile PaywallCallbacks paywallCallbacks;

    public static void registerPaywallCallbacks(PaywallCallbacks cb) { paywallCallbacks = cb; }
    public static void unregisterPaywallCallbacks() { paywallCallbacks = null; }

    public static void presentPaywall(Activity activity, String offeringIdentifier, boolean displayCloseButton) {
        try {
            if (activity == null) {
                Log.e(TAG, "Activity is null; cannot launch paywall");
                return;
            }

            Intent intent = new Intent(activity, PaywallTrampolineActivity.class);
            
            if (offeringIdentifier != null && !offeringIdentifier.isEmpty()) {
                intent.putExtra(EXTRA_OFFERING_ID, offeringIdentifier);
            }
            
            intent.putExtra(EXTRA_SHOULD_DISPLAY_DISMISS_BUTTON, displayCloseButton);

            // TODO: Remove debug log before shipping
            Log.d(TAG, "Launching PaywallTrampolineActivity offering=" + offeringIdentifier + ", displayCloseButton=" + displayCloseButton);
            activity.startActivity(intent);
        } catch (Throwable t) {
            Log.e(TAG, "Error launching PaywallTrampolineActivity", t);
            sendPaywallResult("ERROR");
        }
    }

    public static void presentPaywallIfNeeded(Activity activity, String requiredEntitlementIdentifier, String offeringIdentifier, boolean displayCloseButton) {
        // TODO: Remove debug log before shipping
        Log.d(TAG, "presentPaywallIfNeeded(entitlement=" +
                requiredEntitlementIdentifier + ", offering=" + offeringIdentifier + ", displayCloseButton=" + displayCloseButton + ")");
        
        try {
            if (activity == null) {
                Log.e(TAG, "Activity is null; cannot launch paywall");
                return;
            }

            Intent intent = new Intent(activity, PaywallTrampolineActivity.class);
            intent.putExtra(EXTRA_REQUIRED_ENTITLEMENT_IDENTIFIER, requiredEntitlementIdentifier);
            
            if (offeringIdentifier != null && !offeringIdentifier.isEmpty()) {
                intent.putExtra(EXTRA_OFFERING_ID, offeringIdentifier);
            }
            
            intent.putExtra(EXTRA_SHOULD_DISPLAY_DISMISS_BUTTON, displayCloseButton);

            // TODO: Remove debug log before shipping
            Log.d(TAG, "Launching PaywallTrampolineActivity for presentPaywallIfNeeded entitlement=" + requiredEntitlementIdentifier + ", offering=" + offeringIdentifier + ", displayCloseButton=" + displayCloseButton);
            activity.startActivity(intent);
        } catch (Throwable t) {
            Log.e(TAG, "Error launching PaywallTrampolineActivity for presentPaywallIfNeeded", t);
            sendPaywallResult("ERROR");
        }
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