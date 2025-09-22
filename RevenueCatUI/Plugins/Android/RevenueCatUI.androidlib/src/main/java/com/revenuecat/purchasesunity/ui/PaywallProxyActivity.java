package com.revenuecat.purchasesunity.ui;

import android.content.Intent;
import android.os.Bundle;
import android.util.Log;

import androidx.annotation.Nullable;
import androidx.appcompat.app.AppCompatActivity;

import com.revenuecat.purchases.ui.revenuecatui.activity.PaywallActivityLauncher;
import com.revenuecat.purchases.ui.revenuecatui.activity.PaywallResult;

public class PaywallProxyActivity extends AppCompatActivity {
    public static final String EXTRA_GAME_OBJECT = "rc_proxy_game_object";
    public static final String EXTRA_METHOD     = "rc_proxy_method";
    public static final String EXTRA_OFFERING_ID = "rc_offering_id";

    private static final String TAG = "PurchasesUnity";

    private String gameObject;
    private String method;

    @Override
    protected void onCreate(@Nullable Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        final Intent source = getIntent();
        gameObject = source.getStringExtra(EXTRA_GAME_OBJECT);
        method     = source.getStringExtra(EXTRA_METHOD);

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
        // If you ever need to pass an offering, wire it via intent extras and into the launcher
        launcher.launch(null);
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

        // UnitySendMessage is safest from the UI thread. We're already on it, but guard anyway.
        runOnUiThread(() -> UnityBridge.sendMessage(gameObject, method, resultName));
    }
}