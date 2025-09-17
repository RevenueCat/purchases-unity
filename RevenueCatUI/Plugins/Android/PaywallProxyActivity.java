package com.revenuecat.purchasesunity;

import android.content.Intent;
import android.os.Bundle;
import android.util.Log;

import androidx.annotation.Nullable;
import androidx.appcompat.app.AppCompatActivity;

import com.revenuecat.purchases.ui.revenuecatui.activity.PaywallActivityLauncher;
import com.revenuecat.purchases.ui.revenuecatui.activity.PaywallResult;
import com.revenuecat.purchases.ui.revenuecatui.activity.PaywallResultHandler;
import com.unity3d.player.UnityPlayer;

public class PaywallProxyActivity extends AppCompatActivity {
    static final String EXTRA_GAME_OBJECT = "rc_proxy_game_object";
    static final String EXTRA_METHOD = "rc_proxy_method";
    static final String EXTRA_OFFERING_ID = "rc_offering_id";

    private String gameObject;
    private String method;

    @Override
    protected void onCreate(@Nullable Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        
        Intent source = getIntent();
        gameObject = source.getStringExtra(EXTRA_GAME_OBJECT);
        method = source.getStringExtra(EXTRA_METHOD);

        PaywallActivityLauncher launcher = new PaywallActivityLauncher(this, result -> {
            try {
                if (result != null) {
                    sendPaywallResult(result);
                }
            } finally {
                finish();
            }
        });

        Log.d("PurchasesUnity", "Launching paywall with PaywallActivityLauncher");
        launcher.launch(null);
    }

    private void sendPaywallResult(PaywallResult result) {
        if (gameObject == null || method == null) return;
        
        String resultName;
        if (result instanceof PaywallResult.Purchased) {
            resultName = "purchased";
        } else if (result instanceof PaywallResult.Restored) {
            resultName = "restored";
        } else if (result instanceof PaywallResult.Cancelled) {
            resultName = "cancelled";
        } else if (result instanceof PaywallResult.Error) {
            resultName = "error";
        } else {
            resultName = "cancelled"; // fallback
        }
        
        Log.d("PurchasesUnity", "Sending PaywallResult: " + resultName);
        UnityPlayer.UnitySendMessage(gameObject, method, resultName);
    }
}


