package com.revenuecat.unity.ui;

public class RevenueCatUI {
    
    private static final String PAYWALL_EVENT = "_paywallEvent";
    private static final String PAYWALL_RESULT = "_paywallResult";

    // Java-side callbacks registered from C#; no UnitySendMessage fallback.
    public interface PaywallCallbacks { void onPaywallResult(String result); }

    private static volatile PaywallCallbacks paywallCallbacks;

    public static void registerPaywallCallbacks(PaywallCallbacks cb) { paywallCallbacks = cb; }
    public static void unregisterPaywallCallbacks() { paywallCallbacks = null; }

    public static void presentPaywall(String offeringIdentifier) {
        try {
            android.app.Activity activity = UnityPlayer.currentActivity;
            Intent intent = new Intent(activity, com.revenuecat.purchasesunity.PaywallProxyActivity.class);
            intent.putExtra(com.revenuecat.purchasesunity.PaywallProxyActivity.EXTRA_GAME_OBJECT, gameObject);
            intent.putExtra(com.revenuecat.purchasesunity.PaywallProxyActivity.EXTRA_METHOD, PAYWALL_RESULT);
            if (offeringIdentifier != null) {
                intent.putExtra(com.revenuecat.purchasesunity.PaywallProxyActivity.EXTRA_OFFERING_ID, offeringIdentifier);
            }
            activity.startActivity(intent);
        } catch (Throwable t) {
            Log.e("Purchases", "Error launching PaywallProxyActivity", t);
        }
    }

    public static void presentPaywallIfNeeded(String requiredEntitlementIdentifier, String offeringIdentifier) {
        android.util.Log.d("RevenueCatUI", "presentPaywallIfNeeded(entitlement=" + requiredEntitlementIdentifier + ", offering=" + offeringIdentifier + ")");
        sendPaywallResult("NOT_PRESENTED|Stub: no native UI");
    }

    // No Customer Center in this stub

    public static boolean isSupported() {
        return true;
    }

    private static void sendPaywallResult(String result) {
        try {
            PaywallCallbacks cb = paywallCallbacks;
            if (cb != null) cb.onPaywallResult(result);
        } catch (Throwable ignored) {}
    }

    // No Customer Center in this stub
}
