package com.revenuecat.purchasesunity.ui;

import android.content.Intent;
import android.os.Bundle;
import android.util.Log;

import androidx.annotation.Nullable;
import androidx.activity.ComponentActivity;

import com.revenuecat.purchases.ui.revenuecatui.activity.PaywallActivityLauncher;
import com.revenuecat.purchases.ui.revenuecatui.activity.PaywallResult;
import com.revenuecat.purchases.ui.revenuecatui.activity.PaywallResultHandler;
import com.revenuecat.purchases.PresentedOfferingContext;

public class PaywallTrampolineActivity extends ComponentActivity implements PaywallResultHandler {
    public static final String EXTRA_OFFERING_ID = "rc_offering_id";
    public static final String EXTRA_SHOULD_DISPLAY_DISMISS_BUTTON = "rc_should_display_dismiss_button";
    public static final String EXTRA_REQUIRED_ENTITLEMENT_IDENTIFIER = "rc_required_entitlement_identifier";

    private static final String TAG = "PurchasesUnity";

    private PaywallActivityLauncher launcher;

    @Override
    protected void onCreate(@Nullable Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        final Intent source = getIntent();
        String offeringId = source.getStringExtra(EXTRA_OFFERING_ID);
        boolean shouldDisplayDismissButton = source.getBooleanExtra(EXTRA_SHOULD_DISPLAY_DISMISS_BUTTON, false);
        String requiredEntitlementIdentifier = source.getStringExtra(EXTRA_REQUIRED_ENTITLEMENT_IDENTIFIER);

        launcher = new PaywallActivityLauncher(this, this);

        if (requiredEntitlementIdentifier != null) {
            // TODO: Remove debug log before shipping
            Log.d(TAG, "Using launchIfNeeded for entitlement '" + requiredEntitlementIdentifier + "'");
            launchPaywallIfNeeded(requiredEntitlementIdentifier, offeringId, shouldDisplayDismissButton);
        } else {
            // TODO: Remove debug log before shipping
            Log.d(TAG, "No entitlement check required, presenting paywall directly");
            launchPaywall(offeringId, shouldDisplayDismissButton);
        }
    }

    private void launchPaywallIfNeeded(String requiredEntitlementIdentifier, String offeringId, boolean shouldDisplayDismissButton) {
        // TODO: Remove debug logs before shipping
        Log.d(TAG, "Launching paywall if needed with PaywallActivityLauncher");
        Log.d(TAG, "Options - entitlement: " + requiredEntitlementIdentifier + ", offering: " + offeringId + ", dismissButton: " + shouldDisplayDismissButton);
        
        if (offeringId != null) {
            // TODO: Remove debug log before shipping
            Log.d(TAG, "Using launchIfNeeded with offering ID");
            launcher.launchIfNeeded(
                requiredEntitlementIdentifier,
                offeringId,
                new PresentedOfferingContext(offeringId), // TODO: pass PresentedOfferingContext data
                null, // fontProvider
                shouldDisplayDismissButton,
                false, // edgeToEdge
                paywallDisplayResult -> {
                    if (!paywallDisplayResult) {
                        // TODO: Remove debug log before shipping
                        Log.d(TAG, "PaywallDisplayCallback: paywall not needed");
                        RevenueCatUI.sendPaywallResult("NOT_PRESENTED");
                        finish();
                    }
                    // If paywallDisplayResult is true, the paywall will be shown and result will come through normal callback
                }
            );
        } else {
            Log.w(TAG, "launchIfNeeded requires an offering ID, falling back to regular launch");
            launchPaywall(offeringId, shouldDisplayDismissButton);
        }
    }

    private void launchPaywall(String offeringId, boolean shouldDisplayDismissButton) {
        // TODO: Remove debug logs before shipping
        Log.d(TAG, "Launching paywall with PaywallActivityLauncher");
        Log.d(TAG, "Options - offering: " + offeringId + ", dismissButton: " + shouldDisplayDismissButton);
        
        if (offeringId != null) {
            // TODO: Remove debug log before shipping
            Log.d(TAG, "Launching paywall with offering ID");
            launcher.launch(offeringId,
                    new PresentedOfferingContext(offeringId), // TODO: pass PresentedOfferingContext data
                    null, // fontProvider
                    shouldDisplayDismissButton);
        } else {
            // TODO: Remove debug log before shipping
            Log.d(TAG, "Launching paywall with standard method");
            launcher.launch(
                null, // offering (Offering object, not String)
                null, // fontProvider
                shouldDisplayDismissButton
            );
        }
    }

    @Override
    public void onActivityResult(PaywallResult result) {
        try {
            if (result != null) {
                sendPaywallResult(result);
            }
        } finally {
            finish();
        }
    }


    private void sendPaywallResult(PaywallResult result) {
        final String resultName;
        if (result instanceof PaywallResult.Purchased) {
            resultName = "PURCHASED";
        } else if (result instanceof PaywallResult.Restored) {
            resultName = "RESTORED";
        } else if (result instanceof PaywallResult.Cancelled) {
            resultName = "CANCELLED";
        } else if (result instanceof PaywallResult.Error) {
            resultName = "ERROR";
        } else {
            resultName = "CANCELLED";
        }

        RevenueCatUI.sendPaywallResult(resultName);
    }
}