package com.revenuecat.purchasesunity.ui;

import android.app.Activity;
import android.content.Intent;
import android.os.Build;
import android.os.Bundle;
import android.util.Log;

import androidx.annotation.Nullable;
import androidx.activity.ComponentActivity;

import com.revenuecat.purchases.ui.revenuecatui.activity.PaywallActivityLauncher;
import com.revenuecat.purchases.ui.revenuecatui.activity.PaywallDisplayCallback;
import com.revenuecat.purchases.ui.revenuecatui.activity.PaywallResult;
import com.revenuecat.purchases.ui.revenuecatui.activity.PaywallResultHandler;
import com.revenuecat.purchases.PresentedOfferingContext;

public class PaywallTrampolineActivity extends ComponentActivity implements PaywallResultHandler {
    public static final String EXTRA_PAYWALL_OPTIONS = "rc_paywall_options";

    private static final String TAG = "PurchasesUnity";

    private static final String RESULT_PURCHASED = "PURCHASED";
    private static final String RESULT_RESTORED = "RESTORED";
    private static final String RESULT_CANCELLED = "CANCELLED";
    private static final String RESULT_ERROR = "ERROR";
    private static final String RESULT_NOT_PRESENTED = "NOT_PRESENTED";

    private PaywallActivityLauncher launcher;

    @Override
    protected void onCreate(@Nullable Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        final Intent source = getIntent();
        PaywallUnityOptions options = source.getParcelableExtra(EXTRA_PAYWALL_OPTIONS);

        if (options == null) {
            Log.e(TAG, "PaywallUnityOptions is null; cannot launch paywall");
            RevenueCatUI.sendPaywallResult(RESULT_ERROR);
            finish();
            return;
        }

        launcher = new PaywallActivityLauncher(this, this);

        if (options.getRequiredEntitlementIdentifier() != null) {
            launchPaywallIfNeeded(options);
        } else {
            launchPaywall(options);
        }
    }

    private void launchPaywallIfNeeded(PaywallUnityOptions options) {
        String requiredEntitlementIdentifier = options.getRequiredEntitlementIdentifier();
        String offeringId = options.getOfferingId();
        boolean shouldDisplayDismissButton = options.getShouldDisplayDismissButton();

        if (offeringId == null) {
            launcher.launchIfNeeded(
                    requiredEntitlementIdentifier,
                    null,
                    null,
                    shouldDisplayDismissButton,
                    Build.VERSION.SDK_INT >= 35,
                    paywallDisplayResult -> {
                        if (!paywallDisplayResult) {
                            RevenueCatUI.sendPaywallResult(RESULT_NOT_PRESENTED);
                            finish();
                        }
                    }
            );
        } else {
            launcher.launchIfNeededWithOfferingId(
                    requiredEntitlementIdentifier,
                    offeringId,
                    new PresentedOfferingContext(offeringId),
                    null,
                    shouldDisplayDismissButton,
                    Build.VERSION.SDK_INT >= 35,
                    paywallDisplayResult -> {
                        if (!paywallDisplayResult) {
                            RevenueCatUI.sendPaywallResult(RESULT_NOT_PRESENTED);
                            finish();
                        }
                    }
            );
        }
    }

    private void launchPaywall(PaywallUnityOptions options) {
        String offeringId = options.getOfferingId();
        boolean shouldDisplayDismissButton = options.getShouldDisplayDismissButton();

        // TODO: add support for edge to edge, fonts, and offering context
        if (offeringId != null) {
            launcher.launchWithOfferingId(
                    offeringId,
                    new PresentedOfferingContext(offeringId), // TODO: support passing context
                    null,
                    shouldDisplayDismissButton
            );
        } else {
            launcher.launch(
                null,
                null,
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
            resultName = RESULT_PURCHASED;
        } else if (result instanceof PaywallResult.Restored) {
            resultName = RESULT_RESTORED;
        } else if (result instanceof PaywallResult.Cancelled) {
            resultName = RESULT_CANCELLED;
        } else if (result instanceof PaywallResult.Error) {
            resultName = RESULT_ERROR;
        } else {
            resultName = RESULT_CANCELLED;
        }

        RevenueCatUI.sendPaywallResult(resultName);
    }

    public static void presentPaywall(Activity activity, @Nullable String offeringIdentifier, boolean displayCloseButton) {
        if (activity == null) {
            Log.e(TAG, "Activity is null; cannot launch paywall");
            RevenueCatUI.sendPaywallResult(RESULT_ERROR);
            return;
        }

        PaywallUnityOptions options = new PaywallUnityOptions(offeringIdentifier, displayCloseButton, null);

        Intent intent = new Intent(activity, PaywallTrampolineActivity.class);
        intent.putExtra(EXTRA_PAYWALL_OPTIONS, options);

        try {
            activity.startActivity(intent);
        } catch (Throwable t) {
            Log.e(TAG, "Error launching PaywallTrampolineActivity", t);
            RevenueCatUI.sendPaywallResult(RESULT_ERROR);
        }
    }

    public static void presentPaywallIfNeeded(Activity activity, String requiredEntitlementIdentifier, @Nullable String offeringIdentifier, boolean displayCloseButton) {
        if (activity == null) {
            Log.e(TAG, "Activity is null; cannot launch paywall");
            RevenueCatUI.sendPaywallResult(RESULT_ERROR);
            return;
        }

        if (requiredEntitlementIdentifier == null) {
            Log.e(TAG, "Required entitlement identifier is null; cannot launch paywall if needed");
            RevenueCatUI.sendPaywallResult(RESULT_ERROR);
            return;
        }

        PaywallUnityOptions options = new PaywallUnityOptions(offeringIdentifier, displayCloseButton, requiredEntitlementIdentifier);

        Intent intent = new Intent(activity, PaywallTrampolineActivity.class);
        intent.putExtra(EXTRA_PAYWALL_OPTIONS, options);

        try {
            activity.startActivity(intent);
        } catch (Throwable t) {
            Log.e(TAG, "Error launching PaywallTrampolineActivity for presentPaywallIfNeeded", t);
            RevenueCatUI.sendPaywallResult(RESULT_ERROR);
        }
    }
}