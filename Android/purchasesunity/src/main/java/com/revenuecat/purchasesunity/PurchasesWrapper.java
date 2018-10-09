package com.revenuecat.purchasesunity;

import android.content.Context;
import android.util.Log;

import com.android.billingclient.api.SkuDetails;
import com.revenuecat.purchases.PurchaserInfo;
import com.revenuecat.purchases.Purchases;
import com.revenuecat.purchases.util.Iso8601Utils;
import com.unity3d.player.UnityPlayer;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import java.util.ArrayList;
import java.util.Date;
import java.util.HashMap;
import java.util.Iterator;
import java.util.List;
import java.util.Map;
import java.util.Set;

import static com.revenuecat.purchases.Purchases.AttributionNetwork.ADJUST;

public class PurchasesWrapper {
    private static Purchases purchases;
    private static String gameObject;

    private static Purchases.PurchasesListener listener = new Purchases.PurchasesListener() {
        @Override
        public void onCompletedPurchase(String sku, PurchaserInfo purchaserInfo) {
            sendPurchaserInfo(purchaserInfo, sku, null, false);
        }

        @Override
        public void onFailedPurchase(int domain, int code, String reason) {
            sendPurchaserInfo(null, null, errorJSON(domain, code, reason), false);
        }

        @Override
        public void onReceiveUpdatedPurchaserInfo(PurchaserInfo purchaserInfo) {
            sendPurchaserInfo(purchaserInfo, null, null, false);
        }

        @Override
        public void onRestoreTransactions(PurchaserInfo purchaserInfo) {
            sendPurchaserInfo(purchaserInfo, null, null, false);
        }

        @Override
        public void onRestoreTransactionsFailed(int domain, int code, String reason) {
            sendPurchaserInfo(null, null, errorJSON(domain, code, reason), false);
        }
    };

    public static void setup(String apiKey, String appUserId, String gameObject_) {
        gameObject = gameObject_;

        if (purchases != null) {
            purchases.close();
        }
        
        purchases = new Purchases.Builder(UnityPlayer.currentActivity, apiKey, listener).appUserID(appUserId).build();
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

    // makePurchase to upgrade/downgrade current subscriptions
    public static void makePurchase(String productIdentifier, String type, String oldSku) {
        ArrayList<String> oldSkuList = new ArrayList<>();
        oldSkuList.add(oldSku);
        purchases.makePurchase(UnityPlayer.currentActivity, productIdentifier, type, oldSkuList);
    }

    public static void makePurchase(String productIdentifier, String type) {
        purchases.makePurchase(UnityPlayer.currentActivity, productIdentifier, type);
    }

    public static void addAttributionData(String dataJson, String network) {
        JSONObject data;
        try {
            data = new JSONObject(dataJson);
        } catch (JSONException e) {
            logJSONException(e);
            return;
        }

        final JSONObject finalData = data;

        if (network.equals("adjust")) {
            new Thread(new Runnable() {
                public void run() {
                    try {
                        Context context = UnityPlayer.currentActivity;
                        AdvertisingIdClient.AdInfo adInfo = AdvertisingIdClient.getAdvertisingIdInfo(context);

                        String advertisingId = adInfo.getId();
                        Boolean trackingLimited = adInfo.isLimitAdTrackingEnabled();

                        if (!trackingLimited) {
                            finalData.put("rc_gps_adid", advertisingId);
                        }

                    } catch (Exception e) {
                        Log.e("Purchases", e.getLocalizedMessage());
                        e.printStackTrace();
                    }
                    
                    purchases.addAttributionData(finalData, ADJUST);
                }
            }).start();
        } else {
            Log.e("Purchases", "Network " + network + " not supported");
        }

    }

    public static void restoreTransactions() {
        purchases.restorePurchasesForPlayStoreAccount();
    }

    private static void logJSONException(JSONException e) {
        Log.e("Purchases", "JSON Error: " + e.getLocalizedMessage());
    }

    private static void sendSkuDetails(List<SkuDetails> skus) {
        JSONArray products = new JSONArray();
        for (SkuDetails sku : skus) {
            try {
                JSONObject skuObject = new JSONObject();
                skuObject.put("identifier", sku.getSku());
                skuObject.put("description", sku.getDescription());
                skuObject.put("price", sku.getPriceAmountMicros() / 1000000.0);
                skuObject.put("priceString", sku.getPrice());
                skuObject.put("title", sku.getTitle());
                products.put(skuObject);
            } catch (JSONException e) {
                logJSONException(e);
            }
        }

        try {
            JSONObject object = new JSONObject();
            object.put("products", products);
            sendJSONObject(object, "_receiveProducts");
        } catch (JSONException e) {
            logJSONException(e);
        }
    }

    private static JSONObject errorJSON(int domain, int code, String reason) {
        JSONObject error = new JSONObject();
        try {
            error.put("domain", domain);
            error.put("code", code);
            error.put("message", reason);
        } catch (JSONException e) {
            logJSONException(e);
        }
        return error;
    }

    private static JSONObject purchaserInfoJSON(PurchaserInfo info) throws JSONException {
        JSONObject jsonInfo = new JSONObject();

        JSONArray activeSubs = new JSONArray();
        for (String active : info.getActiveSubscriptions()) {
            activeSubs.put(active);
        }

        jsonInfo.put("activeSubscriptions", activeSubs);

        JSONArray allPurchasedProductIdentifiers = new JSONArray();
        for (String productId : info.getAllPurchasedSkus()) {
            allPurchasedProductIdentifiers.put(productId);
        }
        jsonInfo.put("allPurchasedProductIdentifiers", allPurchasedProductIdentifiers);

        Date latest = info.getLatestExpirationDate();
        if (latest != null) {
            jsonInfo.put("latestExpirationDate", info.getLatestExpirationDate().getTime() / 1000.0);
        }

        JSONArray expirationDateKeys = new JSONArray();
        JSONArray expirationDateValues = new JSONArray();

        Map<String, Date> allExpDates = info.getAllExpirationDatesByProduct();
        for (String sku : allExpDates.keySet()) {
            expirationDateKeys.put(sku);
            expirationDateValues.put(allExpDates.get(sku).getTime() / 1000.0);
        }

        jsonInfo.put("allExpirationDateKeys", expirationDateKeys);
        jsonInfo.put("allExpirationDateValues", expirationDateValues);

        return jsonInfo;
    }

    private static void sendPurchaserInfo(PurchaserInfo info, String completedTransaction, JSONObject error, Boolean isRestore) {
        JSONObject message = new JSONObject();

        try {
            if (info != null) {
                message.put("purchaserInfo", purchaserInfoJSON(info));
            }

            if (completedTransaction != null) {
                message.put("productIdentifier", completedTransaction);
            }

            if (error != null) {
                message.put("error", error);
            }

            message.put("isRestore", isRestore);

            sendJSONObject(message, "_receivePurchaserInfo");

        } catch (JSONException e) {
            Log.e("Purchases", "Error sending message");
        }
    }

    private static void sendJSONObject(JSONObject object, String method) {
        Log.e("Purchases", object.toString());
        UnityPlayer.UnitySendMessage(gameObject, method, object.toString());
    }
}
