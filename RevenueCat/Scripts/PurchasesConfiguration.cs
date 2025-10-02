using System;

public partial class Purchases
{
    /// <summary>
    /// Class used to configure the SDK programmatically.
    /// Create a configuration builder, set its properties, then call `Build` to obtain the configuration.
    /// Lastly, call Purchases.Configure and with the obtained PurchasesConfiguration object.
    /// </summary>
    ///
    /// <example>
    /// For example:
    /// <code>
    /// Purchases.PurchasesConfiguration.Builder builder = Purchases.PurchasesConfiguration.Builder.Init("api_key");
    /// Purchases.PurchasesConfiguration purchasesConfiguration =
    ///     builder
    ///         .SetAppUserId(appUserId)
    ///         .Build();
    /// purchases.Configure(purchasesConfiguration);
    /// </code>
    /// </example>
    ///
    public class PurchasesConfiguration
    {
        public readonly string ApiKey;
        public readonly string AppUserId;
        public readonly bool ObserverMode;
        public readonly PurchasesAreCompletedBy PurchasesAreCompletedBy;
        public readonly string UserDefaultsSuiteName;
        public readonly bool UseAmazon;
        public readonly DangerousSettings DangerousSettings;
        public readonly StoreKitVersion StoreKitVersion;
        public readonly bool ShouldShowInAppMessagesAutomatically;
        public readonly EntitlementVerificationMode EntitlementVerificationMode;
        public readonly bool PendingTransactionsForPrepaidPlansEnabled;

        private PurchasesConfiguration(string apiKey, string appUserId, PurchasesAreCompletedBy purchasesAreCompletedBy, string userDefaultsSuiteName,
            bool useAmazon, DangerousSettings dangerousSettings, StoreKitVersion storeKitVersion, bool shouldShowInAppMessagesAutomatically, 
            EntitlementVerificationMode entitlementVerificationMode, bool pendingTransactionsForPrepaidPlansEnabled)
        {
            ApiKey = apiKey;
            AppUserId = appUserId;
            PurchasesAreCompletedBy = purchasesAreCompletedBy;
            UserDefaultsSuiteName = userDefaultsSuiteName;
            UseAmazon = useAmazon;
            DangerousSettings = dangerousSettings;
            StoreKitVersion = storeKitVersion;
            ShouldShowInAppMessagesAutomatically = shouldShowInAppMessagesAutomatically;
            EntitlementVerificationMode = entitlementVerificationMode;
            PendingTransactionsForPrepaidPlansEnabled = pendingTransactionsForPrepaidPlansEnabled;
        }

        /// <summary>
        /// Use this object to create a PurchasesConfiguration object that can be used to configure
        /// the SDK programmatically.
        /// Create a configuration builder, set its properties, then call `Build` to obtain the configuration.
        /// Lastly, call Purchases.Configure and with the obtained PurchasesConfiguration object.
        /// </summary>
        ///
        /// <example>
        /// For example:
        /// <code>
        /// Purchases.PurchasesConfiguration.Builder builder = Purchases.PurchasesConfiguration.Builder.Init("api_key");
        /// Purchases.PurchasesConfiguration purchasesConfiguration =
        ///     builder
        ///         .SetAppUserId(appUserId)
        ///         .Build();
        /// purchases.Configure(purchasesConfiguration);
        /// </code>
        /// </example>
        ///
        public class Builder
        {
            private readonly string _apiKey;
            private string _appUserId;
            private PurchasesAreCompletedBy _purchasesAreCompletedBy;
            private string _userDefaultsSuiteName;
            private bool _useAmazon;
            private DangerousSettings _dangerousSettings;
            private StoreKitVersion _storeKitVersion;
            private bool _shouldShowInAppMessagesAutomatically;
            private EntitlementVerificationMode _entitlementVerificationMode;
            private bool _pendingTransactionsForPrepaidPlansEnabled;

            private Builder(string apiKey)
            {
                _apiKey = apiKey;
            }

            public static Builder Init(string apiKey)
            {
                return new Builder(apiKey);
            }

            public PurchasesConfiguration Build()
            {
                _dangerousSettings = _dangerousSettings ?? new DangerousSettings(true);
                return new PurchasesConfiguration(_apiKey, _appUserId, _purchasesAreCompletedBy, _userDefaultsSuiteName,
                    _useAmazon, _dangerousSettings, _storeKitVersion, _shouldShowInAppMessagesAutomatically, 
                    _entitlementVerificationMode, _pendingTransactionsForPrepaidPlansEnabled);
            }

            public Builder SetAppUserId(string appUserId)
            {
                _appUserId = appUserId;
                return this;
            }

            public Builder SetPurchasesAreCompletedBy(PurchasesAreCompletedBy purchasesAreCompletedBy, StoreKitVersion storeKitVersion)
            {
                _purchasesAreCompletedBy = purchasesAreCompletedBy;
                _storeKitVersion = storeKitVersion;
                return this;
            }

            public Builder SetUserDefaultsSuiteName(string userDefaultsSuiteName)
            {
                _userDefaultsSuiteName = userDefaultsSuiteName;
                return this;
            }

            public Builder SetUseAmazon(bool useAmazon)
            {
                _useAmazon = useAmazon;
                return this;
            }

            public Builder SetDangerousSettings(DangerousSettings dangerousSettings)
            {
                _dangerousSettings = dangerousSettings;
                return this;
            }

            public Builder SetStoreKitVersion(StoreKitVersion storeKitVersion)
            {
                _storeKitVersion = storeKitVersion;
                return this;
            }

            public Builder SetShouldShowInAppMessagesAutomatically(bool shouldShowInAppMessagesAutomatically)
            {
                _shouldShowInAppMessagesAutomatically = shouldShowInAppMessagesAutomatically;
                return this;
            }

            public Builder SetEntitlementVerificationMode(EntitlementVerificationMode entitlementVerificationMode)
            {
                _entitlementVerificationMode = entitlementVerificationMode;
                return this;
            }

            public Builder SetPendingTransactionsForPrepaidPlansEnabled(bool pendingTransactionsForPrepaidPlansEnabled)
            {
                _pendingTransactionsForPrepaidPlansEnabled = pendingTransactionsForPrepaidPlansEnabled;
                return this;
            }

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
