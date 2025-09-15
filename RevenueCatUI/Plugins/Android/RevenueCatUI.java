package com.revenuecat.unity;

public class RevenueCatUI {

    // Java-side callbacks registered from C#; no UnitySendMessage fallback.
    public interface PaywallCallbacks { void onPaywallResult(String result); }
    public interface CustomerCenterCallbacks { void onCustomerCenterResult(String result); }

    private static volatile PaywallCallbacks paywallCallbacks;
    private static volatile CustomerCenterCallbacks customerCenterCallbacks;

    public static void registerPaywallCallbacks(PaywallCallbacks cb) { paywallCallbacks = cb; }
    public static void unregisterPaywallCallbacks() { paywallCallbacks = null; }

    public static void registerCustomerCenterCallbacks(CustomerCenterCallbacks cb) { customerCenterCallbacks = cb; }
    public static void unregisterCustomerCenterCallbacks() { customerCenterCallbacks = null; }

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
            PaywallCallbacks cb = paywallCallbacks;
            if (cb != null) cb.onPaywallResult(result);
        } catch (Throwable ignored) {}
    }

    private static void sendCustomerCenterResult(String result) {
        try {
            CustomerCenterCallbacks cb = customerCenterCallbacks;
            if (cb != null) cb.onCustomerCenterResult(result);
        } catch (Throwable ignored) {}
    }
}
