package com.revenuecat.purchasesunity;

import android.support.annotation.NonNull;
import android.support.annotation.Nullable;
import android.util.Log;

import com.android.billingclient.api.Purchase;
import com.android.billingclient.api.SkuDetails;
import com.revenuecat.purchases.*;
import com.revenuecat.purchases.interfaces.*;
import com.unity3d.player.UnityPlayer;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import java.util.ArrayList;
import java.util.Date;
import java.util.List;
import java.util.Map;
import java.text.NumberFormat;
import java.util.Currency;
import java.util.Locale;

public class PurchasesWrapper {
    private static final String RECEIVE_PRODUCTS = "_receiveProducts";
    private static final String GET_PURCHASER_INFO = "_getPurchaserInfo";
    private static final String MAKE_PURCHASE = "_makePurchase";
    private static final String CREATE_ALIAS = "_createAlias";
    private static final String RECEIVE_PURCHASER_INFO = "_receivePurchaserInfo";
    private static final String RESTORE_TRANSACTIONS = "_restoreTransactions";
    private static final String IDENTIFY = "_identify";
    private static final String RESET = "_reset";
    private static final String GET_ENTITLEMENTS = "_getEntitlements";

    private static String gameObject;
    private static UpdatedPurchaserInfoListener listener = new UpdatedPurchaserInfoListener() {
        @Override
        public void onReceived(@NonNull PurchaserInfo purchaserInfo) {
            sendPurchaserInfo(purchaserInfo, RECEIVE_PURCHASER_INFO);
        }
    };

    private static List<SkuDetails> productsCache = new ArrayList<>();

    public static void setup(String apiKey, String appUserId, String gameObject_, boolean observerMode) {
        gameObject = gameObject_;
        Purchases.configure(UnityPlayer.currentActivity, apiKey, appUserId, observerMode);
        Purchases.getSharedInstance().setUpdatedPurchaserInfoListener(listener);
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

            GetSkusResponseListener handler = new GetSkusResponseListener() {
                @Override
                public void onReceived(List<SkuDetails> skus) {
                    sendSkuDetails(skus);
                }

                @Override
                public void onError(@NonNull PurchasesError purchasesError) {
                    sendError(purchasesError, RECEIVE_PRODUCTS);
                }
            };

            Purchases purchases = Purchases.getSharedInstance();
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
    public static void makePurchase(final String productIdentifier, final String type, @Nullable final String oldSku) {
        if (UnityPlayer.currentActivity != null) {
            if (productsCache.isEmpty()) {
                Purchases.getSharedInstance().getEntitlements(new ReceiveEntitlementsListener() {
                    @Override
                    public void onReceived(Map<String, Entitlement> map) {
                        try {
                            mapEntitlementsAndCacheProducts(map);
                            startPurchase(productIdentifier, type, oldSku);
                        } catch (JSONException e) {
                            e.printStackTrace();
                        }
                    }

                    @Override
                    public void onError(PurchasesError purchasesError) {
                        sendErrorPurchase(purchasesError, false);
                    }
                });
            } else {
                startPurchase(productIdentifier, type, oldSku);
            }
        } else {
            sendErrorPurchase(
                    new PurchasesError(PurchasesErrorCode.PurchaseInvalidError, "There is no current Activity"), false);
        }
    }

    public static void makePurchase(String productIdentifier, String type) {
        makePurchase(productIdentifier, type, null);
    }

    @Nullable
    private static SkuDetails findProduct(String productIdentifier, String type) {
        for (SkuDetails product : productsCache) {
            if (product.getSku().equals(productIdentifier) && product.getType().equalsIgnoreCase(type)) {
                return product;
            }
        }
        return null;
    }

    private static void startPurchase(final String productIdentifier, final String type,
            @Nullable final String oldSku) {
        SkuDetails productToBuy = findProduct(productIdentifier, type);
        if (productToBuy != null) {
            MakePurchaseListener listener = new MakePurchaseListener() {
                @Override
                public void onCompleted(@NonNull Purchase purchase, @NonNull PurchaserInfo purchaserInfo) {
                    sendCompletedPurchase(purchase, purchaserInfo);
                }

                @Override
                public void onError(@NonNull PurchasesError purchasesError, Boolean userCancelled) {
                    sendErrorPurchase(purchasesError, userCancelled);
                }
            };
            if (oldSku == null || oldSku.isEmpty()) {
                Purchases.getSharedInstance().makePurchase(UnityPlayer.currentActivity, productToBuy, listener);
            } else {
                Purchases.getSharedInstance().makePurchase(UnityPlayer.currentActivity, productToBuy, oldSku, listener);
            }
        } else {
            sendErrorPurchase(new PurchasesError(PurchasesErrorCode.ProductNotAvailableForPurchaseError,
                    "Couldn't find product."), false);
        }
    }

    public static void addAttributionData(String dataJson, final int network, @Nullable String networkUserId) {
        JSONObject data;
        try {
            data = new JSONObject(dataJson);
        } catch (JSONException e) {
            logJSONException(e);
            return;
        }

        for (Purchases.AttributionNetwork attributionNetwork : Purchases.AttributionNetwork.values()) {
            if (attributionNetwork.getServerValue() == network) {
                Purchases.addAttributionData(data, attributionNetwork, networkUserId);
                break;
            }
        }
    }

    public static void restoreTransactions() {
        Purchases.getSharedInstance().restorePurchases(getPurchaserInfoListener(RESTORE_TRANSACTIONS));
    }

    public static void createAlias(String newAppUserID) {
        Purchases.getSharedInstance().createAlias(newAppUserID, getPurchaserInfoListener(CREATE_ALIAS));
    }

    public static void identify(String newAppUserID) {
        Purchases.getSharedInstance().identify(newAppUserID, getPurchaserInfoListener(IDENTIFY));
    }

    public static void reset() {
        Purchases.getSharedInstance().reset(getPurchaserInfoListener(RESET));
    }

    public static void setAllowSharingStoreAccount(boolean allowSharingStoreAccount) {
        Purchases.getSharedInstance().setAllowSharingPlayStoreAccount(allowSharingStoreAccount);
    }

    public static void getEntitlements() {
        Purchases.getSharedInstance().getEntitlements(new ReceiveEntitlementsListener() {
            @Override
            public void onReceived(@NonNull Map<String, Entitlement> entitlementMap) {
                sendEntitlements(entitlementMap);
            }

            @Override
            public void onError(@NonNull PurchasesError purchasesError) {
                sendError(purchasesError, GET_ENTITLEMENTS);
            }
        });
    }

    public static void setDebugLogsEnabled(boolean enabled) {
        Purchases.setDebugLogsEnabled(enabled);
    }

    public static String getAppUserID() {
        return Purchases.getSharedInstance().getAppUserID();
    }

    public static void getPurchaserInfo() {
        Purchases.getSharedInstance().getPurchaserInfo(getPurchaserInfoListener(GET_PURCHASER_INFO));
    }

    public static void setFinishTransactions(boolean enabled) {
        Purchases.getSharedInstance().setFinishTransactions(enabled);
    }

    public static void syncPurchases() {
        Purchases.getSharedInstance().syncPurchases();
    }

    private static void logJSONException(JSONException e) {
        Log.e("Purchases", "JSON Error: " + e.getLocalizedMessage());
    }

    private static JSONObject skuDetailsJSON(SkuDetails sku) throws JSONException {
        JSONObject skuObject = new JSONObject();
        skuObject.put("identifier", sku.getSku());
        skuObject.put("description", sku.getDescription());
        skuObject.put("price", sku.getPriceAmountMicros() / 1000000.0);
        skuObject.put("priceString", sku.getPrice());
        skuObject.put("title", sku.getTitle());
        skuObject.put("currencyCode", sku.getPriceCurrencyCode());
        if (sku.getFreeTrialPeriod().isEmpty()) {
            String introductoryPriceAmountMicros = sku.getIntroductoryPriceAmountMicros();
            if (introductoryPriceAmountMicros != null && !introductoryPriceAmountMicros.isEmpty()) {
                skuObject.put("introPrice", Long.parseLong(introductoryPriceAmountMicros) / 1000000d);
                skuObject.put("introPriceString", sku.getIntroductoryPrice());
                skuObject.put("introPricePeriod", sku.getIntroductoryPricePeriod());
                if (sku.getIntroductoryPricePeriod() != null && !sku.getIntroductoryPricePeriod().isEmpty()) {
                    PurchasesPeriod period = PurchasesPeriod.parse(sku.getIntroductoryPricePeriod());
                    if (period.years > 0) {
                        skuObject.put("introPricePeriodUnit", "YEAR");
                        skuObject.put("introPricePeriodNumberOfUnits", period.years);
                    } else if (period.months > 0) {
                        skuObject.put("introPricePeriodUnit", "MONTH");
                        skuObject.put("introPricePeriodNumberOfUnits", period.months);
                    } else if (period.days > 0) {
                        skuObject.put("introPricePeriodUnit", "DAY");
                        skuObject.put("introPricePeriodNumberOfUnits", period.days);
                    }
                }
                skuObject.put("introPriceCycles", sku.getIntroductoryPriceCycles());
            }
        } else {
            skuObject.put("introPrice", 0);
            // Format using device locale. iOS will format using App Store locale, but there's no way
            // to figure out how the price in the SKUDetails is being formatted.
            NumberFormat format = NumberFormat.getCurrencyInstance();
            format.setCurrency(Currency.getInstance(sku.getPriceCurrencyCode()));
            skuObject.put("introPriceString", format.format(0));
            skuObject.put("introPricePeriod", sku.getFreeTrialPeriod());
            PurchasesPeriod period = PurchasesPeriod.parse(sku.getFreeTrialPeriod());
            if (period.years > 0) {
                skuObject.put("introPricePeriodUnit", "YEAR");
                skuObject.put("introPricePeriodNumberOfUnits", period.years);
            } else if (period.months > 0) {
                skuObject.put("introPricePeriodUnit", "MONTH");
                skuObject.put("introPricePeriodNumberOfUnits", period.months);
            } else if (period.days > 0) {
                skuObject.put("introPricePeriodUnit", "DAY");
                skuObject.put("introPricePeriodNumberOfUnits", period.days);
            }
            skuObject.put("introPriceCycles", 1);
        }

        return skuObject;
    }

    private static JSONObject errorJSON(PurchasesError purchasesError) {
        JSONObject error = new JSONObject();
        try {
            error.put("code", purchasesError.getCode().ordinal());
            error.put("message", purchasesError.getMessage());
            if (purchasesError.getUnderlyingErrorMessage() != null) {
                error.put("underlyingErrorMessage", purchasesError.getUnderlyingErrorMessage());
            }
        } catch (JSONException e) {
            logJSONException(e);
        }
        return error;
    }

    private static JSONObject purchaserInfoJSON(PurchaserInfo info) {
        JSONObject jsonInfo = new JSONObject();
        try {
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
        } catch (JSONException e) {
            logJSONException(e);
        }
        return jsonInfo;
    }

    private static JSONArray mapEntitlementsAndCacheProducts(@NonNull Map<String, Entitlement> entitlementMap)
            throws JSONException {
        productsCache = new ArrayList<>();
        JSONArray entitlementsArray = new JSONArray();
        for (String entId : entitlementMap.keySet()) {
            JSONObject entitlementObject = new JSONObject();
            Entitlement ent = entitlementMap.get(entId);
            JSONArray offeringsArray = new JSONArray();
            if (ent != null) {
                Map<String, Offering> offerings = ent.getOfferings();
                for (String offeringId : offerings.keySet()) {
                    Offering offering = offerings.get(offeringId);
                    JSONObject offeringObject = new JSONObject();
                    if (offering != null) {
                        offeringObject.put("offeringId", offeringId);
                        SkuDetails skuDetails = offering.getSkuDetails();
                        if (skuDetails != null) {
                            productsCache.add(skuDetails);
                            JSONObject product = skuDetailsJSON(skuDetails);
                            offeringObject.put("product", product);
                        } else {
                            offeringObject.put("product", JSONObject.NULL);
                        }
                    }
                    offeringsArray.put(offeringObject);
                }
            }
            entitlementObject.put("entitlementId", entId);
            entitlementObject.put("offerings", offeringsArray);
            entitlementsArray.put(entitlementObject);
        }
        return entitlementsArray;
    }

    private static void sendJSONObject(JSONObject object, String method) {
        Log.e("Purchases", object.toString());
        UnityPlayer.UnitySendMessage(gameObject, method, object.toString());
    }

    private static void sendError(PurchasesError error, String method) {
        JSONObject jsonObject = new JSONObject();
        try {
            jsonObject.put("error", errorJSON(error));
        } catch (JSONException e) {
            logJSONException(e);
        }
        sendJSONObject(jsonObject, method);
    }

    private static void sendPurchaserInfo(PurchaserInfo purchaserInfo, String method) {
        JSONObject jsonObject = new JSONObject();
        try {
            jsonObject.put("purchaserInfo", purchaserInfoJSON(purchaserInfo));
        } catch (JSONException e) {
            logJSONException(e);
        }
        sendJSONObject(jsonObject, method);
    }

    private static void sendEntitlements(@NonNull Map<String, Entitlement> entitlementMap) {
        JSONObject jsonObject = new JSONObject();
        try {
            jsonObject.put("entitlements", mapEntitlementsAndCacheProducts(entitlementMap));
            sendJSONObject(jsonObject, GET_ENTITLEMENTS);
        } catch (JSONException e) {
            logJSONException(e);
        }
    }

    private static void sendSkuDetails(List<SkuDetails> skus) {
        JSONArray products = new JSONArray();
        for (SkuDetails sku : skus) {
            try {
                JSONObject skuObject = skuDetailsJSON(sku);
                products.put(skuObject);
            } catch (JSONException e) {
                logJSONException(e);
            }
        }

        try {
            JSONObject object = new JSONObject();
            object.put("products", products);
            sendJSONObject(object, RECEIVE_PRODUCTS);
        } catch (JSONException e) {
            logJSONException(e);
        }
    }

    private static void sendCompletedPurchase(Purchase purchase, PurchaserInfo purchaserInfo) {
        JSONObject jsonObject = new JSONObject();

        try {
            jsonObject.put("productIdentifier", purchase.getSku());
            jsonObject.put("purchaserInfo", purchaserInfoJSON(purchaserInfo));
            jsonObject.put("userCancelled", false);
        } catch (JSONException e) {
            logJSONException(e);
        }

        sendJSONObject(jsonObject, MAKE_PURCHASE);
    }

    private static void sendErrorPurchase(PurchasesError purchasesError, boolean userCancelled) {
        JSONObject jsonObject = new JSONObject();
        try {
            jsonObject.put("error", errorJSON(purchasesError));
            jsonObject.put("userCancelled", userCancelled);
        } catch (JSONException e) {
            logJSONException(e);
        }
        sendJSONObject(jsonObject, MAKE_PURCHASE);
    }

    @NonNull
    private static ReceivePurchaserInfoListener getPurchaserInfoListener(final String method) {
        return new ReceivePurchaserInfoListener() {
            @Override
            public void onReceived(@NonNull PurchaserInfo purchaserInfo) {
                sendPurchaserInfo(purchaserInfo, method);
            }

            @Override
            public void onError(@NonNull PurchasesError purchasesError) {
                sendError(purchasesError, method);
            }
        };
    }
}
