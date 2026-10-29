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

    @Keep
    public interface Callbacks {
        void onPassed(String callId, String resultJson);
        void onError(String callId, String code, String message);
    }

    public static void checkpoint(String identifier, String variablesJson, String presentationMode,
                                  String callId, Callbacks callbacks) {
        new Handler(Looper.getMainLooper()).post(() -> {
            try {
                Map<String, Object> variables = null;
                if (variablesJson != null) {
                    variables = new HashMap<>();
                    JSONObject raw = new JSONObject(variablesJson);
                    Iterator<String> keys = raw.keys();
                    while (keys.hasNext()) {
                        String key = keys.next();
                        variables.put(key, raw.get(key));
                    }
                }
                CheckpointHelpersKt.presentCheckpoint(identifier, variables, presentationMode, new CheckpointListener() {
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
                });
            } catch (Exception e) {
                Log.e(TAG, "Failed to call checkpoint", e);
                callbacks.onError(callId, "UNKNOWN", String.valueOf(e.getMessage()));
            }
        });
    }
}
