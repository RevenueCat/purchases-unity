package com.revenuecat.unity;

import android.app.Activity;
import android.content.Intent;
import android.os.Bundle;
import androidx.appcompat.app.AppCompatActivity;
import com.revenuecat.purchases.ui.revenuecatui.activity.PaywallActivityLauncher;
import com.revenuecat.purchases.ui.revenuecatui.activity.PaywallResultHandler;
import com.revenuecat.purchases.ui.revenuecatui.activity.PaywallResult;
import com.unity3d.player.UnityPlayer;

/**
 * Optional host-wrapper activity that extends AppCompatActivity
 * This is used as a fallback when the Unity activity is not a FragmentActivity
 * Because it extends AppCompatActivity, it already implements the ActivityResultCaller interface
 * that PaywallActivityLauncher needs.
 */
public class PaywallBridgeActivity extends AppCompatActivity {
    
    public static final String EXTRA_OFFERING_ID = "offering_id";
    public static final String EXTRA_REQUIRED_ENTITLEMENT = "required_entitlement";
    public static final String EXTRA_ONLY_IF_NEEDED = "only_if_needed";
    public static final String EXTRA_IS_CUSTOMER_CENTER = "is_customer_center";
    
    private static final String UNITY_CALLBACK_OBJECT = "RevenueCatUI";
    
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        
        // Get parameters from intent
        String offeringId = getIntent().getStringExtra(EXTRA_OFFERING_ID);
        String requiredEntitlement = getIntent().getStringExtra(EXTRA_REQUIRED_ENTITLEMENT);
        boolean onlyIfNeeded = getIntent().getBooleanExtra(EXTRA_ONLY_IF_NEEDED, false);
        boolean isCustomerCenter = getIntent().getBooleanExtra(EXTRA_IS_CUSTOMER_CENTER, false);
        
        try {
            if (isCustomerCenter) {
                // For now, immediately report not implemented to customer center handler
                sendCustomerCenterResultToUnity("ERROR|Customer center not yet implemented");
                finish();
            } else {
                // Launch RevenueCat paywall
                PaywallActivityLauncher launcher = new PaywallActivityLauncher(this, new PaywallResultHandler() {
                    @Override
                    public void onActivityResult(PaywallResult result) {
                        handlePaywallResult(result);
                        finish(); // Close this bridge activity
                    }
                });
                
                // Configure the launcher based on parameters (use reflection for API compatibility)
                try {
                    if (offeringId != null && !offeringId.isEmpty()) {
                        boolean invoked = false;
                        try {
                            PaywallActivityLauncher.class
                                .getMethod("setOfferingIdentifier", String.class)
                                .invoke(launcher, offeringId);
                            invoked = true;
                        } catch (NoSuchMethodException ignored) {}
                        if (!invoked) {
                            try {
                                PaywallActivityLauncher.class
                                    .getMethod("setOfferingId", String.class)
                                    .invoke(launcher, offeringId);
                                invoked = true;
                            } catch (NoSuchMethodException ignored) {}
                        }
                        if (!invoked) {
                            try {
                                PaywallActivityLauncher.class
                                    .getMethod("setOffering", String.class)
                                    .invoke(launcher, offeringId);
                            } catch (NoSuchMethodException ignored) {}
                        }
                    }
                    if (onlyIfNeeded && requiredEntitlement != null && !requiredEntitlement.isEmpty()) {
                        boolean invokedReq = false;
                        try {
                            PaywallActivityLauncher.class
                                .getMethod("setRequiredEntitlementIdentifier", String.class)
                                .invoke(launcher, requiredEntitlement);
                            invokedReq = true;
                        } catch (NoSuchMethodException ignored) {}
                        if (!invokedReq) {
                            try {
                                PaywallActivityLauncher.class
                                    .getMethod("setRequiredEntitlementId", String.class)
                                    .invoke(launcher, requiredEntitlement);
                                invokedReq = true;
                            } catch (NoSuchMethodException ignored) {}
                        }
                        if (!invokedReq) {
                            try {
                                PaywallActivityLauncher.class
                                    .getMethod("setRequiredEntitlement", String.class)
                                    .invoke(launcher, requiredEntitlement);
                            } catch (NoSuchMethodException ignored) {}
                        }
                    }
                } catch (Exception reflectionError) {
                    android.util.Log.w("RevenueCatUI", "Optional configuration via reflection failed: " + reflectionError.getMessage());
                }
                
                launcher.launch(); // Launch full-screen paywall
            }
        } catch (Exception e) {
            sendResultToUnity("ERROR|Failed to launch paywall: " + e.getMessage());
            finish();
        }
    }
    
    /**
     * Handle the result from PaywallActivityLauncher
     */
    private void handlePaywallResult(PaywallResult result) {
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
                // Fallback: try toString pattern matching
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
    private void sendResultToUnity(String result) {
        try {
            UnityPlayer.UnitySendMessage(UNITY_CALLBACK_OBJECT, "OnPaywallResult", result);
        } catch (Exception e) {
            // If callback fails, at least log it
            android.util.Log.e("RevenueCatUI", "Failed to send result to Unity: " + e.getMessage());
        }
    }

    private void sendCustomerCenterResultToUnity(String result) {
        try {
            UnityPlayer.UnitySendMessage(UNITY_CALLBACK_OBJECT, "OnCustomerCenterResult", result);
        } catch (Exception e) {
            android.util.Log.e("RevenueCatUI", "Failed to send CC result to Unity: " + e.getMessage());
        }
    }
    
    /**
     * Static method to launch this bridge activity from Unity
     * @deprecated This method is kept for backward compatibility but the new bridge pattern
     * should be used instead through RevenueCatUIPlugin.presentPaywall()
     */
    @Deprecated
    public static void launch(Activity fromActivity, String offeringId, String requiredEntitlement, int requestCode) {
        Intent intent = new Intent(fromActivity, PaywallBridgeActivity.class);
        intent.putExtra(EXTRA_OFFERING_ID, offeringId);
        intent.putExtra(EXTRA_REQUIRED_ENTITLEMENT, requiredEntitlement);
        intent.putExtra(EXTRA_ONLY_IF_NEEDED, false);
        fromActivity.startActivityForResult(intent, requestCode);
    }
} 