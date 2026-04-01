package com.revenuecat.purchasesunity.ui;

import android.app.Activity;
import android.app.Dialog;
import android.content.DialogInterface;
import android.graphics.Color;
import android.os.Build;
import android.os.Handler;
import android.os.Looper;
import android.util.Log;
import android.view.KeyEvent;
import android.view.ViewGroup;
import android.view.Window;
import android.view.WindowManager;
import android.widget.FrameLayout;
import android.window.OnBackInvokedDispatcher;

import androidx.activity.OnBackPressedDispatcher;
import androidx.activity.OnBackPressedDispatcherOwner;
import androidx.activity.ViewTreeOnBackPressedDispatcherOwner;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.core.graphics.Insets;
import androidx.core.view.ViewCompat;
import androidx.core.view.WindowInsetsCompat;
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

import java.util.HashMap;
import java.util.Iterator;
import java.util.Map;

import com.revenuecat.purchases.ui.revenuecatui.CustomVariableValue;

import kotlin.Unit;

public class PaywallViewPresenter {

    private static final String TAG = "PurchasesUnity";

    private static final String RESULT_PURCHASED = "PURCHASED";
    private static final String RESULT_RESTORED = "RESTORED";
    private static final String RESULT_CANCELLED = "CANCELLED";
    private static final String RESULT_ERROR = "ERROR";
    private static final String RESULT_NOT_PRESENTED = "NOT_PRESENTED";

    static final String OPERATION_TYPE_PURCHASE = "PURCHASE";
    static final String OPERATION_TYPE_RESTORE = "RESTORE";

    // These fields must only be accessed on the UI thread. All compound check-then-act
    // operations (e.g. if (currentDialog != null)) are safe because the UI thread is
    // single-threaded. The volatile modifier provides cross-thread visibility but does
    // not substitute for synchronization — do not read or write these off the UI thread.
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
            @Nullable String customVariablesJson,
            boolean hasPurchaseLogic
    ) {
        if (activity == null) {
            Log.e(TAG, "Activity is null; cannot present paywall");
            RevenueCatUI.sendPaywallResult(RESULT_ERROR);
            return;
        }

        activity.runOnUiThread(() ->
                showPaywallView(activity, offeringIdentifier, presentedOfferingContextJson, displayCloseButton, customVariablesJson, hasPurchaseLogic)
        );
    }

    public static void presentPaywallIfNeeded(
            Activity activity,
            @NonNull String requiredEntitlementIdentifier,
            @Nullable String offeringIdentifier,
            @Nullable String presentedOfferingContextJson,
            boolean displayCloseButton,
            @Nullable String customVariablesJson,
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
                            showPaywallView(activity, offeringIdentifier, presentedOfferingContextJson, displayCloseButton, customVariablesJson, hasPurchaseLogic)
                    );
                }
            }

            @Override
            public void onError(@NonNull PurchasesError error) {
                Log.e(TAG, "Error fetching customer info to display paywall: " + error.getMessage());
                RevenueCatUI.sendPaywallResult(RESULT_NOT_PRESENTED);
            }
        });
    }

    // region Dialog setup

    @SuppressWarnings("unchecked")
    private static void showPaywallView(
            Activity activity,
            @Nullable String offeringIdentifier,
            @Nullable String presentedOfferingContextJson,
            boolean displayCloseButton,
            @Nullable String customVariablesJson,
            boolean hasPurchaseLogic
    ) {
        if (currentDialog != null) {
            Log.w(TAG, "Paywall is already being presented");
            RevenueCatUI.sendPaywallResult(RESULT_ERROR);
            return;
        }

        lastResult = RESULT_CANCELLED;

        Dialog dialog = createDialog(activity);
        currentDialog = dialog;

        PaywallView paywallView = createPaywallView(
                activity, offeringIdentifier, presentedOfferingContextJson, displayCloseButton, customVariablesJson, hasPurchaseLogic
        );

        setupBackPressedOwner(dialog.getWindow());
        setupBackPressRouting(dialog);
        setupDismissListener(dialog);

        FrameLayout container = createEdgeToEdgeContainer(activity, paywallView);
        dialog.setContentView(container, new FrameLayout.LayoutParams(
                FrameLayout.LayoutParams.MATCH_PARENT,
                FrameLayout.LayoutParams.MATCH_PARENT
        ));

        dialog.show();
        applyEdgeToEdgeFlags(dialog.getWindow());
    }

    /**
     * Creates a hardware-accelerated, fullscreen Dialog.
     *
     * A Dialog (rather than a View added directly to the Activity) is used because Unity's
     * main window may use software rendering. Compose + Coil use hardware bitmaps by default,
     * which crash on a software canvas. A Dialog creates a separate, hardware-accelerated window.
     */
    private static Dialog createDialog(Activity activity) {
        Dialog dialog = new Dialog(activity, android.R.style.Theme_Light_NoTitleBar);

        Window window = dialog.getWindow();
        if (window != null) {
            window.addFlags(WindowManager.LayoutParams.FLAG_HARDWARE_ACCELERATED);

            // NOTE: FLAG_NOT_FOCUSABLE is NOT set here so the Dialog can receive
            // back navigation events (key events on pre-API 33, gesture back on
            // API 33+). For PurchaseLogic, FLAG_NOT_FOCUSABLE is toggled on/off
            // only during active purchase/restore operations to fix a threading
            // issue with PurchasesAreCompletedBy.MyApp.

            window.setLayout(
                    WindowManager.LayoutParams.MATCH_PARENT,
                    WindowManager.LayoutParams.MATCH_PARENT
            );
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.P) {
                window.getAttributes().layoutInDisplayCutoutMode =
                        WindowManager.LayoutParams.LAYOUT_IN_DISPLAY_CUTOUT_MODE_SHORT_EDGES;
            }
        }

        // Prevent the Dialog from dismissing itself on back press. Back events are
        // routed to our OnBackPressedDispatcher instead (see setupBackPressRouting).
        dialog.setCancelable(false);

        return dialog;
    }

    // endregion

    // region PaywallView setup

    private static PaywallView createPaywallView(
            Activity activity,
            @Nullable String offeringIdentifier,
            @Nullable String presentedOfferingContextJson,
            boolean displayCloseButton,
            @Nullable String customVariablesJson,
            boolean hasPurchaseLogic
    ) {
        PaywallView paywallView = new PaywallView(activity);

        if (offeringIdentifier != null) {
            PresentedOfferingContext presentedOfferingContext =
                    mapPresentedOfferingContext(presentedOfferingContextJson, offeringIdentifier);
            paywallView.setOfferingId(offeringIdentifier, presentedOfferingContext);
        }

        Map<String, CustomVariableValue> customVariables = parseCustomVariables(customVariablesJson);
        if (customVariables != null) {
            paywallView.setCustomVariables(customVariables);
        }

        paywallView.setDisplayDismissButton(displayCloseButton);
        paywallView.setPaywallListener(createPaywallListener());
        paywallView.setDismissHandler(() -> {
            activity.runOnUiThread(() -> {
                if (currentDialog != null) {
                    String result = lastResult;
                    dismissDialog();
                    RevenueCatUI.sendPaywallResult(result);
                }
            });
            return Unit.INSTANCE;
        });

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

        return paywallView;
    }

    private static PaywallListener createPaywallListener() {
        return new PaywallListener() {
            @Override
            public void onRestoreError(@NonNull PurchasesError purchasesError) {
                setDialogNotFocusable(false);
            }

            @Override
            public void onRestoreStarted() {}

            @Override
            public void onPurchaseError(@NonNull PurchasesError purchasesError) {
                lastResult = RESULT_ERROR;
                setDialogNotFocusable(false);
            }

            @Override
            public void onPurchaseStarted(@NonNull Package aPackage) {}

            @Override
            public void onPurchaseCancelled() {
                setDialogNotFocusable(false);
            }

            @Override
            public void onPurchasePackageInitiated(@NonNull Package aPackage, @NonNull Resumable resumable) {
                resumable.invoke(true);
            }

            @Override
            public void onRestoreInitiated(@NonNull Resumable resume) {
                resume.invoke(true);
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
        };
    }

    // endregion

    // region Edge-to-edge

    /**
     * Wraps the PaywallView in a container that provides corrected window insets.
     *
     * FLAG_LAYOUT_NO_LIMITS (applied later) is needed to draw the paywall behind the
     * status bar and navigation bar, but it causes the system to report zero insets.
     * This container intercepts the zero insets and replaces them with the real system
     * bar values so the PaywallView's Compose content can pad itself appropriately.
     */
    private static FrameLayout createEdgeToEdgeContainer(Activity activity, PaywallView paywallView) {
        final Insets statusBarInsets = getActivityInsets(activity, WindowInsetsCompat.Type.statusBars());
        final Insets navBarInsets = getActivityInsets(activity, WindowInsetsCompat.Type.navigationBars());

        FrameLayout container = new FrameLayout(activity);
        container.addView(paywallView, new FrameLayout.LayoutParams(
                FrameLayout.LayoutParams.MATCH_PARENT,
                FrameLayout.LayoutParams.MATCH_PARENT
        ));

        ViewCompat.setOnApplyWindowInsetsListener(container, (v, insets) -> {
            WindowInsetsCompat corrected = new WindowInsetsCompat.Builder(insets)
                    .setInsets(WindowInsetsCompat.Type.statusBars(), statusBarInsets)
                    .setInsets(WindowInsetsCompat.Type.navigationBars(), navBarInsets)
                    .build();
            ViewCompat.dispatchApplyWindowInsets(paywallView, corrected);
            return corrected;
        });

        return container;
    }

    /**
     * Reads the real insets of the given type from the activity's window.
     * Falls back to the status_bar_height system resource for status bar insets
     * if the activity's insets are not yet available.
     */
    private static Insets getActivityInsets(Activity activity, int insetsType) {
        WindowInsetsCompat activityInsets = ViewCompat.getRootWindowInsets(
                activity.getWindow().getDecorView());
        if (activityInsets != null) {
            return activityInsets.getInsets(insetsType);
        }
        if (insetsType == WindowInsetsCompat.Type.statusBars()) {
            int sbHeight = getSystemBarHeight(activity, "status_bar_height");
            return Insets.of(0, sbHeight, 0, 0);
        }
        return Insets.NONE;
    }

    /**
     * Applies edge-to-edge window flags after the dialog is shown.
     * Must be called after {@link Dialog#show()} so the decor view is attached.
     */
    private static void applyEdgeToEdgeFlags(@Nullable Window window) {
        if (window == null) return;
        window.setStatusBarColor(Color.TRANSPARENT);
        window.setNavigationBarColor(Color.TRANSPARENT);
        window.addFlags(WindowManager.LayoutParams.FLAG_LAYOUT_NO_LIMITS);
    }

    private static int getSystemBarHeight(Activity activity, String resourceName) {
        int resourceId = activity.getResources().getIdentifier(resourceName, "dimen", "android");
        if (resourceId > 0) {
            return activity.getResources().getDimensionPixelSize(resourceId);
        }
        return 0;
    }

    // endregion

    // region Back press and dismiss

    /**
     * Installs an OnBackPressedDispatcherOwner on the dialog's decor view so Compose's
     * BackHandler composable can find it in the view tree. Unity's Activity does not
     * extend ComponentActivity, so this owner is not available by default.
     */
    private static void setupBackPressedOwner(@Nullable Window window) {
        if (window == null) return;
        ViewGroup decorView = (ViewGroup) window.getDecorView();
        if (ViewTreeOnBackPressedDispatcherOwner.get(decorView) == null) {
            backPressedOwner = new PaywallBackPressedOwner();
            ViewTreeOnBackPressedDispatcherOwner.set(decorView, backPressedOwner);
        }
    }

    /**
     * Routes the Dialog's back events to our {@link PaywallBackPressedOwner}'s dispatcher.
     *
     * The Dialog has {@code setCancelable(false)} so it won't dismiss itself on back press.
     * However, nothing automatically connects the system back event to our custom
     * OnBackPressedDispatcher. This method bridges that gap:
     * <ul>
     *   <li>API 33+: Registers an {@code OnBackInvokedCallback} for predictive back gestures.</li>
     *   <li>Pre-33: Uses an {@code OnKeyListener} to intercept KEYCODE_BACK.</li>
     * </ul>
     */
    private static void setupBackPressRouting(Dialog dialog) {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.TIRAMISU) {
            // API 33+: back gestures go through OnBackInvokedDispatcher, not KEYCODE_BACK.
            dialog.getOnBackInvokedDispatcher().registerOnBackInvokedCallback(
                    OnBackInvokedDispatcher.PRIORITY_DEFAULT,
                    () -> {
                        if (backPressedOwner != null) {
                            backPressedOwner.getOnBackPressedDispatcher().onBackPressed();
                        }
                    }
            );
        } else {
            // Pre-33: back presses arrive as KEYCODE_BACK key events.
            dialog.setOnKeyListener((DialogInterface d, int keyCode, KeyEvent event) -> {
                if (keyCode == KeyEvent.KEYCODE_BACK && event.getAction() == KeyEvent.ACTION_UP) {
                    if (backPressedOwner != null && backPressedOwner.getOnBackPressedDispatcher().hasEnabledCallbacks()) {
                        backPressedOwner.getOnBackPressedDispatcher().onBackPressed();
                        return true;
                    }
                }
                return false;
            });
        }
    }

    /**
     * Safety net: if the dialog is dismissed by the system (e.g. Activity finishing)
     * without the PaywallView dismiss handler firing, clean up static state so future
     * paywall presentations are not permanently blocked.
     */
    private static void setupDismissListener(Dialog dialog) {
        dialog.setOnDismissListener(d -> {
            if (currentDialog == d) {
                String result = lastResult != null ? lastResult : RESULT_CANCELLED;
                dismissDialog();
                RevenueCatUI.sendPaywallResult(result);
            }
        });
    }

    private static void dismissDialog() {
        // Null currentDialog before calling dismiss() so the OnDismissListener
        // safety net sees currentDialog != d and does not re-enter.
        Dialog dialog = currentDialog;
        currentDialog = null;
        lastResult = null;
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
     * Called from C# after a custom PurchaseLogic operation (purchase or restore) completes.
     * Updates lastResult based on the operation type and result, and clears FLAG_NOT_FOCUSABLE.
     * Must be called before HybridPurchaseLogicBridge.resolveResult so lastResult is set
     * before the ViewModel potentially dismisses the paywall.
     */
    public static void onPurchaseLogicResult(String operationType, String resultString) {
        if ("SUCCESS".equals(resultString)) {
            if (OPERATION_TYPE_RESTORE.equals(operationType)) {
                lastResult = RESULT_RESTORED;
            } else if (OPERATION_TYPE_PURCHASE.equals(operationType)) {
                lastResult = RESULT_PURCHASED;
            }
        }
        // setDialogNotFocusable touches the dialog window and must run on the Android UI thread.
        // This method is called from the Unity main thread, so post to the main looper.
        new Handler(Looper.getMainLooper()).post(() -> setDialogNotFocusable(false));
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

    // endregion

    // region Custom variables

    @Nullable
    private static Map<String, CustomVariableValue> parseCustomVariables(@Nullable String jsonString) {
        if (jsonString == null || jsonString.isEmpty()) {
            return null;
        }
        try {
            JSONObject json = new JSONObject(jsonString);
            Map<String, CustomVariableValue> result = new HashMap<>();
            Iterator<String> keys = json.keys();
            while (keys.hasNext()) {
                String key = keys.next();
                Object value = json.opt(key);
                if (value == null || value == JSONObject.NULL) {
                    continue;
                }
                if (value instanceof Boolean) {
                    result.put(key, new CustomVariableValue.Boolean((boolean) value));
                } else if (value instanceof Number) {
                    result.put(key, new CustomVariableValue.Number(((Number) value).doubleValue()));
                } else {
                    result.put(key, new CustomVariableValue.String(value.toString()));
                }
            }
            return result.isEmpty() ? null : result;
        } catch (JSONException e) {
            Log.w(TAG, "Failed to parse custom variables JSON: " + jsonString, e);
            return null;
        }
    }

    // endregion

    // region PresentedOfferingContext mapping

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

    // endregion
}
