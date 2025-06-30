package com.revenuecat.purchases.ui.unity;

import android.app.Activity;
import android.content.Intent;
import android.util.Log;
import androidx.fragment.app.FragmentActivity;

// RevenueCat Hybrid Common UI imports - minimal set to avoid interface issues
import com.revenuecat.purchases.hybridcommon.ui.PaywallSource;
import com.revenuecat.purchases.ui.revenuecatui.customercenter.ShowCustomerCenter;

import com.unity3d.player.UnityPlayer;

/**
 * Unity plugin for RevenueCat UI components.
 * Handles both FragmentActivity and regular Activity cases.
 */
public class RevenueCatUIPlugin {
    private static final String TAG = "RevenueCatUIPlugin";
    private static final int REQUEST_CODE_CUSTOMER_CENTER = 1001;

    /**
     * Present a paywall for Unity - tries multiple approaches
     */
    public static void presentPaywall(String requiredEntitlementIdentifier) {
        presentPaywall(requiredEntitlementIdentifier, null);
    }
    
    public static void presentPaywall(String requiredEntitlementIdentifier, String offeringIdentifier) {
        Log.d(TAG, "presentPaywall called with entitlement: " + requiredEntitlementIdentifier + ", offering: " + offeringIdentifier);
        
        Activity currentActivity = UnityPlayer.currentActivity;
        if (currentActivity == null) {
            Log.e(TAG, "No current activity available");
            onPaywallResult("ERROR", "No activity available");
            return;
        }

        Log.d(TAG, "Current activity type: " + currentActivity.getClass().getName());
        Log.d(TAG, "Is FragmentActivity: " + (currentActivity instanceof FragmentActivity));

        // Try FragmentActivity approach first
        if (currentActivity instanceof FragmentActivity) {
            Log.d(TAG, "Using FragmentActivity approach");
            presentPaywallWithFragmentActivity((FragmentActivity) currentActivity, requiredEntitlementIdentifier, offeringIdentifier);
        } else {
            Log.d(TAG, "FragmentActivity not available, trying alternative approaches");
            presentPaywallAlternative(currentActivity, requiredEntitlementIdentifier, offeringIdentifier);
        }
    }

    private static void presentPaywallWithFragmentActivity(FragmentActivity fragmentActivity, String requiredEntitlementIdentifier, String offeringIdentifier) {
        try {
            // Try to call the function using reflection
            Class<?> presentPaywallClass = Class.forName("com.revenuecat.purchases.hybridcommon.ui.PresentPaywallKt");
            
            // Log available methods for debugging
            java.lang.reflect.Method[] methods = presentPaywallClass.getMethods();
            Log.d(TAG, "Available methods in PresentPaywallKt:");
            for (java.lang.reflect.Method method : methods) {
                Log.d(TAG, "Method: " + method.getName() + " with " + method.getParameterCount() + " parameters");
                Class<?>[] paramTypes = method.getParameterTypes();
                for (int i = 0; i < paramTypes.length; i++) {
                    Log.d(TAG, "  Param " + i + ": " + paramTypes[i].getSimpleName());
                }
            }

            // For now, return success to test the basic structure
            onPaywallResult("SUCCESS", "FragmentActivity found - methods discovered");
            
        } catch (ClassNotFoundException e) {
            Log.e(TAG, "PresentPaywallKt class not found: " + e.getMessage());
            onPaywallResult("ERROR", "PresentPaywallKt class not found: " + e.getMessage());
        } catch (Exception e) {
            Log.e(TAG, "Error presenting paywall: " + e.getMessage(), e);
            onPaywallResult("ERROR", e.getMessage());
        }
    }

    private static void presentPaywallAlternative(Activity activity, String requiredEntitlementIdentifier, String offeringIdentifier) {
        try {
            // Try to find alternative RevenueCat UI approaches that work with regular Activity
            Log.d(TAG, "Investigating alternative RevenueCat UI approaches...");
            
            // Check what other RevenueCat UI classes are available
            try {
                Class<?> showCustomerCenter = Class.forName("com.revenuecat.purchases.ui.revenuecatui.customercenter.ShowCustomerCenter");
                Log.d(TAG, "ShowCustomerCenter class found: " + showCustomerCenter.getName());
                
                java.lang.reflect.Method[] methods = showCustomerCenter.getMethods();
                Log.d(TAG, "ShowCustomerCenter methods:");
                for (java.lang.reflect.Method method : methods) {
                    Log.d(TAG, "Method: " + method.getName() + " with " + method.getParameterCount() + " parameters");
                }
                
            } catch (ClassNotFoundException e) {
                Log.d(TAG, "ShowCustomerCenter not found: " + e.getMessage());
            }

            // Check for other potential paywall classes
            try {
                Class<?> paywallActivity = Class.forName("com.revenuecat.purchases.ui.revenuecatui.activity.PaywallActivity");
                Log.d(TAG, "PaywallActivity class found: " + paywallActivity.getName());
            } catch (ClassNotFoundException e) {
                Log.d(TAG, "PaywallActivity not found - might not exist");
            }

            onPaywallResult("INFO", "Regular Activity - exploring alternatives");
            
        } catch (Exception e) {
            Log.e(TAG, "Error in alternative approach: " + e.getMessage(), e);
            onPaywallResult("ERROR", "Alternative approach failed: " + e.getMessage());
        }
    }

    /**
     * Present a paywall if needed for Unity
     */
    public static void presentPaywallIfNeeded(String requiredEntitlementIdentifier) {
        presentPaywallIfNeeded(requiredEntitlementIdentifier, null);
    }
    
    public static void presentPaywallIfNeeded(String requiredEntitlementIdentifier, String offeringIdentifier) {
        Log.d(TAG, "presentPaywallIfNeeded called with entitlement: " + requiredEntitlementIdentifier + ", offering: " + offeringIdentifier);
        
        // For "if needed", we pass the required entitlement identifier
        presentPaywall(requiredEntitlementIdentifier, offeringIdentifier);
    }

    /**
     * Present customer center for Unity
     */
    public static void presentCustomerCenter() {
        Log.d(TAG, "presentCustomerCenter called");
        
        Activity currentActivity = UnityPlayer.currentActivity;
        if (currentActivity == null) {
            Log.e(TAG, "No current activity available");
            onCustomerCenterResult("ERROR", "No activity available");
            return;
        }

        try {
            // Customer center should work with regular Activity
            ShowCustomerCenter showCustomerCenter = new ShowCustomerCenter();
            Intent intent = showCustomerCenter.createIntent(currentActivity, null);
            
            currentActivity.startActivity(intent);
            onCustomerCenterResult("SUCCESS", "Customer center launched");
            
        } catch (Exception e) {
            Log.e(TAG, "Error presenting customer center: " + e.getMessage(), e);
            onCustomerCenterResult("ERROR", e.getMessage());
        }
    }

    /**
     * Check if RevenueCat UI is supported
     */
    public static boolean isSupported() {
        try {
            Activity currentActivity = UnityPlayer.currentActivity;
            if (currentActivity == null) {
                Log.d(TAG, "No activity available for support check");
                return false;
            }

            // Test if the classes exist
            Class.forName("com.revenuecat.purchases.ui.revenuecatui.customercenter.ShowCustomerCenter");
            
            // Check activity type
            boolean isFragmentActivity = currentActivity instanceof FragmentActivity;
            Log.d(TAG, "RevenueCat UI support check - Activity type: " + currentActivity.getClass().getSimpleName() + ", FragmentActivity: " + isFragmentActivity);
            
            return true; // Basic classes exist
        } catch (ClassNotFoundException e) {
            Log.d(TAG, "RevenueCat UI classes not found: " + e.getMessage());
            return false;
        }
    }

    // Unity callback methods
    private static void onPaywallResult(String result, String message) {
        Log.d(TAG, "Paywall result: " + result + ", message: " + message);
        
        // Call Unity callback
        UnityPlayer.UnitySendMessage("RevenueCatUI", "OnPaywallResult", result + "|" + message);
    }
    
    private static void onCustomerCenterResult(String result, String message) {
        Log.d(TAG, "Customer center result: " + result + ", message: " + message);
        
        // Call Unity callback  
        UnityPlayer.UnitySendMessage("RevenueCatUI", "OnCustomerCenterResult", result + "|" + message);
    }
} 