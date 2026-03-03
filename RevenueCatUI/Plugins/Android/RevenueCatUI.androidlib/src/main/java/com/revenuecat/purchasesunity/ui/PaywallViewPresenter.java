package com.revenuecat.purchasesunity.ui;

import android.app.Activity;
import android.app.Dialog;
import android.graphics.Color;
import android.graphics.drawable.ColorDrawable;
import android.util.Log;
import android.view.ViewGroup;
import android.view.Window;
import android.view.WindowManager;
import android.widget.FrameLayout;

import androidx.activity.OnBackPressedDispatcher;
import androidx.activity.OnBackPressedDispatcherOwner;
import androidx.activity.ViewTreeOnBackPressedDispatcherOwner;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.lifecycle.Lifecycle;
import androidx.lifecycle.LifecycleRegistry;

import com.revenuecat.purchases.CustomerInfo;
import com.revenuecat.purchases.EntitlementInfo;
import com.revenuecat.purchases.Package;
import com.revenuecat.purchases.PresentedOfferingContext;
import com.revenuecat.purchases.Purchases;
import com.revenuecat.purchases.PurchasesError;
import com.revenuecat.purchases.hybridcommon.ui.HybridPurchaseLogicBridge;
import com.revenuecat.purchases.interfaces.ReceiveCustomerInfoCallback;
import com.revenuecat.purchases.models.StoreTransaction;
import com.revenuecat.purchases.ui.revenuecatui.PaywallListener;
import com.revenuecat.purchases.ui.revenuecatui.utils.Resumable;
import com.revenuecat.purchases.ui.revenuecatui.views.PaywallView;

import org.json.JSONException;
import org.json.JSONObject;

import java.util.Map;

import kotlin.Unit;

public class PaywallViewPresenter {

    private static final String TAG = "PurchasesUnity";

    private static final String RESULT_PURCHASED = "PURCHASED";
    private static final String RESULT_RESTORED = "RESTORED";
    private static final String RESULT_CANCELLED = "CANCELLED";
    private static final String RESULT_ERROR = "ERROR";
    private static final String RESULT_NOT_PRESENTED = "NOT_PRESENTED";

    private static volatile Dialog currentDialog;
    private static volatile String lastResult;
    private static PaywallBackPressedOwner backPressedOwner;
    private static volatile HybridPurchaseLogicBridge currentPurchaseLogicBridge;

    /**
     * A simple OnBackPressedDispatcherOwner that provides the OnBackPressedDispatcher
     * required by Compose's BackHandler composable. Unity's Activity does not extend
     * ComponentActivity, so this owner is not available in the view tree by default.
     */
    private static class PaywallBackPressedOwner implements OnBackPressedDispatcherOwner {
        private final LifecycleRegistry lifecycleRegistry = new LifecycleRegistry(this);
        private final OnBackPressedDispatcher dispatcher = new OnBackPressedDispatcher();

        PaywallBackPressedOwner() {
            lifecycleRegistry.handleLifecycleEvent(Lifecycle.Event.ON_CREATE);
            lifecycleRegistry.handleLifecycleEvent(Lifecycle.Event.ON_START);
            lifecycleRegistry.handleLifecycleEvent(Lifecycle.Event.ON_RESUME);
        }

        @NonNull
        @Override
        public OnBackPressedDispatcher getOnBackPressedDispatcher() {
            return dispatcher;
        }

        @NonNull
        @Override
        public Lifecycle getLifecycle() {
            return lifecycleRegistry;
        }

        void destroy() {
            lifecycleRegistry.handleLifecycleEvent(Lifecycle.Event.ON_PAUSE);
            lifecycleRegistry.handleLifecycleEvent(Lifecycle.Event.ON_STOP);
            lifecycleRegistry.handleLifecycleEvent(Lifecycle.Event.ON_DESTROY);
        }
    }

    public static void presentPaywall(
            Activity activity,
            @Nullable String offeringIdentifier,
            @Nullable String presentedOfferingContextJson,
            boolean displayCloseButton,
            boolean hasPurchaseLogic
    ) {
        if (activity == null) {
            Log.e(TAG, "Activity is null; cannot present paywall");
            RevenueCatUI.sendPaywallResult(RESULT_ERROR);
            return;
        }

        activity.runOnUiThread(() ->
                showPaywallView(activity, offeringIdentifier, presentedOfferingContextJson, displayCloseButton, hasPurchaseLogic)
        );
    }

    public static void presentPaywallIfNeeded(
            Activity activity,
            @NonNull String requiredEntitlementIdentifier,
            @Nullable String offeringIdentifier,
            @Nullable String presentedOfferingContextJson,
            boolean displayCloseButton,
            boolean hasPurchaseLogic
    ) {
        if (activity == null) {
            Log.e(TAG, "Activity is null; cannot present paywall");
            RevenueCatUI.sendPaywallResult(RESULT_ERROR);
            return;
        }

        if (requiredEntitlementIdentifier == null) {
            Log.e(TAG, "Required entitlement identifier is null; cannot present paywall if needed");
            RevenueCatUI.sendPaywallResult(RESULT_ERROR);
            return;
        }

        Purchases.getSharedInstance().getCustomerInfo(new ReceiveCustomerInfoCallback() {
            @Override
            public void onReceived(@NonNull CustomerInfo customerInfo) {
                EntitlementInfo entitlement = customerInfo.getEntitlements().get(requiredEntitlementIdentifier);
                if (entitlement != null && entitlement.isActive()) {
                    RevenueCatUI.sendPaywallResult(RESULT_NOT_PRESENTED);
                } else {
                    activity.runOnUiThread(() ->
                            showPaywallView(activity, offeringIdentifier, presentedOfferingContextJson, displayCloseButton, hasPurchaseLogic)
                    );
                }
            }

            @Override
            public void onError(@NonNull PurchasesError error) {
                Log.w(TAG, "Error checking entitlement, showing paywall anyway: " + error.getMessage());
                activity.runOnUiThread(() ->
                        showPaywallView(activity, offeringIdentifier, presentedOfferingContextJson, displayCloseButton, hasPurchaseLogic)
                );
            }
        });
    }

    @SuppressWarnings("unchecked")
    private static void showPaywallView(
            Activity activity,
            @Nullable String offeringIdentifier,
            @Nullable String presentedOfferingContextJson,
            boolean displayCloseButton,
            boolean hasPurchaseLogic
    ) {
        if (currentDialog != null) {
            Log.w(TAG, "Paywall is already being presented");
            RevenueCatUI.sendPaywallResult(RESULT_ERROR);
            return;
        }

        lastResult = RESULT_CANCELLED;

        // Use a Dialog to host the PaywallView. This creates a separate window that
        // is hardware-accelerated, which is required because Unity's main window may
        // use software rendering. Compose + Coil use hardware bitmaps by default,
        // which crash on a software canvas.
        Dialog dialog = new Dialog(activity, android.R.style.Theme_Light_NoTitleBar_Fullscreen);
        currentDialog = dialog;

        Window window = dialog.getWindow();
        if (window != null) {
            // Ensure this window is hardware accelerated for Compose rendering
            window.addFlags(WindowManager.LayoutParams.FLAG_HARDWARE_ACCELERATED);

            // FLAG_NOT_FOCUSABLE: the Dialog window does not take input focus.
            // The Activity retains focus, which prevents Unity's message processing
            // from breaking after ProxyBillingActivity (Google Play billing) closes.
            // Touch events still reach the Dialog; only key events go to the Activity.
            window.addFlags(WindowManager.LayoutParams.FLAG_NOT_FOCUSABLE);

            // Ensure truly fullscreen on all device sizes (tablets, foldables, etc.)
            window.setLayout(
                    WindowManager.LayoutParams.MATCH_PARENT,
                    WindowManager.LayoutParams.MATCH_PARENT
            );
        }

        // Disable default dialog back-press handling; we handle back via the Activity.
        dialog.setCancelable(false);

        PaywallView paywallView = new PaywallView(activity);

        if (offeringIdentifier != null) {
            PresentedOfferingContext presentedOfferingContext =
                    mapPresentedOfferingContext(presentedOfferingContextJson, offeringIdentifier);
            paywallView.setOfferingId(offeringIdentifier, presentedOfferingContext);
        }

        paywallView.setDisplayDismissButton(displayCloseButton);

        if (hasPurchaseLogic) {
            HybridPurchaseLogicBridge bridge = new HybridPurchaseLogicBridge(
                    eventData -> {
                        String requestId = (String) eventData.get(HybridPurchaseLogicBridge.EVENT_KEY_REQUEST_ID);
                        Object packageMap = eventData.get(HybridPurchaseLogicBridge.EVENT_KEY_PACKAGE_BEING_PURCHASED);
                        String packageJson = "{}";
                        if (packageMap instanceof Map) {
                            try {
                                packageJson = new JSONObject((Map<String, ?>) packageMap).toString();
                            } catch (Throwable e) {
                                Log.w(TAG, "Failed to serialize package to JSON: " + e.getMessage());
                            }
                        }
                        RevenueCatUI.sendPerformPurchase(requestId, packageJson);
                        return Unit.INSTANCE;
                    },
                    eventData -> {
                        String requestId = (String) eventData.get(HybridPurchaseLogicBridge.EVENT_KEY_REQUEST_ID);
                        RevenueCatUI.sendPerformRestore(requestId);
                        return Unit.INSTANCE;
                    }
            );
            currentPurchaseLogicBridge = bridge;
            paywallView.setPurchaseLogic(bridge);
        }

        paywallView.setPaywallListener(new PaywallListener() {
            @Override
            public void onRestoreError(@NonNull PurchasesError purchasesError) {
            }

            @Override
            public void onRestoreStarted() {
            }

            @Override
            public void onPurchaseError(@NonNull PurchasesError purchasesError) {
                lastResult = RESULT_ERROR;
            }

            @Override
            public void onPurchaseStarted(@NonNull Package aPackage) {
            }

            @Override
            public void onPurchaseCancelled() {
            }

            @Override
            public void onPurchasePackageInitiated(@NonNull Package aPackage, @NonNull Resumable resumable) {
                resumable.invoke(true);
            }

            @Override
            public void onPurchaseCompleted(@NonNull CustomerInfo customerInfo,
                                            @NonNull StoreTransaction storeTransaction) {
                lastResult = RESULT_PURCHASED;
            }

            @Override
            public void onRestoreCompleted(@NonNull CustomerInfo customerInfo) {
                lastResult = RESULT_RESTORED;
            }
        });

        paywallView.setDismissHandler(() -> {
            activity.runOnUiThread(() -> {
                String result = lastResult;
                dismissDialog();
                RevenueCatUI.sendPaywallResult(result);
            });
            return Unit.INSTANCE;
        });

        // Set OnBackPressedDispatcherOwner on the dialog's decor view so Compose's
        // BackHandler composable can find it in the view tree.
        if (window != null) {
            ViewGroup decorView = (ViewGroup) window.getDecorView();
            if (ViewTreeOnBackPressedDispatcherOwner.get(decorView) == null) {
                backPressedOwner = new PaywallBackPressedOwner();
                ViewTreeOnBackPressedDispatcherOwner.set(decorView, backPressedOwner);
            }
        }

        dialog.setContentView(paywallView, new FrameLayout.LayoutParams(
                FrameLayout.LayoutParams.MATCH_PARENT,
                FrameLayout.LayoutParams.MATCH_PARENT
        ));

        dialog.show();
    }

    private static void dismissDialog() {
        if (currentDialog != null) {
            try {
                currentDialog.dismiss();
            } catch (Throwable e) {
                Log.w(TAG, "Error dismissing paywall dialog: " + e.getMessage());
            }
            currentDialog = null;
            lastResult = null;
        }
        if (currentPurchaseLogicBridge != null) {
            currentPurchaseLogicBridge.cancelPending();
            currentPurchaseLogicBridge = null;
        }
        if (backPressedOwner != null) {
            backPressedOwner.destroy();
            backPressedOwner = null;
        }
    }

    @Nullable
    private static PresentedOfferingContext mapPresentedOfferingContext(
            @Nullable String jsonString,
            @Nullable String fallbackOfferingId
    ) {
        if (jsonString == null) {
            if (fallbackOfferingId == null) return null;
            return new PresentedOfferingContext(fallbackOfferingId);
        }
        try {
            JSONObject map = new JSONObject(jsonString);
            String offeringIdentifier = map.optString("offeringIdentifier", fallbackOfferingId);
            if (offeringIdentifier == null) return null;

            String placementIdentifier = map.optString("placementIdentifier", null);
            PresentedOfferingContext.TargetingContext targetingContext = null;

            JSONObject targetingMap = map.optJSONObject("targetingContext");
            if (targetingMap != null && targetingMap.has("revision") && targetingMap.has("ruleId")) {
                int revision = targetingMap.optInt("revision");
                String ruleId = targetingMap.optString("ruleId", null);
                if (ruleId != null) {
                    targetingContext = new PresentedOfferingContext.TargetingContext(revision, ruleId);
                }
            }

            return new PresentedOfferingContext(offeringIdentifier, placementIdentifier, targetingContext);
        } catch (JSONException e) {
            Log.w(TAG, "Failed to parse PresentedOfferingContext JSON: " + jsonString, e);
            if (fallbackOfferingId == null) return null;
            return new PresentedOfferingContext(fallbackOfferingId);
        }
    }
}
