package com.revenuecat.purchasesunity.ui;

import android.app.Activity;
import android.content.Intent;
import android.os.Bundle;
import android.util.Log;

import androidx.activity.ComponentActivity;
import androidx.activity.result.ActivityResultLauncher;
import androidx.activity.result.contract.ActivityResultContracts;
import androidx.annotation.Nullable;

import com.revenuecat.purchases.Purchases;
import com.revenuecat.purchases.ui.revenuecatui.customercenter.CustomerCenterActivity;

public class CustomerCenterTrampolineActivity extends ComponentActivity {
    private static final String TAG = "PurchasesUnity";

    private static final String RESULT_DISMISSED = "DISMISSED";
    private static final String RESULT_ERROR = "ERROR";

    private ActivityResultLauncher<Intent> launcher;

    @Override
    protected void onCreate(@Nullable Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        launcher = registerForActivityResult(
                new ActivityResultContracts.StartActivityForResult(),
                result -> {
                    RevenueCatUI.sendCustomerCenterResult(RESULT_DISMISSED);
                    finish();
                }
        );

        if (!Purchases.isConfigured()) {
            Log.e(TAG, "Purchases is not configured. Cannot launch Customer Center.");
            RevenueCatUI.sendCustomerCenterResult(RESULT_ERROR);
            finish();
            return;
        }

        try {
            Intent intent = CustomerCenterActivity.Companion.createIntent$revenuecatui_defaultsRelease(this);
            launcher.launch(intent);
        } catch (Throwable t) {
            Log.e(TAG, "Error launching CustomerCenterActivity", t);
            RevenueCatUI.sendCustomerCenterResult(RESULT_ERROR);
            finish();
        }
    }

    public static void presentCustomerCenter(Activity activity) {
        if (activity == null) {
            Log.e(TAG, "Activity is null; cannot launch Customer Center");
            RevenueCatUI.sendCustomerCenterResult(RESULT_ERROR);
            return;
        }

        Intent intent = new Intent(activity, CustomerCenterTrampolineActivity.class);

        try {
            activity.startActivity(intent);
        } catch (Throwable t) {
            Log.e(TAG, "Error launching CustomerCenterTrampolineActivity", t);
            RevenueCatUI.sendCustomerCenterResult(RESULT_ERROR);
        }
    }
}
