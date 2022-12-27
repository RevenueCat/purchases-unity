package com.revenuecat.purchasesunity;

import android.util.Log;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

import com.revenuecat.purchases.CustomerInfo;
import com.revenuecat.purchases.DangerousSettings;
import com.revenuecat.purchases.Purchases;
import com.revenuecat.purchases.Store;
import com.revenuecat.purchases.common.PlatformInfo;
import com.revenuecat.purchases.hybridcommon.CommonKt;
import com.revenuecat.purchases.hybridcommon.ErrorContainer;
import com.revenuecat.purchases.hybridcommon.OnResult;
import com.revenuecat.purchases.hybridcommon.OnResultAny;
import com.revenuecat.purchases.hybridcommon.OnResultList;
import com.revenuecat.purchases.hybridcommon.SubscriberAttributesKt;
import com.revenuecat.purchases.hybridcommon.mappers.CustomerInfoMapperKt;
import com.revenuecat.purchases.hybridcommon.mappers.MappersHelpersKt;
import com.revenuecat.purchases.interfaces.UpdatedCustomerInfoListener;
import com.unity3d.player.UnityPlayer;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import java.util.ArrayList;
import java.util.List;
import java.util.Map;

public class PurchasesWrapper {
    private static final String RECEIVE_PRODUCTS = "_receiveProducts";
    private static final String GET_CUSTOMER_INFO = "_getCustomerInfo";
    private static final String MAKE_PURCHASE = "_makePurchase";
    private static final String RECEIVE_CUSTOMER_INFO = "_receiveCustomerInfo";
    private static final String RESTORE_PURCHASES = "_restorePurchases";
    private static final String LOG_IN = "_logIn";
    private static final String LOG_OUT = "_logOut";
    private static final String GET_OFFERINGS = "_getOfferings";
    private static final String CHECK_ELIGIBILITY = "_checkTrialOrIntroductoryPriceEligibility";
    private static final String CAN_MAKE_PAYMENTS = "_canMakePayments";
    private static final String GET_PROMOTIONAL_OFFER = "_getPromotionalOffer";

    private static final String PLATFORM_NAME = "unity";
    private static final String PLUGIN_VERSION = "4.6.4";

    private static String gameObject;
    private static UpdatedCustomerInfoListener listener = new UpdatedCustomerInfoListener() {
        @Override
        public void onReceived(@NonNull CustomerInfo customerInfo) {
            sendCustomerInfo(CustomerInfoMapperKt.map(customerInfo), RECEIVE_CUSTOMER_INFO);
        }
    };

    public static void setup(String apiKey,
                             String appUserId,
                             String gameObject_,
                             boolean observerMode,
                             String userDefaultsSuiteName,
                             boolean useAmazon,
                             String dangerousSettingsJSON) {
        gameObject = gameObject_;
        PlatformInfo platformInfo = new PlatformInfo(PLATFORM_NAME, PLUGIN_VERSION);
        Store store = useAmazon ? Store.AMAZON : Store.PLAY_STORE;
        DangerousSettings dangerousSettings = getDangerousSettingsFromJSON(dangerousSettingsJSON);
        CommonKt.configure(UnityPlayer.currentActivity,
                apiKey, appUserId, observerMode, platformInfo, store, dangerousSettings);
        Purchases.getSharedInstance().setUpdatedCustomerInfoListener(listener);
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

            CommonKt.getProductInfo(productIds, type, new OnResultList() {
                @Override
                public void onReceived(List<Map<String, ?>> map) {
                    try {
                        JSONObject object = new JSONObject();
                        object.put("products", MappersHelpersKt.convertToJsonArray(map));
                        sendJSONObject(object, RECEIVE_PRODUCTS);
                    } catch (JSONException e) {
                        logJSONException(e);
                    }
                }

                @Override
                public void onError(ErrorContainer errorContainer) {
                    sendError(errorContainer, RECEIVE_PRODUCTS);
                }
            });
        } catch (JSONException e) {
            Log.e("Purchases", "Failure parsing product identifiers " + jsonProducts);
        }
    }

    public static void purchaseProduct(final String productIdentifier,
                                       final String type,
                                       @Nullable final String oldSKU,
                                       final int prorationMode) {
        CommonKt.purchaseProduct(
                UnityPlayer.currentActivity,
                productIdentifier,
                oldSKU,
                prorationMode,
                type,
                new OnResult() {
                    @Override
                    public void onReceived(Map<String, ?> map) {
                        sendJSONObject(MappersHelpersKt.convertToJson(map), MAKE_PURCHASE);
                    }

                    @Override
                    public void onError(ErrorContainer errorContainer) {
                        sendErrorPurchase(errorContainer);
                    }
                });
    }

    public static void purchaseProduct(String productIdentifier, String type) {
        purchaseProduct(productIdentifier, type, null,  0);
    }

    public static void purchasePackage(String packageIdentifier,
                                       String offeringIdentifier,
                                       @Nullable final String oldSKU,
                                       final int prorationMode) {
        CommonKt.purchasePackage(
                UnityPlayer.currentActivity,
                packageIdentifier,
                offeringIdentifier,
                oldSKU,
                prorationMode,
                new OnResult() {
                    @Override
                    public void onReceived(Map<String, ?> map) {
                        sendJSONObject(MappersHelpersKt.convertToJson(map), MAKE_PURCHASE);
                    }

                    @Override
                    public void onError(ErrorContainer errorContainer) {
                        sendErrorPurchase(errorContainer);
                    }
                });
    }

    public static void purchasePackage(String packageIdentifier,
                                       String offeringIdentifier) {
        purchasePackage(packageIdentifier, offeringIdentifier, null,  0);
    }

    public static void restorePurchases() {
        CommonKt.restorePurchases(getCustomerInfoListener(RESTORE_PURCHASES));
    }

    public static void logIn(String appUserId) {
        CommonKt.logIn(appUserId, getLogInListener(LOG_IN));
    }

    public static void logOut() {
        CommonKt.logOut(getCustomerInfoListener(LOG_OUT));
    }

    public static void setAllowSharingStoreAccount(boolean allowSharingStoreAccount) {
        CommonKt.setAllowSharingAppStoreAccount(allowSharingStoreAccount);
    }

    public static void getOfferings() {
        CommonKt.getOfferings(new OnResult() {
            @Override
            public void onReceived(Map<String, ?> map) {
                try {
                    JSONObject object = new JSONObject();
                    object.put("offerings", MappersHelpersKt.convertToJson(map));
                    sendJSONObject(object, GET_OFFERINGS);
                } catch (JSONException e) {
                    logJSONException(e);
                }
            }

            @Override
            public void onError(ErrorContainer errorContainer) {
                sendError(errorContainer, GET_OFFERINGS);
            }
        });
    }

    public static void syncObserverModeAmazonPurchase(
            String productID,
            String receiptID,
            String amazonUserID,
            String isoCurrencyCode,
            double price
    ) {
        Purchases.getSharedInstance().syncObserverModeAmazonPurchase(productID, receiptID,
                amazonUserID, isoCurrencyCode, price);
    }

    public static void setDebugLogsEnabled(boolean enabled) {
        CommonKt.setDebugLogsEnabled(enabled);
    }

    public static void setProxyURL(String proxyURL) {
        CommonKt.setProxyURLString(proxyURL);
    }

    public static String getAppUserID() {
        return CommonKt.getAppUserID();
    }

    public static void getCustomerInfo() {
        CommonKt.getCustomerInfo(getCustomerInfoListener(GET_CUSTOMER_INFO));
    }

    public static void setFinishTransactions(boolean enabled) {
        CommonKt.setFinishTransactions(enabled);
    }

    public static void syncPurchases() {
        CommonKt.syncPurchases();
    }

    public static boolean isAnonymous() {
        return CommonKt.isAnonymous();
    }

    public static void checkTrialOrIntroductoryPriceEligibility(String jsonProducts) {
        try {
            JSONObject request = new JSONObject(jsonProducts);
            JSONArray products = request.getJSONArray("productIdentifiers");
            List<String> productIds = new ArrayList<>();
            for (int i = 0; i < products.length(); i++) {
                String product = products.getString(i);
                productIds.add(product);
            }

            Map<String, Map<String, Object>> map = CommonKt.checkTrialOrIntroductoryPriceEligibility(productIds);
            sendJSONObject(MappersHelpersKt.convertToJson(map), CHECK_ELIGIBILITY);
        } catch (JSONException e) {
            Log.e("Purchases", "Failure parsing product identifiers " + jsonProducts);
        }
    }

    public static void invalidateCustomerInfoCache() {
        CommonKt.invalidateCustomerInfoCache();
    }

    public static void setAttributes(String jsonAttributes) {
        try {
            JSONObject jsonObject = new JSONObject(jsonAttributes);
            SubscriberAttributesKt.setAttributes(MappersHelpersKt.convertToMap(jsonObject));
        } catch (JSONException e) {
            Log.e("Purchases", "Failure parsing attributes " + jsonAttributes);
        }
    }

    public static void setEmail(String email) {
        SubscriberAttributesKt.setEmail(email);
    }

    public static void setPhoneNumber(String phoneNumber) {
        SubscriberAttributesKt.setPhoneNumber(phoneNumber);
    }

    public static void setDisplayName(String displayName) {
        SubscriberAttributesKt.setDisplayName(displayName);
    }

    public static void setPushToken(String token) {
        SubscriberAttributesKt.setPushToken(token);
    }

    public static void setAdjustID(String adjustID) {
        SubscriberAttributesKt.setAdjustID(adjustID);
    }

    public static void setAppsflyerID(String appsflyerID) {
        SubscriberAttributesKt.setAppsflyerID(appsflyerID);
    }

    public static void setFBAnonymousID(String fbAnonymousID) {
        SubscriberAttributesKt.setFBAnonymousID(fbAnonymousID);
    }

    public static void setMparticleID(String mparticleID) {
        SubscriberAttributesKt.setMparticleID(mparticleID);
    }

    public static void setOnesignalID(String onesignalID) {
        SubscriberAttributesKt.setOnesignalID(onesignalID);
    }

    public static void setAirshipChannelID(String airshipChannelID) {
        SubscriberAttributesKt.setAirshipChannelID(airshipChannelID);
    }

    public static void setMediaSource(String mediaSource) {
        SubscriberAttributesKt.setMediaSource(mediaSource);
    }

    public static void setCampaign(String campaign) {
        SubscriberAttributesKt.setCampaign(campaign);
    }

    public static void setAdGroup(String adGroup) {
        SubscriberAttributesKt.setAdGroup(adGroup);
    }

    public static void setAd(String ad) {
        SubscriberAttributesKt.setAd(ad);
    }

    public static void setKeyword(String keyword) {
        SubscriberAttributesKt.setKeyword(keyword);
    }

    public static void setCreative(String creative) {
        SubscriberAttributesKt.setCreative(creative);
    }

    public static void collectDeviceIdentifiers() {
        SubscriberAttributesKt.collectDeviceIdentifiers();
    }
    
    public static void canMakePayments(String featuresJson) {
        try {
            JSONObject request = new JSONObject(featuresJson);
            JSONArray features = request.getJSONArray("features");
            List<Integer> featuresToSend = new ArrayList<>();
            for (int i = 0; i < features.length(); i++) {
                int feature = features.getInt(i);
                featuresToSend.add(feature);
            }

            CommonKt.canMakePayments(UnityPlayer.currentActivity, featuresToSend, new OnResultAny<Boolean>() {
               @Override
               public void onReceived(Boolean canMakePayments) {
                   JSONObject object = new JSONObject();
                   try {
                       object.put("canMakePayments", canMakePayments);
                   } catch (JSONException e) {
                       logJSONException(e);
                   }
                   sendJSONObject(object, CAN_MAKE_PAYMENTS);
               }
   
               @Override
               public void onError(ErrorContainer errorContainer) {
                   sendError(errorContainer, CAN_MAKE_PAYMENTS);
               }
           });
        } catch (JSONException e) {
            logJSONException(e);
        }
    }

    public static void getPromotionalOffer(String productIdentifier, String discountIdentifier) {
        ErrorContainer errorContainer = CommonKt.getPromotionalOffer();
        sendError(errorContainer, GET_PROMOTIONAL_OFFER);
    }

    private static void logJSONException(JSONException e) {
        Log.e("Purchases", "JSON Error: " + e.getLocalizedMessage());
    }

    private static void sendJSONObject(JSONObject object, String method) {
        Log.e("Purchases", object.toString());
        UnityPlayer.UnitySendMessage(gameObject, method, object.toString());
    }

    private static void sendError(ErrorContainer error, String method) {
        JSONObject jsonObject = new JSONObject();
        try {
            jsonObject.put("error", MappersHelpersKt.convertToJson(error.getInfo()));
        } catch (JSONException e) {
            logJSONException(e);
        }
        sendJSONObject(jsonObject, method);
    }

    private static void sendCustomerInfo(Map<String, ?> map, String method) {
        JSONObject jsonObject = new JSONObject();
        try {
            jsonObject.put("customerInfo", MappersHelpersKt.convertToJson(map));
        } catch (JSONException e) {
            logJSONException(e);
        }
        sendJSONObject(jsonObject, method);
    }

    private static void sendErrorPurchase(ErrorContainer errorContainer) {
        JSONObject jsonObject = new JSONObject();
        try {
            jsonObject.put("error", MappersHelpersKt.convertToJson(errorContainer.getInfo()));
            jsonObject.put("userCancelled", errorContainer.getInfo().get("userCancelled"));
        } catch (JSONException e) {
            logJSONException(e);
        }
        sendJSONObject(jsonObject, MAKE_PURCHASE);
    }

    @NonNull
    private static OnResult getLogInListener(final String method) {
        return new OnResult() {
            @Override
            public void onReceived(Map<String, ?> map) {
                JSONObject jsonObject = new JSONObject();
                try {
                    Map<String, ?> customerInfoMap = (Map<String, ?>)map.get("customerInfo");
                    jsonObject.put("customerInfo", MappersHelpersKt.convertToJson(customerInfoMap));
                    jsonObject.put("created", (Boolean)map.get("created"));
                } catch (ClassCastException castException) {
                    Log.e("Purchases", "invalid casting Error: " + castException.getLocalizedMessage());
                } catch (JSONException e) {
                    logJSONException(e);
                }
                sendJSONObject(jsonObject, method);
            }

            @Override
            public void onError(ErrorContainer errorContainer) {
                sendError(errorContainer, method);
            }
        };
    }

    @NonNull
    private static OnResult getCustomerInfoListener(final String method) {
        return new OnResult() {
            @Override
            public void onReceived(Map<String, ?> map) {
                sendCustomerInfo(map, method);
            }

            @Override
            public void onError(ErrorContainer errorContainer) {
                sendError(errorContainer, method);
            }
        };
    }

    @Nullable
    private static DangerousSettings getDangerousSettingsFromJSON(String dangerousSettingsJSON) {
        JSONObject jsonObject;
        DangerousSettings dangerousSettings = null;
        try {
            jsonObject = new JSONObject(dangerousSettingsJSON);
            boolean autoSyncPurchases = jsonObject.getBoolean("AutoSyncPurchases");
            dangerousSettings = new DangerousSettings(autoSyncPurchases);
        } catch (JSONException e) {
            Log.e("Purchases", "Error parsing dangerousSettings JSON: " + dangerousSettingsJSON);
            logJSONException(e);
        }
        return dangerousSettings;
    }

}
