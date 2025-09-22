package com.revenuecat.purchasesunity.ui;

import static com.revenuecat.purchasesunity.ui.PaywallProxyActivity.EXTRA_GAME_OBJECT;
import static com.revenuecat.purchasesunity.ui.PaywallProxyActivity.EXTRA_METHOD;
import static com.revenuecat.purchasesunity.ui.PaywallProxyActivity.EXTRA_OFFERING_ID;
import static com.revenuecat.purchasesunity.ui.PaywallProxyActivity.EXTRA_SHOULD_DISPLAY_DISMISS_BUTTON;
import static com.revenuecat.purchasesunity.ui.PaywallProxyActivity.EXTRA_REQUIRED_ENTITLEMENT_IDENTIFIER;

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

            Intent intent = new Intent(activity, PaywallProxyActivity.class);
            intent.putExtra(EXTRA_GAME_OBJECT, gameObject);
            intent.putExtra(EXTRA_METHOD, "OnPaywallResultFromActivity");
            
            if (offeringIdentifier != null && !offeringIdentifier.isEmpty()) {
                intent.putExtra(EXTRA_OFFERING_ID, offeringIdentifier);
            }
            
            intent.putExtra(EXTRA_SHOULD_DISPLAY_DISMISS_BUTTON, displayCloseButton);

            Log.d(TAG, "Launching PaywallProxyActivity for gameObject=" + gameObject +
                    ", offering=" + offeringIdentifier + ", displayCloseButton=" + displayCloseButton);
            activity.startActivity(intent);
        } catch (Throwable t) {
            Log.e(TAG, "Error launching PaywallProxyActivity", t);
            UnityBridge.sendMessage(gameObject, "OnPaywallResultFromActivity", "ERROR|" + t.getClass().getSimpleName());
        }
    }

    public static void presentPaywallIfNeeded(String gameObject, String requiredEntitlementIdentifier, String offeringIdentifier, boolean displayCloseButton) {
        Log.d(TAG, "presentPaywallIfNeeded(go=" + gameObject + ", entitlement=" +
                requiredEntitlementIdentifier + ", offering=" + offeringIdentifier + ", displayCloseButton=" + displayCloseButton + ")");
        
        try {
            Activity activity = UnityBridge.currentActivityOrNull();
            if (activity == null) {
                Log.e(TAG, "currentActivity is null; cannot launch paywall");
                UnityBridge.sendMessage(gameObject, "OnPaywallResultFromActivity", "ERROR|NoActivity");
                return;
            }

            Intent intent = new Intent(activity, PaywallProxyActivity.class);
            intent.putExtra(EXTRA_GAME_OBJECT, gameObject);
            intent.putExtra(EXTRA_METHOD, "OnPaywallResultFromActivity");
            intent.putExtra(EXTRA_REQUIRED_ENTITLEMENT_IDENTIFIER, requiredEntitlementIdentifier);
            
            if (offeringIdentifier != null && !offeringIdentifier.isEmpty()) {
                intent.putExtra(EXTRA_OFFERING_ID, offeringIdentifier);
            }
            
            intent.putExtra(EXTRA_SHOULD_DISPLAY_DISMISS_BUTTON, displayCloseButton);

            Log.d(TAG, "Launching PaywallProxyActivity for presentPaywallIfNeeded gameObject=" + gameObject +
                    ", entitlement=" + requiredEntitlementIdentifier + ", offering=" + offeringIdentifier + ", displayCloseButton=" + displayCloseButton);
            activity.startActivity(intent);
        } catch (Throwable t) {
            Log.e(TAG, "Error launching PaywallProxyActivity for presentPaywallIfNeeded", t);
            UnityBridge.sendMessage(gameObject, "OnPaywallResultFromActivity", "ERROR|" + t.getClass().getSimpleName());
        }
    }

    public static boolean isSupported() { return true; }

    // Keeps your callback path intact if you use it internally (not used by SendMessage flow)
    private static void sendPaywallResult(String result) {
        try {
            PaywallCallbacks cb = paywallCallbacks;
            if (cb != null) {
                Log.d(TAG, "Forwarding result to registered callback: " + result);
                cb.onPaywallResult(result);
            } else {
                Log.w(TAG, "No callback registered to receive paywall result: " + result);
            }
        } catch (Throwable e) {
            Log.e(TAG, "Error sending paywall result: " + e.getMessage());
        }
    }
}