package com.revenuecat.purchasesunity.ui;

import android.app.Activity;
import android.util.Log;

import androidx.annotation.Nullable;

public class RevenueCatUI {
    public interface PaywallCallbacks { void onPaywallResult(String result); }
    
    public interface CustomerCenterCallbacks {
        void onCustomerCenterDismissed();
        void onCustomerCenterError();
        void onFeedbackSurveyCompleted(String feedbackSurveyOptionId);
        void onShowingManageSubscriptions();
        void onRestoreCompleted(String customerInfoJson);
        void onRestoreFailed(String errorJson);
        void onRestoreStarted();
        void onRefundRequestStarted(String productIdentifier);
        void onRefundRequestCompleted(String productIdentifier, String refundRequestStatus);
        void onManagementOptionSelected(String option, @Nullable String url);
        void onCustomActionSelected(String actionId, @Nullable String purchaseIdentifier);
    }

    private static final String TAG = "RevenueCatUI";
    private static volatile PaywallCallbacks paywallCallbacks;
    private static volatile CustomerCenterCallbacks customerCenterCallbacks;

    public static void registerPaywallCallbacks(PaywallCallbacks cb) { paywallCallbacks = cb; }
    public static void unregisterPaywallCallbacks() { paywallCallbacks = null; }

    public static void registerCustomerCenterCallbacks(CustomerCenterCallbacks cb) { customerCenterCallbacks = cb; }
    public static void unregisterCustomerCenterCallbacks() { customerCenterCallbacks = null; }

    public static void presentPaywall(Activity activity, String offeringIdentifier, String presentedOfferingContextJson, boolean displayCloseButton, String customVariablesJson) {
        PaywallTrampolineActivity.presentPaywall(activity, offeringIdentifier, presentedOfferingContextJson, displayCloseButton, customVariablesJson);
    }

    public static void presentPaywallIfNeeded(Activity activity, String requiredEntitlementIdentifier, String offeringIdentifier, String presentedOfferingContextJson, boolean displayCloseButton, String customVariablesJson) {
        PaywallTrampolineActivity.presentPaywallIfNeeded(activity, requiredEntitlementIdentifier, offeringIdentifier, presentedOfferingContextJson, displayCloseButton, customVariablesJson);
    }

    public static void presentCustomerCenter(Activity activity) {
        CustomerCenterTrampolineActivity.presentCustomerCenter(activity);
    }

    public static void sendPaywallResult(String result) {
        try {
            PaywallCallbacks cb = paywallCallbacks;
            if (cb != null) {
                cb.onPaywallResult(result);
            } else {
                Log.w(TAG, "No callback registered to receive paywall result: " + result);
            }
        } catch (Throwable e) {
            Log.e(TAG, "Error sending paywall result: " + e.getMessage());
        }
    }

    public static void sendCustomerCenterDismissed() {
        try {
            CustomerCenterCallbacks cb = customerCenterCallbacks;
            if (cb != null) {
                cb.onCustomerCenterDismissed();
            } else {
                Log.w(TAG, "No callback registered to receive customer center dismissed");
            }
        } catch (Throwable e) {
            Log.e(TAG, "Error sending customer center dismissed: " + e.getMessage());
        }
    }

    public static void sendCustomerCenterError() {
        try {
            CustomerCenterCallbacks cb = customerCenterCallbacks;
            if (cb != null) {
                cb.onCustomerCenterError();
            } else {
                Log.w(TAG, "No callback registered to receive customer center error");
            }
        } catch (Throwable e) {
            Log.e(TAG, "Error sending customer center error: " + e.getMessage());
        }
    }

    public static void sendFeedbackSurveyCompleted(String feedbackSurveyOptionId) {
        try {
            CustomerCenterCallbacks cb = customerCenterCallbacks;
            if (cb != null) {
                cb.onFeedbackSurveyCompleted(feedbackSurveyOptionId);
            }
        } catch (Throwable e) {
            Log.e(TAG, "Error sending feedback survey completed: " + e.getMessage());
        }
    }

    public static void sendShowingManageSubscriptions() {
        try {
            CustomerCenterCallbacks cb = customerCenterCallbacks;
            if (cb != null) {
                cb.onShowingManageSubscriptions();
            }
        } catch (Throwable e) {
            Log.e(TAG, "Error sending showing manage subscriptions: " + e.getMessage());
        }
    }

    public static void sendRestoreCompleted(String customerInfoJson) {
        try {
            CustomerCenterCallbacks cb = customerCenterCallbacks;
            if (cb != null) {
                cb.onRestoreCompleted(customerInfoJson);
            }
        } catch (Throwable e) {
            Log.e(TAG, "Error sending restore completed: " + e.getMessage());
        }
    }

    public static void sendRestoreFailed(String errorJson) {
        try {
            CustomerCenterCallbacks cb = customerCenterCallbacks;
            if (cb != null) {
                cb.onRestoreFailed(errorJson);
            }
        } catch (Throwable e) {
            Log.e(TAG, "Error sending restore failed: " + e.getMessage());
        }
    }

    public static void sendRestoreStarted() {
        try {
            CustomerCenterCallbacks cb = customerCenterCallbacks;
            if (cb != null) {
                cb.onRestoreStarted();
            }
        } catch (Throwable e) {
            Log.e(TAG, "Error sending restore started: " + e.getMessage());
        }
    }

    public static void sendRefundRequestStarted(String productIdentifier) {
        try {
            CustomerCenterCallbacks cb = customerCenterCallbacks;
            if (cb != null) {
                cb.onRefundRequestStarted(productIdentifier);
            }
        } catch (Throwable e) {
            Log.e(TAG, "Error sending refund request started: " + e.getMessage());
        }
    }

    public static void sendRefundRequestCompleted(String productIdentifier, String refundRequestStatus) {
        try {
            CustomerCenterCallbacks cb = customerCenterCallbacks;
            if (cb != null) {
                cb.onRefundRequestCompleted(productIdentifier, refundRequestStatus);
            }
        } catch (Throwable e) {
            Log.e(TAG, "Error sending refund request completed: " + e.getMessage());
        }
    }

    public static void sendManagementOptionSelected(String option, @Nullable String url) {
        try {
            CustomerCenterCallbacks cb = customerCenterCallbacks;
            if (cb != null) {
                cb.onManagementOptionSelected(option, url);
            }
        } catch (Throwable e) {
            Log.e(TAG, "Error sending management option selected: " + e.getMessage());
        }
    }

    public static void sendCustomActionSelected(String actionId, @Nullable String purchaseIdentifier) {
        try {
            CustomerCenterCallbacks cb = customerCenterCallbacks;
            if (cb != null) {
                cb.onCustomActionSelected(actionId, purchaseIdentifier);
            }
        } catch (Throwable e) {
            Log.e(TAG, "Error sending custom action selected: " + e.getMessage());
        }
    }
}
