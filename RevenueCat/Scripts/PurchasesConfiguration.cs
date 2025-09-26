using System;
using UnityEngine;

namespace RevenueCat
{
    /// <summary>
    /// Class used to configure the SDK programmatically.
    /// </summary>
    public sealed class PurchasesConfiguration
    {
        public string ApiKey { get; }

        public string AppUserId { get; }

        public PurchasesAreCompletedBy PurchasesAreCompletedBy { get; }

        public string UserDefaultsSuiteName { get; }

        public bool UseAmazon { get; }

        public DangerousSettings DangerousSettings { get; }

        public StoreKitVersion StoreKitVersion { get; }

        public bool ShouldShowInAppMessagesAutomatically { get; }

        public EntitlementVerificationMode EntitlementVerificationMode { get; }

        public bool PendingTransactionsForPrepaidPlansEnabled { get; }

        public PurchasesConfiguration(
            string apiKey,
            string appUserId = null,
            PurchasesAreCompletedBy purchasesAreCompletedBy = PurchasesAreCompletedBy.REVENUECAT,
            string userDefaultsSuiteName = null,
            bool useAmazon = false,
            DangerousSettings dangerousSettings = null,
            StoreKitVersion storeKitVersion = StoreKitVersion.Default,
            bool shouldShowInAppMessagesAutomatically = true,
            EntitlementVerificationMode entitlementVerificationMode = EntitlementVerificationMode.DISABLED,
            bool pendingTransactionsForPrepaidPlansEnabled = false)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new ArgumentException("API key cannot be null or empty.", nameof(apiKey));
            }

            ApiKey = apiKey;
            AppUserId = appUserId;
            PurchasesAreCompletedBy = purchasesAreCompletedBy;
            UserDefaultsSuiteName = userDefaultsSuiteName;
            UseAmazon = useAmazon;
            DangerousSettings = dangerousSettings ?? new DangerousSettings();
            StoreKitVersion = storeKitVersion;
            ShouldShowInAppMessagesAutomatically = shouldShowInAppMessagesAutomatically;
            EntitlementVerificationMode = entitlementVerificationMode;
            PendingTransactionsForPrepaidPlansEnabled = pendingTransactionsForPrepaidPlansEnabled;
        }

        public PurchasesConfiguration(string appUserId = null)
        {
            var config = RevenueCatConfig.Instance;

            if (Application.platform == RuntimePlatform.IPhonePlayer ||
                Application.platform == RuntimePlatform.OSXPlayer)
            {
                ApiKey = config.RevenueCatApiKeyApple;

                if (config.PurchasesAreCompletedBy == PurchasesAreCompletedBy.MY_APP && config.StoreKitVersion == StoreKitVersion.Default)
                {
                    throw new InvalidOperationException("You must set a StoreKit version if you are setting PurchasesAreCompletedBy to MyApp");
                }
            }
            else if (Application.platform == RuntimePlatform.Android ||
                     Utilities.IsAndroidEmulator())
            {
                ApiKey = config.UseAmazon ? config.RevenueCatApiKeyAmazon : config.RevenueCatApiKeyGoogle;
            }

            AppUserId = appUserId;
            PurchasesAreCompletedBy = config.PurchasesAreCompletedBy;
            UserDefaultsSuiteName = config.UserDefaultsSuiteName;
            UseAmazon = config.UseAmazon;
            DangerousSettings = new DangerousSettings(config.AutoSyncPurchases);
            StoreKitVersion = config.StoreKitVersion;
            ShouldShowInAppMessagesAutomatically = config.ShouldShowInAppMessagesAutomatically;
            EntitlementVerificationMode = config.EntitlementVerificationMode;
            PendingTransactionsForPrepaidPlansEnabled = config.PendingTransactionsForPrepaidPlansEnabled;
        }

        public override string ToString()
        {
            return
                $"{nameof(ApiKey)}: {ApiKey}\n" +
                $"{nameof(AppUserId)}: {AppUserId}\n" +
                $"{nameof(PurchasesAreCompletedBy)}: {PurchasesAreCompletedBy}\n" +
                $"{nameof(UserDefaultsSuiteName)}: {UserDefaultsSuiteName}\n" +
                $"{nameof(UseAmazon)}: {UseAmazon}\n" +
                $"{nameof(DangerousSettings)}: {DangerousSettings}\n" +
                $"{nameof(StoreKitVersion)}: {StoreKitVersion}\n" +
                $"{nameof(ShouldShowInAppMessagesAutomatically)}: {ShouldShowInAppMessagesAutomatically}\n" +
                $"{nameof(EntitlementVerificationMode)}: {EntitlementVerificationMode}\n" +
                $"{nameof(PendingTransactionsForPrepaidPlansEnabled)}: {PendingTransactionsForPrepaidPlansEnabled}\n";
        }
    }
}
