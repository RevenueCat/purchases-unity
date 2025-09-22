package com.revenuecat.purchasesunity.ui;

import android.app.Activity;
import android.util.Log;

final class UnityBridge {
    private static final String TAG = "UnityBridge";
    private UnityBridge() {}

    static Activity currentActivityOrNull() {
        try {
            Class<?> up = Class.forName("com.unity3d.player.UnityPlayer");
            return (Activity) up.getField("currentActivity").get(null);
        } catch (Throwable t) {
            return null;
        }
    }

    static void sendMessage(String gameObject, String method, String message) {
        try {
            Class<?> up = Class.forName("com.unity3d.player.UnityPlayer");
            up.getMethod("UnitySendMessage", String.class, String.class, String.class)
              .invoke(null, gameObject, method, message);
        } catch (Throwable t) {
            Log.w(TAG, "UnitySendMessage failed (" + gameObject + "." + method + "): " + message, t);
        }
    }
}