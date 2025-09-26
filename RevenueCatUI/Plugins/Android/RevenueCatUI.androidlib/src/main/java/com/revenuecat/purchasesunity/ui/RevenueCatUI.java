package com.revenuecat.purchasesunity.ui;

import static com.revenuecat.purchasesunity.ui.PaywallTrampolineActivity.EXTRA_GAME_OBJECT;
import static com.revenuecat.purchasesunity.ui.PaywallTrampolineActivity.EXTRA_METHOD;
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

    public static void presentPaywall(String gameObject, String offeringIdentifier, boolean displayCloseButton) {
        try {
            Activity activity = UnityBridge.currentActivityOrNull();
            if (activity == null) {
                Log.e(TAG, "currentActivity is null; cannot launch paywall");
                UnityBridge.sendMessage(gameObject, "OnPaywallResultFromActivity", "ERROR|NoActivity");
                return;
            }

            Intent intent = new Intent(activity, PaywallTrampolineActivity.class);
            intent.putExtra(EXTRA_GAME_OBJECT, gameObject);
            intent.putExtra(EXTRA_METHOD, "OnPaywallResultFromActivity");
            
            if (offeringIdentifier != null && !offeringIdentifier.isEmpty()) {
                intent.putExtra(EXTRA_OFFERING_ID, offeringIdentifier);
            }
            
            intent.putExtra(EXTRA_SHOULD_DISPLAY_DISMISS_BUTTON, displayCloseButton);

            // TODO: Remove debug log before shipping
            Log.d(TAG, "Launching PaywallTrampolineActivity for gameObject=" + gameObject +
                    ", offering=" + offeringIdentifier + ", displayCloseButton=" + displayCloseButton);
            activity.startActivity(intent);
        } catch (Throwable t) {
            Log.e(TAG, "Error launching PaywallTrampolineActivity", t);
            UnityBridge.sendMessage(gameObject, "OnPaywallResultFromActivity", "ERROR|" + t.getClass().getSimpleName());
        }
    }

    public static void presentPaywallIfNeeded(String gameObject, String requiredEntitlementIdentifier, String offeringIdentifier, boolean displayCloseButton) {
        // TODO: Remove debug log before shipping
        Log.d(TAG, "presentPaywallIfNeeded(go=" + gameObject + ", entitlement=" +
                requiredEntitlementIdentifier + ", offering=" + offeringIdentifier + ", displayCloseButton=" + displayCloseButton + ")");
        
        try {
            Activity activity = UnityBridge.currentActivityOrNull();
            if (activity == null) {
                Log.e(TAG, "currentActivity is null; cannot launch paywall");
                UnityBridge.sendMessage(gameObject, "OnPaywallResultFromActivity", "ERROR|NoActivity");
                return;
            }

            Intent intent = new Intent(activity, PaywallTrampolineActivity.class);
            intent.putExtra(EXTRA_GAME_OBJECT, gameObject);
            intent.putExtra(EXTRA_METHOD, "OnPaywallResultFromActivity");
            intent.putExtra(EXTRA_REQUIRED_ENTITLEMENT_IDENTIFIER, requiredEntitlementIdentifier);
            
            if (offeringIdentifier != null && !offeringIdentifier.isEmpty()) {
                intent.putExtra(EXTRA_OFFERING_ID, offeringIdentifier);
            }
            
            intent.putExtra(EXTRA_SHOULD_DISPLAY_DISMISS_BUTTON, displayCloseButton);

            // TODO: Remove debug log before shipping
            Log.d(TAG, "Launching PaywallTrampolineActivity for presentPaywallIfNeeded gameObject=" + gameObject +
                    ", entitlement=" + requiredEntitlementIdentifier + ", offering=" + offeringIdentifier + ", displayCloseButton=" + displayCloseButton);
            activity.startActivity(intent);
        } catch (Throwable t) {
            Log.e(TAG, "Error launching PaywallTrampolineActivity for presentPaywallIfNeeded", t);
            UnityBridge.sendMessage(gameObject, "OnPaywallResultFromActivity", "ERROR|" + t.getClass().getSimpleName());
        }
    }

    public static boolean isSupported() { return true; }

    private static void sendPaywallResult(String result) {
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