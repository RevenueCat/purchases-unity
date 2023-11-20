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
        public readonly string UserDefaultsSuiteName;
        public readonly bool UseAmazon;
        public readonly DangerousSettings DangerousSettings;
        public readonly bool UsesStoreKit2IfAvailable;
        public readonly bool ShouldShowInAppMessagesAutomatically;
        public readonly EntitlementVerificationMode EntitlementVerificationMode;

        private PurchasesConfiguration(string apiKey, string appUserId, bool observerMode, string userDefaultsSuiteName,
            bool useAmazon, DangerousSettings dangerousSettings, bool usesStoreKit2IfAvailable, bool shouldShowInAppMessagesAutomatically, 
            EntitlementVerificationMode entitlementVerificationMode)
        {
            ApiKey = apiKey;
            AppUserId = appUserId;
            ObserverMode = observerMode;
            UserDefaultsSuiteName = userDefaultsSuiteName;
            UseAmazon = useAmazon;
            DangerousSettings = dangerousSettings;
            UsesStoreKit2IfAvailable = usesStoreKit2IfAvailable;
            ShouldShowInAppMessagesAutomatically = shouldShowInAppMessagesAutomatically;
            EntitlementVerificationMode = entitlementVerificationMode;
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
            private bool _observerMode;
            private string _userDefaultsSuiteName;
            private bool _useAmazon;
            private DangerousSettings _dangerousSettings;
            private bool _usesStoreKit2IfAvailable;
            private bool _shouldShowInAppMessagesAutomatically;
            private EntitlementVerificationMode _entitlementVerificationMode;

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
                _dangerousSettings = _dangerousSettings ?? new DangerousSettings(false);
                return new PurchasesConfiguration(_apiKey, _appUserId, _observerMode, _userDefaultsSuiteName,
                    _useAmazon, _dangerousSettings, _usesStoreKit2IfAvailable, _shouldShowInAppMessagesAutomatically, 
                    _entitlementVerificationMode);
            }

            public Builder SetAppUserId(string appUserId)
            {
                _appUserId = appUserId;
                return this;
            }

            public Builder SetObserverMode(bool observerMode)
            {
                _observerMode = observerMode;
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

            [Obsolete("RevenueCat currently uses StoreKit 1 for purchases, as its stability in production " +
                      "scenarios has proven to be more performant than StoreKit 2.\n" +
                      "We're collecting more data on the best approach, but StoreKit 1 vs StoreKit 2 is \n" +
                      "an implementation detail that you shouldn't need to care about.\n" +
                      "We recommend not using this parameter, letting RevenueCat decide for " +
                      "you which StoreKit implementation to use.", false)]
            public Builder SetUsesStoreKit2IfAvailable(bool usesStoreKit2IfAvailable)
            {
                _usesStoreKit2IfAvailable = usesStoreKit2IfAvailable;
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

        }

        public override string ToString()
        {
            return
                $"{nameof(ApiKey)}: {ApiKey}\n" +
                $"{nameof(AppUserId)}: {AppUserId}\n" +
                $"{nameof(ObserverMode)}: {ObserverMode}\n" +
                $"{nameof(UserDefaultsSuiteName)}: {UserDefaultsSuiteName}\n" +
                $"{nameof(UseAmazon)}: {UseAmazon}\n" +
                $"{nameof(DangerousSettings)}: {DangerousSettings}\n" +
                $"{nameof(UsesStoreKit2IfAvailable)}: {UsesStoreKit2IfAvailable}\n" +
                $"{nameof(ShouldShowInAppMessagesAutomatically)}: {ShouldShowInAppMessagesAutomatically}\n" +
                $"{nameof(EntitlementVerificationMode)}: {EntitlementVerificationMode}";
        }
    }
}
