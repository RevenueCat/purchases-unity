package com.revenuecat.purchasesunity.ui;

import android.app.Activity;
import android.content.Intent;
import android.os.Bundle;
import android.util.Log;

import androidx.activity.ComponentActivity;
import androidx.activity.result.ActivityResultLauncher;
import androidx.annotation.Nullable;

import com.revenuecat.purchases.Purchases;
import com.revenuecat.purchases.customercenter.CustomerCenterListener;
import com.revenuecat.purchases.hybridcommon.mappers.MappersHelpersKt;
import com.revenuecat.purchases.hybridcommon.ui.CustomerCenterListenerWrapper;
import com.revenuecat.purchases.ui.revenuecatui.customercenter.ShowCustomerCenter;

import java.util.Map;

import kotlin.Unit;

public class CustomerCenterTrampolineActivity extends ComponentActivity {
    private static final String TAG = "PurchasesUnity";

    private ActivityResultLauncher<Unit> launcher;
    private CustomerCenterListener customerCenterListener;

    @Override
    protected void onCreate(@Nullable Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        launcher = registerForActivityResult(
                new ShowCustomerCenter(),
                ignored -> {
                    RevenueCatUI.sendCustomerCenterDismissed();
                    finish();
                }
        );

        if (!Purchases.isConfigured()) {
            Log.e(TAG, "Purchases is not configured. Cannot launch Customer Center.");
            RevenueCatUI.sendCustomerCenterError();
            finish();
            return;
        }

        customerCenterListener = createCustomerCenterListener();
        Purchases.getSharedInstance().setCustomerCenterListener(customerCenterListener);

        try {
            launcher.launch(Unit.INSTANCE);
        } catch (Throwable t) {
            Log.e(TAG, "Error launching CustomerCenterActivity", t);
            RevenueCatUI.sendCustomerCenterError();
            finish();
        }
    }

    @Override
    protected void onDestroy() {
        super.onDestroy();
        if (Purchases.isConfigured()) {
            Purchases.getSharedInstance().setCustomerCenterListener(null);
        }
    }

    private CustomerCenterListener createCustomerCenterListener() {
        return new CustomerCenterListenerWrapper() {
            @Override
            public void onFeedbackSurveyCompletedWrapper(@Nullable String feedbackSurveyOptionId) {
                if (feedbackSurveyOptionId != null) {
                    RevenueCatUI.sendFeedbackSurveyCompleted(feedbackSurveyOptionId);
                }
            }

            @Override
            public void onManagementOptionSelectedWrapper(@Nullable String action, @Nullable String url) {
                if (action != null) {
                    RevenueCatUI.sendManagementOptionSelected(action, url);
                }
            }

            @Override
            public void onCustomActionSelectedWrapper(@Nullable String actionId, @Nullable String purchaseIdentifier) {
                if (actionId != null) {
                    RevenueCatUI.sendCustomActionSelected(actionId, purchaseIdentifier);
                }
            }

            @Override
            public void onShowingManageSubscriptionsWrapper() {
                RevenueCatUI.sendShowingManageSubscriptions();
            }

            @Override
            public void onRestoreCompletedWrapper(@Nullable Map<String, ?> customerInfo) {
                if (customerInfo != null) {
                    String customerInfoJson = MappersHelpersKt.convertToJson(customerInfo).toString();
                    RevenueCatUI.sendRestoreCompleted(customerInfoJson);
                }
            }

            @Override
            public void onRestoreFailedWrapper(@Nullable Map<String, ?> error) {
                if (error != null) {
                    String errorJson = MappersHelpersKt.convertToJson(error).toString();
                    RevenueCatUI.sendRestoreFailed(errorJson);
                }
            }

            @Override
            public void onRestoreStartedWrapper() {
                RevenueCatUI.sendRestoreStarted();
            }
        };
    }

    public static void presentCustomerCenter(Activity activity) {
        if (activity == null) {
            Log.e(TAG, "Activity is null; cannot launch Customer Center");
            RevenueCatUI.sendCustomerCenterError();
            return;
        }

        Intent intent = new Intent(activity, CustomerCenterTrampolineActivity.class);

        try {
            activity.startActivity(intent);
        } catch (Throwable t) {
            Log.e(TAG, "Error launching CustomerCenterTrampolineActivity", t);
            RevenueCatUI.sendCustomerCenterError();
        }
    }
}
