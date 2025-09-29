package com.revenuecat.purchasesunity.ui;

import android.app.Activity;
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
    public static final String EXTRA_PAYWALL_OPTIONS = "rc_paywall_options";

    private static final String TAG = "PurchasesUnity";

    private PaywallActivityLauncher launcher;

    @Override
    protected void onCreate(@Nullable Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        final Intent source = getIntent();
        PaywallUnityOptions options = source.getParcelableExtra(EXTRA_PAYWALL_OPTIONS);

        if (options == null) {
            Log.e(TAG, "PaywallUnityOptions is null; cannot launch paywall");
            RevenueCatUI.sendPaywallResult("ERROR");
            finish();
            return;
        }

        launcher = new PaywallActivityLauncher(this, this);

        if (options.getRequiredEntitlementIdentifier() != null) {
            // TODO: Remove debug log before shipping
            Log.d(TAG, "Using launchIfNeeded for entitlement '" + options.getRequiredEntitlementIdentifier() + "'");
            launchPaywallIfNeeded(options);
        } else {
            // TODO: Remove debug log before shipping
            Log.d(TAG, "No entitlement check required, presenting paywall directly");
            launchPaywall(options);
        }
    }

    private void launchPaywallIfNeeded(PaywallUnityOptions options) {
        String requiredEntitlementIdentifier = options.getRequiredEntitlementIdentifier();
        String offeringId = options.getOfferingId();
        boolean shouldDisplayDismissButton = options.getShouldDisplayDismissButton();

        // TODO: Remove debug log before shipping
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
            launchPaywall(options);
        }
    }

    private void launchPaywall(PaywallUnityOptions options) {
        String offeringId = options.getOfferingId();
        boolean shouldDisplayDismissButton = options.getShouldDisplayDismissButton();

        // TODO: Remove debug log before shipping
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

    public static void presentPaywall(Activity activity, @Nullable String offeringIdentifier, boolean displayCloseButton) {
        if (activity == null) {
            Log.e(TAG, "Activity is null; cannot launch paywall");
            RevenueCatUI.sendPaywallResult("ERROR");
            return;
        }

        PaywallUnityOptions options = new PaywallUnityOptions(offeringIdentifier, displayCloseButton, null);

        Intent intent = new Intent(activity, PaywallTrampolineActivity.class);
        intent.putExtra(EXTRA_PAYWALL_OPTIONS, options);

        try {
            activity.startActivity(intent);
        } catch (Throwable t) {
            Log.e(TAG, "Error launching PaywallTrampolineActivity", t);
            RevenueCatUI.sendPaywallResult("ERROR");
        }
    }

    public static void presentPaywallIfNeeded(Activity activity, String requiredEntitlementIdentifier, @Nullable String offeringIdentifier, boolean displayCloseButton) {
        if (activity == null) {
            Log.e(TAG, "Activity is null; cannot launch paywall");
            RevenueCatUI.sendPaywallResult("ERROR");
            return;
        }

        if (requiredEntitlementIdentifier == null) {
            Log.e(TAG, "Required entitlement identifier is null; cannot launch paywall if needed");
            RevenueCatUI.sendPaywallResult("ERROR");
            return;
        }

        PaywallUnityOptions options = new PaywallUnityOptions(offeringIdentifier, displayCloseButton, requiredEntitlementIdentifier);

        Intent intent = new Intent(activity, PaywallTrampolineActivity.class);
        intent.putExtra(EXTRA_PAYWALL_OPTIONS, options);

        try {
            activity.startActivity(intent);
        } catch (Throwable t) {
            Log.e(TAG, "Error launching PaywallTrampolineActivity for presentPaywallIfNeeded", t);
            RevenueCatUI.sendPaywallResult("ERROR");
        }
    }
}