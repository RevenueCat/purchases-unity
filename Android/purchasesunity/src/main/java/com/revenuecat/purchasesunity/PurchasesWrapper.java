package com.revenuecat.purchasesunity;

import android.util.Log;

import com.android.billingclient.api.SkuDetails;
import com.revenuecat.purchases.PurchaserInfo;
import com.revenuecat.purchases.Purchases;
import com.unity3d.player.UnityPlayer;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import java.util.ArrayList;
import java.util.List;

public class PurchasesWrapper {
    private static Purchases purchases;
    private static String gameObject;

    private static Purchases.PurchasesListener listener = new Purchases.PurchasesListener() {
        @Override
        public void onCompletedPurchase(String sku, PurchaserInfo purchaserInfo) {

        }

        @Override
        public void onFailedPurchase(int domain, int code, String reason) {

        }

        @Override
        public void onReceiveUpdatedPurchaserInfo(PurchaserInfo purchaserInfo) {

        }

        @Override
        public void onRestoreTransactions(PurchaserInfo purchaserInfo) {

        }

        @Override
        public void onRestoreTransactionsFailed(int domain, int code, String reason) {

        }
    };

    public static void setup(String apiKey, String appUserId, String gameObject_) {
        purchases = new Purchases.Builder(UnityPlayer.currentActivity, apiKey, listener).appUserID(appUserId).build();
        gameObject = gameObject_;
    }

    public static void getProducts(String jsonProducts, String type) {
        try {
            JSONObject request = new JSONObject(jsonProducts);
            JSONArray products = request.getJSONArray("productIdentifiers");
            List<String> productIds = new ArrayList<>();
            for (int i = 0; i < products.length(); i++) {
                String product = products.getString(i);
                productIds.add(product);
            }

            Purchases.GetSkusResponseHandler handler = new Purchases.GetSkusResponseHandler() {
                @Override
                public void onReceiveSkus(List<SkuDetails> skus) {
                    sendSkuDetails(skus);
                }
            };

            if (type.equals("subs")) {
                purchases.getSubscriptionSkus(productIds, handler);
            } else {
                purchases.getNonSubscriptionSkus(productIds, handler);
            }

        } catch (JSONException e) {
            Log.e("Purchases", "Failure parsing product identifiers " + jsonProducts);
        }
    }

    public static void makePurchase(String productIdentifier, String type) {
        purchases.makePurchase(UnityPlayer.currentActivity, productIdentifier, type);
    }

    private static void sendSkuDetails(List<SkuDetails> skus) {
        JSONArray products = new JSONArray();
        for (SkuDetails sku : skus) {
            try {
                JSONObject skuObject = new JSONObject();
                skuObject.put("identifier", sku.getSku());
                skuObject.put("description", sku.getDescription());
                skuObject.put("price", sku.getPriceAmountMicros() / 1000.0);
                skuObject.put("priceString", sku.getPrice());
                products.put(skuObject);
            } catch (JSONException e) {}
        }

        try {
            JSONObject object = new JSONObject();
            object.put("products", products);
            sendJSONObject(object, "_receiveProducts");
        } catch (JSONException e) {}
    }

    private static void sendJSONObject(JSONObject object, String method) {
        UnityPlayer.UnitySendMessage(gameObject, method, object.toString());
    }
}
