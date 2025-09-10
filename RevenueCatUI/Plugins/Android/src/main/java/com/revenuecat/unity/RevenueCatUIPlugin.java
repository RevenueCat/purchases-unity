package com.revenuecat.unity;

import android.app.Activity;
import android.content.Intent;
import com.unity3d.player.UnityPlayer;
import com.revenuecat.purchases.ui.revenuecatui.activity.PaywallResult;

public class RevenueCatUIPlugin {
    
    private static final String UNITY_CALLBACK_OBJECT = "RevenueCatUI";
    
    /**
     * Shows RevenueCat paywall using the new bridge pattern
     * Accepts Activity (Unity's UnityPlayerActivity) and casts to FragmentActivity when possible
     * @param requiredEntitlementIdentifier The required entitlement identifier (optional, can be empty)
     * @param offeringIdentifier The offering identifier to display (optional, can be empty)
     */
    public static void presentPaywall(String requiredEntitlementIdentifier, String offeringIdentifier) {
        Activity activity = UnityPlayer.currentActivity;
        if (activity == null) {
            sendResultToUnity("ERROR|Unity activity is null");
            return;
        }

        // Always use bridge activity to guarantee Activity Result registration occurs before STARTED
        startBridgeActivity(activity, requiredEntitlementIdentifier, offeringIdentifier, false, false);
    }
    
    /**
     * Shows RevenueCat paywall if needed using the new bridge pattern
     * @param requiredEntitlementIdentifier The required entitlement identifier
     * @param offeringIdentifier The offering identifier to display (optional, can be empty)
     */
    public static void presentPaywallIfNeeded(String requiredEntitlementIdentifier, String offeringIdentifier) {
        Activity activity = UnityPlayer.currentActivity;
        if (activity == null) {
            sendResultToUnity("ERROR|Unity activity is null");
            return;
        }

        // Always use bridge activity
        startBridgeActivity(activity, requiredEntitlementIdentifier, offeringIdentifier, true, false);
    }
    
    /**
     * Shows customer center using the new bridge pattern
     */
    public static void presentCustomerCenter() {
        Activity activity = UnityPlayer.currentActivity;
        if (activity == null) {
            sendResultToUnity("ERROR|Unity activity is null");
            return;
        }

        startBridgeActivity(activity, null, null, false, true);
    }
    
    /**
     * Check if RevenueCat UI is supported
     * @return true if supported
     */
    public static boolean isSupported() {
        return true; // RevenueCat UI is available with hybrid-common-ui
    }
    
    /**
     * Internal method that implements the bridge pattern
     * At compile-time we treat the parameter as Activity, but at run-time the object
     * Unity passes in is still a UnityPlayerActivity instance.
     */
    private static void startBridgeActivity(Activity activity, String requiredEntitlementIdentifier, String offeringIdentifier, boolean onlyIfNeeded, boolean isCustomerCenter) {
        try {
            Intent intent = new Intent(activity, PaywallBridgeActivity.class);
            if (offeringIdentifier != null) intent.putExtra(PaywallBridgeActivity.EXTRA_OFFERING_ID, offeringIdentifier);
            if (requiredEntitlementIdentifier != null) intent.putExtra(PaywallBridgeActivity.EXTRA_REQUIRED_ENTITLEMENT, requiredEntitlementIdentifier);
            intent.putExtra(PaywallBridgeActivity.EXTRA_ONLY_IF_NEEDED, onlyIfNeeded);
            intent.putExtra(PaywallBridgeActivity.EXTRA_IS_CUSTOMER_CENTER, isCustomerCenter);
            activity.startActivity(intent);
        } catch (Exception e) {
            sendResultToUnity("ERROR|Failed to present paywall: " + e.getMessage());
        }
    }
    
    /**
     * Internal method for showing customer center
     */
    // Customer center uses the same bridge method (isCustomerCenter=true)
    
    /**
     * Handle the result from PaywallActivityLauncher
     */
    private static void handlePaywallResult(PaywallResult result) {
        try {
            String resultString;
            String simple = result != null ? result.getClass().getSimpleName() : "";
            if ("Purchased".equalsIgnoreCase(simple)) {
                resultString = "PURCHASED|Purchase completed successfully";
            } else if ("Cancelled".equalsIgnoreCase(simple) || "Canceled".equalsIgnoreCase(simple)) {
                resultString = "CANCELLED|Paywall was cancelled by user";
            } else if ("Restored".equalsIgnoreCase(simple)) {
                resultString = "RESTORED|Purchases restored successfully";
            } else if ("NotPresented".equalsIgnoreCase(simple) || "NotShown".equalsIgnoreCase(simple)) {
                resultString = "NOT_PRESENTED|Paywall was not needed";
            } else {
                String asText = String.valueOf(result);
                if (asText.toLowerCase().contains("purchase")) {
                    resultString = "PURCHASED|" + asText;
                } else if (asText.toLowerCase().contains("cancel")) {
                    resultString = "CANCELLED|" + asText;
                } else if (asText.toLowerCase().contains("restore")) {
                    resultString = "RESTORED|" + asText;
                } else if (asText.toLowerCase().contains("not") && asText.toLowerCase().contains("present")) {
                    resultString = "NOT_PRESENTED|" + asText;
                } else {
                    resultString = "ERROR|Unknown result: " + asText;
                }
            }
            sendResultToUnity(resultString);
        } catch (Exception e) {
            sendResultToUnity("ERROR|Failed to handle paywall result: " + e.getMessage());
        }
    }
    
    /**
     * Send result callback to Unity
     */
    private static void sendResultToUnity(String result) {
        try {
            UnityPlayer.UnitySendMessage(UNITY_CALLBACK_OBJECT, "OnPaywallResult", result);
        } catch (Exception e) {
            // If callback fails, at least log it
            android.util.Log.e("RevenueCatUI", "Failed to send result to Unity: " + e.getMessage());
        }
    }
} 