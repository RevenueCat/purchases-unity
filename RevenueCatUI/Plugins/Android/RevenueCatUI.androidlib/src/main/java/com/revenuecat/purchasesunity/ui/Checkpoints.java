package com.revenuecat.purchasesunity.ui;

import android.os.Handler;
import android.os.Looper;
import android.util.Log;
import androidx.annotation.Keep;

import com.revenuecat.purchases.hybridcommon.ui.CheckpointHelpersKt;
import com.revenuecat.purchases.hybridcommon.ui.CheckpointListener;

import org.json.JSONObject;
import java.util.HashMap;
import java.util.Iterator;
import java.util.Map;

@Keep
public final class Checkpoints {
    private static final String TAG = "RevenueCatUI";
    private static final String NAVIGATED_BACK = "NAVIGATED_BACK";

    @Keep
    public interface Callbacks {
        void onPassed(String callId, String resultJson);
        void onError(String callId, String code, String message);
        void onPaywallPresentationRequested(String callId, String requestId, String paramsJson);
        void onErrorDialogRequested(String callId, String requestId, String errorJson);
    }

    public static void checkpoint(String identifier, String variablesJson, String presentationMode,
                                  String callId, boolean useCustomPaywallPresenter,
                                  boolean hasErrorDialogFactory, Callbacks callbacks) {
        runOnMain(() -> {
            try {
                CheckpointHelpersKt.presentCheckpoint(identifier, callId, toMap(variablesJson), presentationMode,
                        useCustomPaywallPresenter, hasErrorDialogFactory, new CheckpointListener() {
                    @Override
                    public void onCheckpointPassed(Map<String, ?> result) {
                        Map<String, Object> body = new HashMap<>(result);
                        if (body.get("flow") == null) body.put("flow", JSONObject.NULL);
                        callbacks.onPassed(callId, new JSONObject(body).toString());
                    }

                    @Override
                    public void onCheckpointError(String code, String message) {
                        callbacks.onError(callId, code, message);
                    }

                    @Override
                    public void onPaywallPresentationRequested(String requestId, Map<String, ?> params) {
                        try {
                            callbacks.onPaywallPresentationRequested(callId, requestId, new JSONObject(params).toString());
                        } catch (Exception e) {
                            Log.e(TAG, "Failed to request checkpoint paywall presentation", e);
                            CheckpointHelpersKt.completePaywallPresentation(requestId, NAVIGATED_BACK);
                        }
                    }

                    @Override
                    public void onErrorDialogRequested(String requestId, Map<String, ?> error) {
                        try {
                            callbacks.onErrorDialogRequested(callId, requestId, new JSONObject(error).toString());
                        } catch (Exception e) {
                            Log.e(TAG, "Failed to request checkpoint error dialog", e);
                            CheckpointHelpersKt.resolveErrorDialog(requestId, null);
                        }
                    }
                });
            } catch (Exception e) {
                Log.e(TAG, "Failed to call checkpoint", e);
                callbacks.onError(callId, "UNKNOWN", String.valueOf(e.getMessage()));
            }
        });
    }

    /** Reports how an app-owned checkpoint paywall ended: CONTINUED, CLOSED, or NAVIGATED_BACK. */
    public static void completePaywallPresentation(String requestId, String result) {
        if (requestId == null) return;
        runOnMain(() -> CheckpointHelpersKt.completePaywallPresentation(
                requestId, result == null ? NAVIGATED_BACK : result));
    }

    /** Resolves an error-dialog request. Missing or invalid outcomes use the SDK's default dialog. */
    public static void resolveErrorDialog(String requestId, String outcomeJson) {
        if (requestId == null) return;
        runOnMain(() -> {
            Map<String, Object> outcome = null;
            try {
                outcome = toMap(outcomeJson);
            } catch (Exception e) {
                Log.e(TAG, "Invalid checkpoint error dialog outcome; using the default dialog", e);
            }
            CheckpointHelpersKt.resolveErrorDialog(requestId, outcome);
        });
    }

    /** Stops delivering callbacks for a call. A paywall whose presenter already ran stays open until it completes. */
    public static void detachCheckpoint(String callId, String activePaywallRequestId) {
        if (callId == null) return;
        runOnMain(() -> CheckpointHelpersKt.detachCheckpoint(callId, activePaywallRequestId));
    }

    /** Backs out pending paywalls and defaults pending dialogs from a previous Unity runtime. */
    public static void invalidate() {
        runOnMain(CheckpointHelpersKt::invalidateCheckpointPresentations);
    }

    private static void runOnMain(Runnable runnable) {
        new Handler(Looper.getMainLooper()).post(runnable);
    }

    private static Map<String, Object> toMap(String json) throws Exception {
        if (json == null) return null;
        Map<String, Object> map = new HashMap<>();
        JSONObject raw = new JSONObject(json);
        Iterator<String> keys = raw.keys();
        while (keys.hasNext()) {
            String key = keys.next();
            Object value = raw.get(key);
            map.put(key, value instanceof JSONObject ? toMap(value.toString()) : value);
        }
        return map;
    }
}
