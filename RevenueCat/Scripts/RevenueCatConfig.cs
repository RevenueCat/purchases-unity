using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace RevenueCat
{
    [CreateAssetMenu(fileName = "RevenueCatConfig.asset", menuName = "RevenueCat/RevenueCatConfig")]
    public class RevenueCatConfig : ScriptableObject
    {
        public static RevenueCatConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<RevenueCatConfig>(nameof(RevenueCatConfig));
                }

#if UNITY_EDITOR
                if (_instance == null)
                {
                    _instance = CreateInstance<RevenueCatConfig>();
                    const string resourcesDirectory = "Assets/Resources";

                    if (!System.IO.Directory.Exists(resourcesDirectory))
                    {
                        System.IO.Directory.CreateDirectory(resourcesDirectory);
                    }

                    var assetPath = System.IO.Path.Combine(resourcesDirectory, "RevenueCatConfig.asset");
                    UnityEditor.AssetDatabase.CreateAsset(_instance, assetPath);
                }
#endif // UNITY_EDITOR
                return _instance;
            }
        }
        private static RevenueCatConfig _instance;

#if UNITY_EDITOR
        private void OnValidate()
        {
            // always make sure an instance of this config exists when the Editor loads
            Assert.NotNull(Instance);
        }
#endif // UNITY_EDITOR

        [field: SerializeField]
        [field: Tooltip("RevenueCat API Key specifically for Apple platforms.\nGet from https://app.revenuecat.com/ \n" +
                      "NOTE: This value will be ignored if \"Use Runtime Setup\" is true. For Runtime Setup, you can configure " +
                      "it through PurchasesConfiguration instead")]
        public string RevenueCatApiKeyApple { get; private set; }

        [field: SerializeField]
        [field: Tooltip("RevenueCat API Key specifically for Google Play.\nGet from https://app.revenuecat.com/ \n" +
                      "NOTE: This value will be ignored if \"Use Runtime Setup\" is true. For Runtime Setup, you can configure " +
                      "it through PurchasesConfiguration instead")]
        public string RevenueCatApiKeyGoogle { get; private set; }

        [field: SerializeField]
        [field: Tooltip("RevenueCat API Key specifically for Amazon Appstore.\nGet from https://app.revenuecat.com/ \n" +
                 "NOTE: This value will be ignored if \"Use Runtime Setup\" is true. For Runtime Setup, you can configure " +
                 "it through PurchasesConfiguration instead")]
        public string RevenueCatApiKeyAmazon { get; private set; }

        [field: SerializeField]
        [field: Tooltip("Enables Amazon Store support. Android only, on iOS it has no effect.\n" +
                      "If enabled, it will use the API key in RevenueCatAPIKeyAmazon.\n" +
                      "NOTE: This value will be ignored if \"Use Runtime Setup\" is true. For Runtime Setup, you can configure " +
                      "it through PurchasesConfiguration instead")]

        public bool UseAmazon { get; private set; }

        public List<string> ProductIdentifiers => productIdentifiers;

        [SerializeField]
        [Tooltip("List of product identifiers.")]
        private List<string> productIdentifiers;

        [field: SerializeField]
        [field: Tooltip("An optional string. iOS only.\n" +
                 "Set this to use a specific NSUserDefaults suite for RevenueCat. " +
                 "This might be handy if you are deleting all NSUserDefaults in your app " +
                 "and leaving RevenueCat in a bad state.\n" +
                 "NOTE: This value will be ignored if \"Use Runtime Setup\" is true. For Runtime Setup, you can configure " +
                 "it through PurchasesConfiguration instead")]
        public string UserDefaultsSuiteName { get; private set; }

        [field: SerializeField]
        [field: Tooltip("Set this to MyApp and provide a StoreKitVersion if you have your own IAP implementation and\n" +
                        "want to only use RevenueCat's backend. Defaults to PurchasesAreCompletedBy.RevenueCat\n." +
                        "If you are on Android and setting this to MyApp, you will have to acknowledge the purchases yourself.\n" +
                        "If your app is only on Android, you may specify any StoreKit version, as it is ignored by the Android SDK.")]
        public PurchasesAreCompletedBy PurchasesAreCompletedBy { get; private set; } = PurchasesAreCompletedBy.REVENUECAT;

        [field: SerializeField]
        [field: Tooltip("Version of StoreKit to use in iOS. By default, RevenueCat will decide for you.\n" +
                        "Set this if you're setting PurchasesAreCompletedBy to MyApp.")]
        public StoreKitVersion StoreKitVersion { get; private set; } = StoreKitVersion.Default;

        [field: SerializeField]
        [field: Tooltip("Whether we should show store in-app messages automatically. Both Google Play and the App Store provide in-app " +
                      "messages for some situations like billing issues. By default, those messages will be shown automatically.\n" +
                      "This allows to disable that behavior, so you can display those messages at your convenience. For more information, " +
                      "check: https://rev.cat/storekit-message and https://rev.cat/googleplayinappmessaging")]
        public bool ShouldShowInAppMessagesAutomatically { get; private set; } = true;

        [field: SerializeField]
        [field: Tooltip("The entitlement verification mode to use. For more information, check: https://rev.cat/trusted-entitlements")]
        public EntitlementVerificationMode EntitlementVerificationMode { get; private set; } = EntitlementVerificationMode.DISABLED;

        [field: SerializeField]
        [field: Tooltip("Enable this setting if you want to allow pending purchases for prepaid subscriptions (only supported " +
                 "in Google Play). Note that entitlements are not granted until payment is done. Disabled by default.")]
        public bool PendingTransactionsForPrepaidPlansEnabled { get; private set; }

        [field: Header("Advanced")]
        [field: SerializeField]
        [field: Tooltip("Set this property to your proxy URL before configuring Purchases *only* if you've received " +
                      "a proxy key value from your RevenueCat contact.\n" +
                      "NOTE: This value will be ignored if \"Use Runtime Setup\" is true. For Runtime Setup, you can configure " +
                      "it through PurchasesConfiguration instead")]
        public string ProxyUrl { get; private set; }

        [field: Header("Dangerous Settings")]
        [field: SerializeField]
        [field: Tooltip("Disable or enable automatically detecting current subscriptions.\n" +
                        "If this is disabled, RevenueCat won't check current purchases, and it will not sync any purchase automatically " +
                        "when the app starts.\nCall syncPurchases whenever a new purchase is detected so the receipt is sent to " +
                        "RevenueCat's backend.\n" +
                        "In iOS, consumables disappear from the receipt after the transaction is finished, so make sure purchases " +
                        "are synced before finishing any consumable transaction, otherwise RevenueCat won't register the purchase.\n" +
                        "Auto syncing of purchases is enabled by default.\n" +
                        "NOTE: This value will be ignored if \"Use Runtime Setup\" is true. For Runtime Setup, you can configure " +
                        "it through PurchasesConfiguration instead")]
        public bool AutoSyncPurchases { get; private set; } = true;
    }
}