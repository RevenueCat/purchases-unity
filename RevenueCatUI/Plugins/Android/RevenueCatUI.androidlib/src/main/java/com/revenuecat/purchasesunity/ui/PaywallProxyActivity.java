package com.revenuecat.purchasesunity.ui;

import android.content.Intent;
import android.os.Bundle;
import android.util.Log;

import androidx.annotation.Nullable;
import androidx.activity.ComponentActivity;

import com.revenuecat.purchases.ui.revenuecatui.activity.PaywallActivityLauncher;
import com.revenuecat.purchases.ui.revenuecatui.activity.PaywallResult;
import com.revenuecat.purchases.PresentedOfferingContext;

public class PaywallProxyActivity extends ComponentActivity {
    public static final String EXTRA_GAME_OBJECT = "rc_proxy_game_object";
    public static final String EXTRA_METHOD     = "rc_proxy_method";
    public static final String EXTRA_OFFERING_ID = "rc_offering_id";
    public static final String EXTRA_SHOULD_DISPLAY_DISMISS_BUTTON = "rc_should_display_dismiss_button";

    private static final String TAG = "PurchasesUnity";

    private String gameObject;
    private String method;

    @Override
    protected void onCreate(@Nullable Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        final Intent source = getIntent();
        gameObject = source.getStringExtra(EXTRA_GAME_OBJECT);
        method     = source.getStringExtra(EXTRA_METHOD);
        String offeringId = source.getStringExtra(EXTRA_OFFERING_ID);
        boolean shouldDisplayDismissButton = source.getBooleanExtra(EXTRA_SHOULD_DISPLAY_DISMISS_BUTTON, false);

        if (gameObject == null || method == null) {
            Log.w(TAG, "Missing gameObject/method extras; finishing.");
            finish();
            return;
        }

        PaywallActivityLauncher launcher = new PaywallActivityLauncher(
            this,
            result -> {
                try {
                    if (result != null) {
                        sendPaywallResult(result);
                    }
                } finally {
                    finish();
                }
            }
        );

        Log.d(TAG, "Launching paywall with PaywallActivityLauncher");
        Log.d(TAG, "Options - offering: " + offeringId + ", dismissButton: " + shouldDisplayDismissButton);
        
        if (offeringId != null) {
            Log.d(TAG, "Launching paywall with offering ID using deprecated method");
            launcher.launch(offeringId,
                    new PresentedOfferingContext(offeringId), // TODO: pass PresentedOfferingContext data
                    null, // fontProvider
                    shouldDisplayDismissButton);
        } else {
            Log.d(TAG, "Launching paywall with standard method");
            launcher.launch(
                null, // offering (Offering object, not String)
                null, // fontProvider
                shouldDisplayDismissButton
            );
        }
    }

    private void sendPaywallResult(PaywallResult result) {
        final String resultName;
        if (result instanceof PaywallResult.Purchased) {
            resultName = "purchased";
        } else if (result instanceof PaywallResult.Restored) {
            resultName = "restored";
        } else if (result instanceof PaywallResult.Cancelled) {
            resultName = "cancelled";
        } else if (result instanceof PaywallResult.Error) {
            resultName = "error";
        } else {
            resultName = "cancelled";
        }

        Log.d(TAG, "Sending PaywallResult: " + resultName);

        runOnUiThread(() -> UnityBridge.sendMessage(gameObject, method, resultName));
    }
}