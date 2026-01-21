package com.revenuecat.purchasesunity;

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
    private static final String RECEIVE_STOREFRONT = "_receiveStorefront";
    private static final String RECEIVE_PRODUCTS = "_receiveProducts";
    private static final String GET_CUSTOMER_INFO = "_getCustomerInfo";
    private static final String MAKE_PURCHASE = "_makePurchase";
    private static final String RECEIVE_CUSTOMER_INFO = "_receiveCustomerInfo";
    private static final String RESTORE_PURCHASES = "_restorePurchases";
    private static final String LOG_IN = "_logIn";
    private static final String LOG_OUT = "_logOut";
    private static final String GET_OFFERINGS = "_getOfferings";
    private static final String GET_CURRENT_OFFERING_FOR_PLACEMENT = "_getCurrentOfferingForPlacement";
    private static final String SYNC_ATTRIBUTES_AND_OFFERINGS_IF_NEEDED = "_syncAttributesAndOfferingsIfNeeded";
    private static final String CHECK_ELIGIBILITY = "_checkTrialOrIntroductoryPriceEligibility";
    private static final String CAN_MAKE_PAYMENTS = "_canMakePayments";
    private static final String GET_PROMOTIONAL_OFFER = "_getPromotionalOffer";
    private static final String GET_LWA_CONSENT_STATUS = "_getAmazonLWAConsentStatus";
    private static final String SYNC_PURCHASES = "_syncPurchases";
    private static final String PARSE_AS_WEB_PURCHASE_REDEMPTION = "_parseAsWebPurchaseRedemption";
    private static final String REDEEM_WEB_PURCHASE = "_redeemWebPurchase";
    private static final String GET_VIRTUAL_CURRENCIES = "_getVirtualCurrencies";
    private static final String GET_ELIGIBLE_WIN_BACK_OFFERS_FOR_PRODUCT = "_getEligibleWinBackOffersForProduct";
    private static final String GET_ELIGIBLE_WIN_BACK_OFFERS_FOR_PACKAGE = "_getEligibleWinBackOffersForPackage";
    private static final String PURCHASE_PRODUCT_WITH_WIN_BACK_OFFER = "_purchaseProductWithWinBackOffer";
    private static final String PURCHASE_PACKAGE_WITH_WIN_BACK_OFFER = "_purchasePackageWithWinBackOffer";
    private static final String HANDLE_LOG = "_handleLog";

    private static final String PLATFORM_NAME = "unity";
    private static final String PLUGIN_VERSION = "8.4.17";

    private static String gameObject;

    private static UpdatedCustomerInfoListener listener = new UpdatedCustomerInfoListener() {
        @Override
        public void onReceived(@NonNull CustomerInfo customerInfo) {
            CustomerInfoMapperKt.mapAsync(
                    customerInfo,
                    map -> {
                        sendCustomerInfo(map, RECEIVE_CUSTOMER_INFO);
                        return Unit.INSTANCE;
                    }
            );
        }
    };

    public static void setup(String apiKey,
                             String appUserId,
                             String gameObject,
                             String purchasesAreCompletedBy,
                             String userDefaultsSuiteName,
                             boolean useAmazon,
                             boolean shouldShowInAppMessagesAutomatically,
                             String dangerousSettingsJSON,
                             String entitlementVerificationMode,
                             boolean pendingTransactionsForPrepaidPlansEnabled) {
        PurchasesWrapper.gameObject = gameObject;
        PlatformInfo platformInfo = new PlatformInfo(PLATFORM_NAME, PLUGIN_VERSION);
        Store store = useAmazon ? Store.AMAZON : Store.PLAY_STORE;
        DangerousSettings dangerousSettings = getDangerousSettingsFromJSON(dangerousSettingsJSON);
        CommonKt.configure(UnityPlayer.currentActivity, apiKey, appUserId, purchasesAreCompletedBy, platformInfo, store,
                dangerousSettings, shouldShowInAppMessagesAutomatically, entitlementVerificationMode,
                pendingTransactionsForPrepaidPlansEnabled);
        Purchases.getSharedInstance().setUpdatedCustomerInfoListener(listener);
    }

    public static void getStorefront() {
        CommonKt.getStorefront(storefrontMap -> {
            if (storefrontMap != null) {
                sendJSONObject(MappersHelpersKt.convertToJson(storefrontMap), RECEIVE_STOREFRONT);
            } else {
                sendEmptyJSONObject(RECEIVE_STOREFRONT);
            }
            return null;
        });
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
                        sendJSONObject(MappersHelpersKt.convertToJson(map), MAKE_PURCHASE);
                    }

                    @Override
                    public void onError(ErrorContainer errorContainer) {
                        sendErrorPurchase(errorContainer);
                    }
                });
    }

    public static void purchaseProduct(String productIdentifier, String type) {
        purchaseProduct(productIdentifier, type, null, 0, false, null);
    }

    public static void purchasePackage(String packageIdentifier,
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
                            sendJSONObject(MappersHelpersKt.convertToJson(map), MAKE_PURCHASE);
                        }

                        @Override
                        public void onError(ErrorContainer errorContainer) {
                            sendErrorPurchase(errorContainer);
                        }
                    });
        } catch (JSONException e) {
            logJSONException(e);
        }

    }

    public static void purchasePackage(String packageIdentifier,
                                       String offeringIdentifier) {
        purchasePackage(packageIdentifier, offeringIdentifier, null, 0, false);
    }

    public static void purchaseSubscriptionOption(final String productIdentifer,
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
        } catch (JSONException e) {
            logJSONException(e);
        }

        CommonKt.purchaseSubscriptionOption(
                UnityPlayer.currentActivity,
                productIdentifer,
                optionIdentifier,
                oldSKU,
                (prorationMode == 0) ? null : prorationMode,
                isPersonalized,
                presentedOfferingContext,
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

    public static void getCurrentOfferingForPlacement(String placementIdentifier) {
        CommonKt.getCurrentOfferingForPlacement(placementIdentifier, new OnNullableResult() {
            @Override
            public void onReceived(Map<String, ?> map) {
                try {
                    if (map == null) {
                        sendEmptyJSONObject(GET_CURRENT_OFFERING_FOR_PLACEMENT);
                        return;
                    }
                    JSONObject offering = null;
                    if (map != null) {
                        offering = MappersHelpersKt.convertToJson(map);
                    }

                    JSONObject object = new JSONObject();
                    object.put("offering", offering);
                    sendJSONObject(object, GET_CURRENT_OFFERING_FOR_PLACEMENT);
                } catch (JSONException e) {
                    logJSONException(e);
                }
            }

            @Override
            public void onError(ErrorContainer errorContainer) {
                sendError(errorContainer, GET_CURRENT_OFFERING_FOR_PLACEMENT);
            }
        });
    }

    public static void syncAttributesAndOfferingsIfNeeded() {
        CommonKt.syncAttributesAndOfferingsIfNeeded(new OnResult() {
            @Override
            public void onReceived(Map<String, ?> map) {
                try {
                    JSONObject object = new JSONObject();
                    object.put("offerings", MappersHelpersKt.convertToJson(map));
                    sendJSONObject(object, SYNC_ATTRIBUTES_AND_OFFERINGS_IF_NEEDED);
                } catch (JSONException e) {
                    logJSONException(e);
                }
            }

            @Override
            public void onError(ErrorContainer errorContainer) {
                sendError(errorContainer, SYNC_ATTRIBUTES_AND_OFFERINGS_IF_NEEDED);
            }
        });
    }

    public static void syncAmazonPurchase(
            String productID,
            String receiptID,
            String amazonUserID,
            String isoCurrencyCode,
            double price
    ) {
        Purchases.getSharedInstance().syncAmazonPurchase(productID, receiptID,
                amazonUserID, isoCurrencyCode, price);
    }

    public static void getAmazonLWAConsentStatus() {
        CommonKt.getAmazonLWAConsentStatus(new OnResultAny<Boolean>() {
            @Override
            public void onReceived(Boolean amazonLWAConsentStatus) {
                JSONObject object = new JSONObject();
                try {
                    object.put("amazonLWAConsentStatus", amazonLWAConsentStatus);
                } catch (JSONException e) {
                    logJSONException(e);
                }
                sendJSONObject(object, GET_LWA_CONSENT_STATUS);
            }

            @Override
            public void onError(@Nullable ErrorContainer errorContainer) {
                sendError(errorContainer, GET_LWA_CONSENT_STATUS);
            }
        });
    }

    public static void setLogLevel(String level) {
        CommonKt.setLogLevel(level);
    }

    public static void setLogHandler() {
        CommonKt.setLogHandlerWithOnResult(new OnResult() {
            @Override
            public void onReceived(@NonNull Map<String, ?> map) {
                sendJSONObject(MappersHelpersKt.convertToJson(map), HANDLE_LOG);
            }

            @Override
            public void onError(@NonNull ErrorContainer errorContainer) {
                // Intentionally left blank since it will never be called
            }
        });
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

    public static void syncPurchases() {
        CommonKt.syncPurchases(getCustomerInfoListener(SYNC_PURCHASES));
    }

    public static boolean isAnonymous() {
        return CommonKt.isAnonymous();
    }

    public static boolean isConfigured() {
        return Purchases.isConfigured();
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

    public static void parseAsWebPurchaseRedemption(String urlString) {
        boolean isWebPurchaseRedemptionURL = CommonKt.isWebPurchaseRedemptionURL(urlString);
        if (isWebPurchaseRedemptionURL) {
            JSONObject object = new JSONObject();
            try {
                object.put("redemptionLink", urlString);
            } catch (JSONException e) {
                logJSONException(e);
            }
            sendJSONObject(object, PARSE_AS_WEB_PURCHASE_REDEMPTION);
        } else {
            sendJSONObject(null, PARSE_AS_WEB_PURCHASE_REDEMPTION);
        }
    }

    public static void redeemWebPurchase(String redemptionLink) {
        CommonKt.redeemWebPurchase(redemptionLink, new OnResult() {
            @Override
            public void onReceived(Map<String, ?> map) {
                sendJSONObject(MappersHelpersKt.convertToJson(map), REDEEM_WEB_PURCHASE);
            }

            @Override
            public void onError(ErrorContainer errorContainer) {
                sendError(errorContainer, REDEEM_WEB_PURCHASE);
            }
        });
    }

    public static void getVirtualCurrencies() {
        CommonKt.getVirtualCurrencies(new OnResult() {
            @Override
            public void onReceived(Map<String, ?> map) {
                sendJSONObject(MappersHelpersKt.convertToJson(map), GET_VIRTUAL_CURRENCIES);
            }

            @Override
            public void onError(ErrorContainer errorContainer) {
                sendError(errorContainer, GET_VIRTUAL_CURRENCIES);
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
        PurchasesError error = new PurchasesError(
                PurchasesErrorCode.UnsupportedError,
                "Win-back offers are not supported on Android.");

        ErrorContainer errorContainer = PurchasesErrorKt.map(
                error,
                new HashMap<>());
        sendError(errorContainer, GET_ELIGIBLE_WIN_BACK_OFFERS_FOR_PRODUCT);
    }

    // This function accepts a product identifier since the PHC code only fetches
    // eligible win-back offers for products
    public static void getEligibleWinBackOffersForPackage(String productIdentifier) {
        // NOOP
        PurchasesError error = new PurchasesError(
                PurchasesErrorCode.UnsupportedError,
                "Win-back offers are not supported on Android.");

        ErrorContainer errorContainer = PurchasesErrorKt.map(
                error,
                new HashMap<>());
        sendError(errorContainer, GET_ELIGIBLE_WIN_BACK_OFFERS_FOR_PACKAGE);
    }

    public static void purchaseProductWithWinBackOffer(String productIdentifier, String winBackOfferIdentifier) {
        // NOOP
        PurchasesError error = new PurchasesError(
                PurchasesErrorCode.UnsupportedError,
                "Win-back offers are not supported on Android.");

        ErrorContainer errorContainer = PurchasesErrorKt.map(error, new HashMap<>());
        sendError(errorContainer, PURCHASE_PRODUCT_WITH_WIN_BACK_OFFER);
    }

    public static void purchasePackageWithWinBackOffer(String packageIdentifier, String presentedOfferingContextJson, String winBackOfferIdentifier) {
        // NOOP
        PurchasesError error = new PurchasesError(
                PurchasesErrorCode.UnsupportedError,
                "Win-back offers are not supported on Android.");

        ErrorContainer errorContainer = PurchasesErrorKt.map(error, new HashMap<>());
        sendError(errorContainer, PURCHASE_PACKAGE_WITH_WIN_BACK_OFFER);
    }

    private static void logJSONException(JSONException e) {
        Log.e("Purchases", "JSON Error: " + e.getLocalizedMessage());
    }

    static void sendEmptyJSONObject(String method) {
        UnityPlayer.UnitySendMessage(gameObject, method, "{}");
    }

    static void sendJSONObject(JSONObject object, String method) {
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
                    Map<String, ?> customerInfoMap = (Map<String, ?>) map.get("customerInfo");
                    jsonObject.put("customerInfo", MappersHelpersKt.convertToJson(customerInfoMap));
                    jsonObject.put("created", (Boolean) map.get("created"));
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
