package com.revenuecat.purchases.unity;

import android.util.Log;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

import com.revenuecat.purchases.CustomerInfo;
import com.revenuecat.purchases.DangerousSettings;
import com.revenuecat.purchases.Purchases;
import com.revenuecat.purchases.PurchasesError;
import com.revenuecat.purchases.PurchasesErrorCode;
import com.revenuecat.purchases.Store;
import com.revenuecat.purchases.common.PlatformInfo;
import com.revenuecat.purchases.hybridcommon.CommonKt;
import com.revenuecat.purchases.hybridcommon.ErrorContainer;
import com.revenuecat.purchases.hybridcommon.OnNullableResult;
import com.revenuecat.purchases.hybridcommon.OnResult;
import com.revenuecat.purchases.hybridcommon.OnResultAny;
import com.revenuecat.purchases.hybridcommon.OnResultList;
import com.revenuecat.purchases.hybridcommon.SubscriberAttributesKt;
import com.revenuecat.purchases.hybridcommon.mappers.CustomerInfoMapperKt;
import com.revenuecat.purchases.hybridcommon.mappers.MappersHelpersKt;
import com.revenuecat.purchases.hybridcommon.mappers.PurchasesErrorKt;
import com.revenuecat.purchases.interfaces.UpdatedCustomerInfoListener;
import com.revenuecat.purchases.models.InAppMessageType;
import com.unity3d.player.UnityPlayer;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import kotlin.Unit;

public class PurchasesWrapper {
    // Java-side callbacks registered from C#; no UnitySendMessage fallback.
    public interface Callback {
        /** Called when the operation completes successfully. */
        void onCompleted(String json);

        /** Called when the operation fails with an error. */
        void onError(String json);
    }

    // Java-side callbacks registered from C#; no UnitySendMessage fallback.
    public interface CustomerInfoHandler {
        /** Called when a customer info is received. */
        void onCustomerReceived(String json);
    }

    public interface LogHandler {
        /** Called when a log is received. */
        void onLogReceived(String json);
    }

    private static final String PLATFORM_NAME = "unity";
    private static final String PLUGIN_VERSION = "9.0.0";

    private static CustomerInfoHandler customerInfoHandler = null;

    public static void configure(
            CustomerInfoHandler customerInfoHandler,
            LogHandler logHandler,
            String apiKey,
            String appUserId,
            String purchasesAreCompletedBy,
            String userDefaultsSuiteName,
            boolean useAmazon,
            boolean shouldShowInAppMessagesAutomatically,
            boolean autoSyncPurchases,
            String entitlementVerificationMode,
            boolean pendingTransactionsForPrepaidPlansEnabled) {
        try {
            PurchasesWrapper.customerInfoHandler = customerInfoHandler;
            CommonKt.setLogHandlerWithOnResult(new OnResult() {
                @Override
                public void onReceived(@NonNull Map<String, ?> map) {
                    logHandler.onLogReceived(MappersHelpersKt.convertToJson(map).toString());
                }

                @Override
                public void onError(@NonNull ErrorContainer errorContainer) {
                    // Intentionally left blank since it will never be called
                }
            });
            PlatformInfo platformInfo = new PlatformInfo(PLATFORM_NAME, PLUGIN_VERSION);
            Store store = useAmazon ? Store.AMAZON : Store.PLAY_STORE;
            DangerousSettings dangerousSettings = new DangerousSettings(autoSyncPurchases);
            CommonKt.configure(
                    UnityPlayer.currentActivity,
                    apiKey,
                    appUserId,
                    purchasesAreCompletedBy,
                    platformInfo,
                    store,
                    dangerousSettings,
                    shouldShowInAppMessagesAutomatically,
                    entitlementVerificationMode,
                    pendingTransactionsForPrepaidPlansEnabled);
            Purchases.getSharedInstance().setUpdatedCustomerInfoListener(new UpdatedCustomerInfoListener() {
                @Override
                public void onReceived(@NonNull CustomerInfo customerInfo) {
                    CustomerInfoMapperKt.mapAsync(customerInfo, map -> {
                        sendCustomerInfo(map);
                        return Unit.INSTANCE;
                    });
                }
            });
        } catch (Exception e) {
            Log.e("Purchases", "Error during setup: " + e.getLocalizedMessage());
        }
    }

    public static void getStorefront(Callback callback) {
        try {
            CommonKt.getStorefront(storefrontMap -> {
                if (storefrontMap != null) {
                    callback.onCompleted(MappersHelpersKt.convertToJson(storefrontMap).toString());
                } else {
                    callback.onCompleted("{}");
                }
                return null;
            });
        } catch (Exception e) {
            Log.e("Purchases", "Error during getStorefront: " + e.getLocalizedMessage());
            callback.onError("{\"error\":\"Error during getStorefront: " + e.getLocalizedMessage() + "\"}");
        }
    }

    public static void getProducts(Callback callback, String jsonProducts, String type) {
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
                        callback.onCompleted(object.toString());
                    } catch (JSONException e) {
                        logJSONException(e);
                        callback.onError("{\"error\":\"" + e.getLocalizedMessage() + "\"}");
                    }
                }

                @Override
                public void onError(ErrorContainer errorContainer) {
                    callback.onError(getError(errorContainer));
                }
            });
        } catch (JSONException e) {
            Log.e("Purchases", "Failure parsing product identifiers " + jsonProducts);
            callback.onError("{\"error\":\"Failure parsing product identifiers " + jsonProducts + "\"}");
        }
    }

    public static void purchaseProduct(
            Callback callback,
            final String productIdentifier,
            final String type,
            @Nullable final String oldSKU,
            final int prorationMode,
            final boolean isPersonalized,
            @Nullable final String presentedOfferingContextJSON) {
        Map<String, ?> presentedOfferingContext = null;
        try {
            if (presentedOfferingContextJSON != null) {
                JSONObject presentedOfferingContextJSONObject = new JSONObject(presentedOfferingContextJSON);
                presentedOfferingContext = MappersHelpersKt.convertToMap(presentedOfferingContextJSONObject);
            }
        } catch (JSONException e) {
            logJSONException(e);
            callback.onError("{\"error\":\"Failure parsing presentedOfferingContextJSON " +
                    presentedOfferingContextJSON + "\"}");
        }

        CommonKt.purchaseProduct(
                UnityPlayer.currentActivity,
                productIdentifier,
                type,
                null,
                oldSKU,
                prorationMode == 0 ? null : prorationMode,
                isPersonalized,
                presentedOfferingContext,
                new OnResult() {
                    @Override
                    public void onReceived(Map<String, ?> map) {
                        callback.onCompleted(MappersHelpersKt.convertToJson(map).toString());
                    }

                    @Override
                    public void onError(ErrorContainer errorContainer) {
                        callback.onError(getError(errorContainer));
                    }
                });
    }

    public static void purchaseProduct(Callback callback, String productIdentifier, String type) {
        purchaseProduct(callback, productIdentifier, type, null, 0, false, null);
    }

    public static void purchasePackage(
            Callback callback,
            String packageIdentifier,
            String presentedOfferingContextJSON,
            @Nullable final String oldSKU,
            final int prorationMode,
            final boolean isPersonalized) {
        try {
            JSONObject presentedOfferingContextJSONObject = new JSONObject(presentedOfferingContextJSON);
            Map<String, ?> presentedOfferingContext = MappersHelpersKt.convertToMap(presentedOfferingContextJSONObject);

            CommonKt.purchasePackage(
                    UnityPlayer.currentActivity,
                    packageIdentifier,
                    presentedOfferingContext,
                    oldSKU,
                    prorationMode == 0 ? null : prorationMode,
                    isPersonalized,
                    new OnResult() {
                        @Override
                        public void onReceived(Map<String, ?> map) {
                            callback.onCompleted(MappersHelpersKt.convertToJson(map).toString());
                        }

                        @Override
                        public void onError(ErrorContainer errorContainer) {
                            JSONObject jsonObject = new JSONObject();
                            try {
                                jsonObject.put("error", MappersHelpersKt.convertToJson(errorContainer.getInfo()));
                                jsonObject.put("userCancelled", errorContainer.getInfo().get("userCancelled"));
                            } catch (JSONException e) {
                                logJSONException(e);
                                callback.onError("{\"error\":\"" + e.getLocalizedMessage() + "\"}");
                                return;
                            }
                            callback.onError(jsonObject.toString());
                        }
                    });
        } catch (JSONException e) {
            logJSONException(e);
            callback.onError("{\"error\":\"Failure parsing presentedOfferingContextJSON " +
                    presentedOfferingContextJSON + "\"}");
        }

    }

    public static void purchasePackage(
            Callback callback,
            String packageIdentifier,
            String offeringIdentifier) {
        purchasePackage(callback, packageIdentifier, offeringIdentifier, null, 0, false);
    }

    public static void purchaseSubscriptionOption(
            Callback callback,
            final String productIdentifier,
            final String optionIdentifier,
            @Nullable final String oldSKU,
            final int prorationMode,
            final boolean isPersonalized,
            @Nullable final String presentedOfferingContextJSON) {
        Map<String, ?> presentedOfferingContext = null;
        try {
            if (presentedOfferingContextJSON != null) {
                JSONObject presentedOfferingContextJSONObject = new JSONObject(presentedOfferingContextJSON);
                presentedOfferingContext = MappersHelpersKt.convertToMap(presentedOfferingContextJSONObject);
            }

            CommonKt.purchaseSubscriptionOption(
                    UnityPlayer.currentActivity,
                    productIdentifier,
                    optionIdentifier,
                    oldSKU,
                    (prorationMode == 0) ? null : prorationMode,
                    isPersonalized,
                    presentedOfferingContext,
                    new OnResult() {
                        @Override
                        public void onReceived(Map<String, ?> map) {
                            callback.onCompleted(MappersHelpersKt.convertToJson(map).toString());
                        }

                        @Override
                        public void onError(ErrorContainer errorContainer) {
                            callback.onError(getError(errorContainer));
                        }
                    });
        } catch (JSONException e) {
            logJSONException(e);
            callback.onError("{\"error\":\"Failure parsing presentedOfferingContextJSON " +
                    presentedOfferingContextJSON + "\"}");
        }
    }

    public static void restorePurchases() {
        CommonKt.restorePurchases(getCustomerInfoListener(null));
    }

    public static void logIn(Callback callback, String appUserId) {
        CommonKt.logIn(appUserId, new OnResult() {
            @Override
            public void onReceived(Map<String, ?> map) {
                JSONObject jsonObject = new JSONObject();
                try {
                    Map<String, ?> customerInfoMap = (Map<String, ?>) map.get("customerInfo");
                    jsonObject.put("customerInfo", MappersHelpersKt.convertToJson(customerInfoMap));
                    jsonObject.put("created", (Boolean) map.get("created"));
                } catch (ClassCastException castException) {
                    Log.e("Purchases", "invalid casting Error: " + castException.getLocalizedMessage());
                    callback.onError("{\"error\":\"invalid casting Error: " +
                            castException.getLocalizedMessage() + "\"}");
                    return;
                } catch (JSONException e) {
                    logJSONException(e);
                    callback.onError("{\"error\":\"" + e.getLocalizedMessage() + "\"}");
                    return;
                }
                callback.onCompleted(jsonObject.toString());
            }

            @Override
            public void onError(ErrorContainer errorContainer) {
                callback.onError(getError(errorContainer));
            }
        });
    }

    public static void logOut() {
        CommonKt.logOut(getCustomerInfoListener(null));
    }

    public static void setAllowSharingStoreAccount(boolean allowSharingStoreAccount) {
        CommonKt.setAllowSharingAppStoreAccount(allowSharingStoreAccount);
    }

    public static void getOfferings(Callback callback) {
        CommonKt.getOfferings(new OnResult() {
            @Override
            public void onReceived(Map<String, ?> map) {
                try {
                    JSONObject object = new JSONObject();
                    object.put("offerings", MappersHelpersKt.convertToJson(map));
                    callback.onCompleted(object.toString());
                } catch (JSONException e) {
                    logJSONException(e);
                    callback.onError("{\"error\":\"" + e.getLocalizedMessage() + "\"}");
                }
            }

            @Override
            public void onError(ErrorContainer errorContainer) {
                callback.onError(getError(errorContainer));
            }
        });
    }

    public static void getCurrentOfferingForPlacement(Callback callback, String placementIdentifier) {
        CommonKt.getCurrentOfferingForPlacement(placementIdentifier, new OnNullableResult() {
            @Override
            public void onReceived(Map<String, ?> map) {
                try {
                    JSONObject offering = null;
                    if (map != null) {
                        offering = MappersHelpersKt.convertToJson(map);
                    }

                    JSONObject object = new JSONObject();
                    object.put("offering", offering);
                    callback.onCompleted(object.toString());
                } catch (JSONException e) {
                    logJSONException(e);
                    callback.onError("{\"error\":\"" + e.getLocalizedMessage() + "\"}");
                }
            }

            @Override
            public void onError(ErrorContainer errorContainer) {
                callback.onError(getError(errorContainer));
            }
        });
    }

    public static void syncAttributesAndOfferingsIfNeeded(Callback callback) {
        CommonKt.syncAttributesAndOfferingsIfNeeded(new OnResult() {
            @Override
            public void onReceived(Map<String, ?> map) {
                try {
                    JSONObject object = new JSONObject();
                    object.put("offerings", MappersHelpersKt.convertToJson(map));
                    callback.onCompleted(object.toString());
                } catch (JSONException e) {
                    logJSONException(e);
                    callback.onError("{\"error\":\"" + e.getLocalizedMessage() + "\"}");
                }
            }

            @Override
            public void onError(ErrorContainer errorContainer) {
                callback.onError(getError(errorContainer));
            }
        });
    }

    public static void syncAmazonPurchase(
            String productID,
            String receiptID,
            String amazonUserID,
            String isoCurrencyCode,
            double price) {
        Purchases.getSharedInstance().syncAmazonPurchase(productID, receiptID, amazonUserID, isoCurrencyCode, price);
    }

    public static void getAmazonLWAConsentStatus(Callback callback) {
        CommonKt.getAmazonLWAConsentStatus(new OnResultAny<Boolean>() {
            @Override
            public void onReceived(Boolean amazonLWAConsentStatus) {
                JSONObject object = new JSONObject();
                try {
                    object.put("amazonLWAConsentStatus", amazonLWAConsentStatus);
                } catch (JSONException e) {
                    logJSONException(e);
                    callback.onError("{\"error\":\"" + e.getLocalizedMessage() + "\"}");
                    return;
                }
                callback.onCompleted(object.toString());
            }

            @Override
            public void onError(@Nullable ErrorContainer errorContainer) {
                callback.onError(getError(errorContainer));
            }
        });
    }

    public static void setLogLevel(String level) {
        CommonKt.setLogLevel(level);
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

    public static void getCustomerInfo(Callback callback) {
        CommonKt.getCustomerInfo(getCustomerInfoListener(callback));
    }

    public static void syncPurchases() {
        CommonKt.syncPurchases(getCustomerInfoListener(null));
    }

    public static boolean isAnonymous() {
        return CommonKt.isAnonymous();
    }

    public static boolean isConfigured() {
        return Purchases.isConfigured();
    }

    public static void checkTrialOrIntroductoryPriceEligibility(Callback callback, String jsonProducts) {
        try {
            JSONObject request = new JSONObject(jsonProducts);
            JSONArray products = request.getJSONArray("productIdentifiers");
            List<String> productIds = new ArrayList<>();
            for (int i = 0; i < products.length(); i++) {
                String product = products.getString(i);
                productIds.add(product);
            }

            Map<String, Map<String, Object>> map = CommonKt.checkTrialOrIntroductoryPriceEligibility(productIds);
            callback.onCompleted(MappersHelpersKt.convertToJson(map).toString());
        } catch (JSONException e) {
            Log.e("Purchases", "Failure parsing product identifiers " + jsonProducts);
            callback.onError("{\"error\":\"Failure parsing product identifiers " + jsonProducts + "\"}");
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

    public static void setCleverTapID(String cleverTapID) {
        SubscriberAttributesKt.setCleverTapID(cleverTapID);
    }

    public static void setMixpanelDistinctID(String mixpanelDistinctID) {
        SubscriberAttributesKt.setMixpanelDistinctID(mixpanelDistinctID);
    }

    public static void setFirebaseAppInstanceID(String firebaseAppInstanceID) {
        SubscriberAttributesKt.setFirebaseAppInstanceID(firebaseAppInstanceID);
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

    public static void canMakePayments(Callback callback, String featuresJson) {
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
                    callback.onCompleted(object.toString());
                }

                @Override
                public void onError(ErrorContainer errorContainer) {
                    callback.onError(getError(errorContainer));
                }
            });
        } catch (JSONException e) {
            logJSONException(e);
            callback.onError("{\"error\":\"Failure parsing features " + featuresJson + "\"}");
        }
    }

    public static void getPromotionalOffer(Callback callback, String productIdentifier, String discountIdentifier) {
        // TODO verify NOOP ?
        // ErrorContainer errorContainer = CommonKt.getPromotionalOffer();
        // callback.onError(getError(errorContainer));
    }

    public static void showInAppMessages(String messagesJson) {
        try {
            JSONObject request = new JSONObject(messagesJson);
            JSONArray messageTypes = request.getJSONArray("messageTypes");
            if (messageTypes == null) {
                CommonKt.showInAppMessagesIfNeeded(UnityPlayer.currentActivity);
            } else {
                ArrayList<InAppMessageType> messageTypesList = new ArrayList<>();
                InAppMessageType[] inAppMessageTypes = InAppMessageType.values();
                for (int i = 0; i < messageTypes.length(); i++) {
                    int messageTypeInt = messageTypes.getInt(i);
                    InAppMessageType messageType = null;
                    if (messageTypeInt < inAppMessageTypes.length) {
                        messageType = inAppMessageTypes[messageTypeInt];
                    }
                    if (messageType != null) {
                        messageTypesList.add(messageType);
                    } else {
                        Log.e("PurchasesPlugin", "Unsupported in-app message type: " + messageTypeInt);
                    }
                }
                CommonKt.showInAppMessagesIfNeeded(UnityPlayer.currentActivity, messageTypesList);
            }
        } catch (JSONException e) {
            logJSONException(e);
        }
    }

    public static void parseAsWebPurchaseRedemption(Callback callback, String urlString) {
        boolean isWebPurchaseRedemptionURL = CommonKt.isWebPurchaseRedemptionURL(urlString);
        JSONObject object = new JSONObject();
        if (isWebPurchaseRedemptionURL) {
            try {
                object.put("redemptionLink", urlString);
            } catch (JSONException e) {
                logJSONException(e);
                callback.onError("{\"error\":\"" + e.getLocalizedMessage() + "\"}");
                return;
            }
        }
        callback.onCompleted(object.toString());
    }

    public static void redeemWebPurchase(Callback callback, String redemptionLink) {
        CommonKt.redeemWebPurchase(redemptionLink, new OnResult() {
            @Override
            public void onReceived(Map<String, ?> map) {
                callback.onCompleted(MappersHelpersKt.convertToJson(map));
            }

            @Override
            public void onError(ErrorContainer errorContainer) {
                callback.onError(errorContainer);
            }
        });
    }

    public static void getVirtualCurrencies(Callback callback) {
        CommonKt.getVirtualCurrencies(new OnResult() {
            @Override
            public void onReceived(Map<String, ?> map) {
                callback.onCompleted(MappersHelpersKt.convertToJson(map));
            }

            @Override
            public void onError(ErrorContainer errorContainer) {
                callback.onError(getError(errorContainer));
            }
        });
    }

    @Nullable
    public static String getCachedVirtualCurrencies() {
        Map<String, ?> map = CommonKt.getCachedVirtualCurrencies();

        if (map != null) {
            JSONObject cachedVirtualCurrencies = MappersHelpersKt.convertToJson(map);
            return cachedVirtualCurrencies.toString();
        }

        return null;
    }

    public static void invalidateVirtualCurrenciesCache() {
        CommonKt.invalidateVirtualCurrenciesCache();
    }

    public static void getEligibleWinBackOffersForProduct(String productIdentifier) {
        // NOOP
        log.e("Purchases", "Win-back offers are not supported on Android.");
    }

    // This function accepts a product identifier since the PHC code only fetches
    // eligible win-back offers for products
    public static void getEligibleWinBackOffersForPackage(String productIdentifier) {
        // NOOP
        log.e("Purchases", "Win-back offers are not supported on Android.");
    }

    public static void purchaseProductWithWinBackOffer(String productIdentifier, String winBackOfferIdentifier) {
        // NOOP
        log.e("Purchases", "Win-back offers are not supported on Android.");
    }

    public static void purchasePackageWithWinBackOffer(
            String packageIdentifier,
            String presentedOfferingContextJson,
            String winBackOfferIdentifier) {
        // NOOP
        log.e("Purchases", "Win-back offers are not supported on Android.");
    }

    private static void logJSONException(JSONException e) {
        Log.e("Purchases", "JSON Error: " + e.getLocalizedMessage());
    }

    private static String getError(ErrorContainer error) {
        JSONObject jsonObject = new JSONObject();
        try {
            jsonObject.put("error", MappersHelpersKt.convertToJson(error.getInfo()));
        } catch (JSONException e) {
            logJSONException(e);
            return "{\"error\":\"" + e.getLocalizedMessage() + "\"}";
        }
        return jsonObject.toString();
    }

    @NonNull
    private static OnResult getCustomerInfoListener(@CanBeNull Callback callback) {
        return new OnResult() {
            @Override
            public void onReceived(Map<String, ?> map) {
                sendCustomerInfo(map, callback);
            }

            @Override
            public void onError(ErrorContainer errorContainer) {
                Log.e("Purchases", "Error fetching customer info: " + errorContainer.getInfo().toString());
                if (callback != null) {
                    callback.onError(getError(errorContainer));
                }
            }
        };
    }

    private static void sendCustomerInfo(Map<String, ?> map, @CanBeNull Callback callback) {
        JSONObject jsonObject = new JSONObject();
        try {
            jsonObject.put("customerInfo", MappersHelpersKt.convertToJson(map));
        } catch (JSONException e) {
            logJSONException(e);
        }
        customerInfoHandler.onCompleted(jsonObject.toString());

        if (callback != null) {
            callback.onCompleted(jsonObject.toString());
        }
    }
}
