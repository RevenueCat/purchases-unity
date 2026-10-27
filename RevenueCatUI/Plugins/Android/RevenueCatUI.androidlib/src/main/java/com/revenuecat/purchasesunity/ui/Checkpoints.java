package com.revenuecat.purchasesunity.ui;

import android.os.Handler;
import android.os.Looper;
import android.util.Log;
import androidx.annotation.Keep;

import com.revenuecat.purchases.Purchases;
import com.revenuecat.purchases.hybridcommon.mappers.CustomerInfoMapperKt;
import com.revenuecat.purchases.hybridcommon.mappers.EntitlementInfoMapperKt;
import com.revenuecat.purchases.hybridcommon.mappers.StoreProductMapperKt;
import com.revenuecat.purchases.ui.revenuecatui.CustomVariableValue;
import com.revenuecat.purchases.ui.revenuecatui.checkpoints.CheckpointParams;
import com.revenuecat.purchases.ui.revenuecatui.checkpoints.CheckpointsExtensionsKt;
import com.revenuecat.purchases.ui.revenuecatui.checkpoints.FlowPresentationMode;
import com.revenuecat.purchases.ui.revenuecatui.checkpoints.ObtainedEntitlement;
import com.revenuecat.purchases.ui.revenuecatui.checkpoints.ObtainedProduct;

import org.json.JSONObject;
import java.util.ArrayList;
import java.util.Collections;
import java.util.HashMap;
import java.util.Iterator;
import java.util.List;
import java.util.Map;

@Keep
public final class Checkpoints {
    @Keep
    public interface Callbacks {
        void onPassed(String callId, String resultJson);
    }

    public static void checkpoint(String identifier, String variablesJson, String presentationMode,
                                  String callId, Callbacks callbacks) {
        if (!Purchases.isConfigured()) {
            throw new IllegalStateException("Configure Purchases before calling a checkpoint.");
        }
        new Handler(Looper.getMainLooper()).post(() -> {
            try {
                Map<String, CustomVariableValue> variables = new HashMap<>();
                if (variablesJson != null) {
                    JSONObject raw = new JSONObject(variablesJson);
                    Iterator<String> keys = raw.keys();
                    while (keys.hasNext()) {
                        String key = keys.next();
                        Object value = raw.get(key);
                        if (value instanceof String) variables.put(key, new CustomVariableValue.String((String) value));
                        else if (value instanceof Boolean) variables.put(key, new CustomVariableValue.Boolean((Boolean) value));
                        else if (value instanceof Number) variables.put(key, new CustomVariableValue.Number(((Number) value).doubleValue()));
                    }
                }
                FlowPresentationMode mode = FlowPresentationMode.DEFAULT;
                if ("MODAL_FULL_SCREEN".equals(presentationMode)) mode = FlowPresentationMode.MODAL_FULL_SCREEN;
                else if ("MODAL_SHEET".equals(presentationMode)) mode = FlowPresentationMode.MODAL_SHEET;
                CheckpointParams params = new CheckpointParams.Builder()
                    .setCustomVariables(variables).setPresentationMode(mode).build();
                CheckpointsExtensionsKt.checkpoint(Purchases.getSharedInstance(), identifier, params, result -> {
                    Map<String, Object> body = new HashMap<>();
                    body.put("customer", CustomerInfoMapperKt.map(result.getCustomer()));
                    Map<String, Object> flow = null;
                    if (result.getFlow() != null) {
                        flow = new HashMap<>();
                        List<Map<String, Object>> entitlements = new ArrayList<>();
                        for (ObtainedEntitlement value : result.getFlow().getObtainedEntitlements()) {
                            entitlements.add(Collections.singletonMap("entitlementInfo", EntitlementInfoMapperKt.map(value.getEntitlementInfo())));
                        }
                        List<Map<String, Object>> products = new ArrayList<>();
                        for (ObtainedProduct value : result.getFlow().getObtainedProducts()) {
                            products.add(Collections.singletonMap("product", StoreProductMapperKt.map(value.getProduct())));
                        }
                        flow.put("obtainedEntitlements", entitlements);
                        flow.put("obtainedProducts", products);
                    }
                    body.put("flow", flow == null ? JSONObject.NULL : flow);
                    callbacks.onPassed(callId, new JSONObject(body).toString());
                });
            } catch (Exception e) {
                Log.e("RevenueCatUI", "Failed to call checkpoint", e);
            }
        });
    }
}
