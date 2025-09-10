package com.revenuecat.unity;

import com.unity3d.player.UnityPlayer;

public class RevenueCatUI {

    private static final String UNITY_CALLBACK_OBJECT = "RevenueCatUI";

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
            UnityPlayer.UnitySendMessage(UNITY_CALLBACK_OBJECT, "OnPaywallResult", result);
        } catch (Throwable ignored) {}
    }

    private static void sendCustomerCenterResult(String result) {
        try {
            UnityPlayer.UnitySendMessage(UNITY_CALLBACK_OBJECT, "OnCustomerCenterResult", result);
        } catch (Throwable ignored) {}
    }
}

