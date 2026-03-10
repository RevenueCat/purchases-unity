package com.revenuecat.purchasesunity.ui;

import android.app.Activity;
import android.app.Dialog;
import android.os.Build;
import android.util.Log;
import android.view.View;
import android.view.Window;
import android.view.WindowManager;
import android.widget.FrameLayout;
import android.window.OnBackInvokedCallback;
import android.window.OnBackInvokedDispatcher;

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
    private static Object backInvokedCallback; // OnBackInvokedCallback (API 33+)

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
        // The subclass overrides onBackPressed() so KEYCODE_BACK (pre-API 33 /
        // button navigation) is forwarded to the paywall's OnBackPressedDispatcher.
        @SuppressWarnings("deprecation") // onBackPressed is deprecated in API 33+; we
        // still need it for KEYCODE_BACK on pre-33 / button nav. API 33+ gesture back
        // is handled separately via OnBackInvokedCallback.
        Dialog dialog = new Dialog(activity, android.R.style.Theme_Light_NoTitleBar_Fullscreen) {
            @Override
            public void onBackPressed() {
                handleBackPress(activity);
            }
        };
        currentDialog = dialog;

        Window window = dialog.getWindow();
        if (window != null) {
            // Ensure this window is hardware accelerated for Compose rendering
            window.addFlags(WindowManager.LayoutParams.FLAG_HARDWARE_ACCELERATED);

            // NOTE: FLAG_NOT_FOCUSABLE is NOT set here so the Dialog can receive
            // back navigation events (key events on pre-API 33, gesture back on
            // API 33+). For PurchaseLogic, FLAG_NOT_FOCUSABLE is toggled on/off
            // only during active purchase/restore operations to fix a threading
            // issue with PurchasesAreCompletedBy.MyApp.

            // Ensure truly fullscreen on all device sizes (tablets, foldables, etc.)
            window.setLayout(
                    WindowManager.LayoutParams.MATCH_PARENT,
                    WindowManager.LayoutParams.MATCH_PARENT
            );
        }

        // Disable default dialog cancel-on-back; our onBackPressed override and
        // OnBackInvokedCallback handle back navigation instead.
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
                        // Add FLAG_NOT_FOCUSABLE during purchase to prevent
                        // threading issues with PurchaseLogic results.
                        setDialogNotFocusable(true);
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
                        // Add FLAG_NOT_FOCUSABLE during restore to prevent
                        // threading issues with PurchaseLogic results.
                        setDialogNotFocusable(true);
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
                setDialogNotFocusable(false);
            }

            @Override
            public void onRestoreStarted() {
            }

            @Override
            public void onPurchaseError(@NonNull PurchasesError purchasesError) {
                lastResult = RESULT_ERROR;
                setDialogNotFocusable(false);
            }

            @Override
            public void onPurchaseStarted(@NonNull Package aPackage) {
            }

            @Override
            public void onPurchaseCancelled() {
                setDialogNotFocusable(false);
            }

            @Override
            public void onPurchasePackageInitiated(@NonNull Package aPackage, @NonNull Resumable resumable) {
                resumable.invoke(true);
            }

            @Override
            public void onPurchaseCompleted(@NonNull CustomerInfo customerInfo,
                                            @NonNull StoreTransaction storeTransaction) {
                lastResult = RESULT_PURCHASED;
                setDialogNotFocusable(false);
            }

            @Override
            public void onRestoreCompleted(@NonNull CustomerInfo customerInfo) {
                lastResult = RESULT_RESTORED;
                setDialogNotFocusable(false);
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

        // --- Back navigation setup ---
        // Set OnBackPressedDispatcherOwner on the dialog's decor view so Compose's
        // BackHandler composable can find it in the view tree.
        if (window != null) {
            View decorView = window.getDecorView();
            if (ViewTreeOnBackPressedDispatcherOwner.get(decorView) == null) {
                backPressedOwner = new PaywallBackPressedOwner();
                ViewTreeOnBackPressedDispatcherOwner.set(decorView, backPressedOwner);
            }

            // For API 33+ gesture navigation, register OnBackInvokedCallback on
            // the Dialog's window so back gestures are handled while focused.
            if (Build.VERSION.SDK_INT >= 33) {
                try {
                    OnBackInvokedCallback callback = () ->
                            handleBackPress(activity);
                    window.getOnBackInvokedDispatcher().registerOnBackInvokedCallback(
                            OnBackInvokedDispatcher.PRIORITY_DEFAULT,
                            callback
                    );
                    backInvokedCallback = callback;
                } catch (Throwable e) {
                    Log.w(TAG, "Failed to register OnBackInvokedCallback on dialog: " + e.getMessage());
                }
            }
        }

        // Safety net: if the dialog is dismissed by the system (e.g. Activity finishing)
        // without the PaywallView dismiss handler firing, clean up static state so future
        // paywall presentations are not permanently blocked.
        dialog.setOnDismissListener(d -> {
            if (currentDialog == d) {
                String result = lastResult != null ? lastResult : RESULT_CANCELLED;
                dismissDialog();
                RevenueCatUI.sendPaywallResult(result);
            }
        });

        dialog.setContentView(paywallView, new FrameLayout.LayoutParams(
                FrameLayout.LayoutParams.MATCH_PARENT,
                FrameLayout.LayoutParams.MATCH_PARENT
        ));

        dialog.show();
    }

    private static void dismissDialog() {
        // Null currentDialog before calling dismiss() so the OnDismissListener
        // safety net sees currentDialog != d and does not re-enter.
        Dialog dialog = currentDialog;
        currentDialog = null;
        lastResult = null;
        if (Build.VERSION.SDK_INT >= 33 && backInvokedCallback != null && dialog != null) {
            try {
                OnBackInvokedCallback callback =
                        (OnBackInvokedCallback) backInvokedCallback;
                Window window = dialog.getWindow();
                if (window != null) {
                    window.getOnBackInvokedDispatcher()
                            .unregisterOnBackInvokedCallback(callback);
                }
            } catch (Throwable e) {
                Log.w(TAG, "Error unregistering OnBackInvokedCallback: " + e.getMessage());
            }
            backInvokedCallback = null;
        }
        if (dialog != null) {
            try {
                dialog.dismiss();
            } catch (Throwable e) {
                Log.w(TAG, "Error dismissing paywall dialog: " + e.getMessage());
            }
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

    /**
     * Toggles FLAG_NOT_FOCUSABLE on the dialog window.
     * When enabled, the dialog won't receive key/back events — needed during
     * PurchaseLogic operations for correct threading.
     * When disabled, the dialog is focusable and back navigation works normally.
     */
    private static void setDialogNotFocusable(boolean notFocusable) {
        Dialog dialog = currentDialog;
        if (dialog == null) return;
        Window window = dialog.getWindow();
        if (window == null) return;

        if (notFocusable) {
            window.addFlags(WindowManager.LayoutParams.FLAG_NOT_FOCUSABLE);
        } else {
            window.clearFlags(WindowManager.LayoutParams.FLAG_NOT_FOCUSABLE);
        }
    }

    private static void handleBackPress(Activity activity) {
        if (backPressedOwner != null
                && backPressedOwner.getOnBackPressedDispatcher().hasEnabledCallbacks()) {
            backPressedOwner.getOnBackPressedDispatcher().onBackPressed();
        } else {
            // Fallback: dismiss via the same path as the close button
            activity.runOnUiThread(() -> {
                String result = lastResult != null ? lastResult : RESULT_CANCELLED;
                dismissDialog();
                RevenueCatUI.sendPaywallResult(result);
            });
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
