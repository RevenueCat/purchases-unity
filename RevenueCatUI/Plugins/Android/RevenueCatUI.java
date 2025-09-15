package com.revenuecat.unity;

public class RevenueCatUI {

    // Java-side callbacks registered from C#; no UnitySendMessage fallback.
    public interface RevenueCatUICallbacks {
        void onPaywallResult(String result);
        void onCustomerCenterResult(String result);
    }

    private static volatile RevenueCatUICallbacks callbacks;

    public static void registerCallbacks(RevenueCatUICallbacks cb) {
        callbacks = cb;
    }

    public static void unregisterCallbacks() {
        callbacks = null;
    }

    public static void presentPaywall(String offeringIdentifier) {
        sendPaywallResult("CANCELLED|Stub: no native UI");
    }

    public static void presentPaywallIfNeeded(String requiredEntitlementIdentifier, String offeringIdentifier) {
        sendPaywallResult("NOT_PRESENTED|Stub: no native UI");
    }

    public static void presentCustomerCenter() {
        sendCustomerCenterResult("DONE|Stub: no native UI");
    }

    public static boolean isSupported() {
        return true;
    }

    private static void sendPaywallResult(String result) {
        try {
            RevenueCatUICallbacks cb = callbacks;
            if (cb != null) cb.onPaywallResult(result);
        } catch (Throwable ignored) {}
    }

    private static void sendCustomerCenterResult(String result) {
        try {
            RevenueCatUICallbacks cb = callbacks;
            if (cb != null) cb.onCustomerCenterResult(result);
        } catch (Throwable ignored) {}
    }
}
