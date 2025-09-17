package com.revenuecat.unity.ui;

public class RevenueCatUI {

    // Java-side callbacks registered from C#; no UnitySendMessage fallback.
    public interface PaywallCallbacks { void onPaywallResult(String result); }

    private static volatile PaywallCallbacks paywallCallbacks;

    public static void registerPaywallCallbacks(PaywallCallbacks cb) { paywallCallbacks = cb; }
    public static void unregisterPaywallCallbacks() { paywallCallbacks = null; }


    public static void presentPaywall(String offeringIdentifier) {
        android.util.Log.d("RevenueCatUI", "presentPaywall(offering=" + offeringIdentifier + ")");
        sendPaywallResult("CANCELLED|Stub: no native UI");
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
