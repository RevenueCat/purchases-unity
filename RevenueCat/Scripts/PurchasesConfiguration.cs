public partial class Purchases
{
    /// <summary>
    /// Class used to configure the SDK programmatically.
    /// Create a configuration builder, set its properties, then call `Build` to obtain the configuration.
    /// Lastly, call Purchases.Configure and with the obtained PurchasesConfiguration object.
    /// </summary>
    public class PurchasesConfiguration
    {
        public readonly string ApiKey;
        public readonly string AppUserId;
        public readonly bool ObserverMode;
        public readonly string UserDefaultsSuiteName;
        public readonly bool UseAmazon;
        public readonly DangerousSettings DangerousSettings;

        private PurchasesConfiguration(string apiKey, string appUserId, bool observerMode, string userDefaultsSuiteName,
            bool useAmazon, DangerousSettings dangerousSettings)
        {
            ApiKey = apiKey;
            AppUserId = appUserId;
            ObserverMode = observerMode;
            UserDefaultsSuiteName = userDefaultsSuiteName;
            UseAmazon = useAmazon;
            DangerousSettings = dangerousSettings;
        }

        /// <summary>
        /// Use this object to create a PurchasesConfiguration object that can be used to configure
        /// the SDK programmatically. 
        /// Create a configuration builder, set its properties, then call `Build` to obtain the configuration.
        /// Lastly, call Purchases.Configure and with the obtained PurchasesConfiguration object.
        /// </summary>
        public class Builder
        {
            private readonly string _apiKey;
            private string _appUserId;
            private bool _observerMode;
            private string _userDefaultsSuiteName;
            private bool _useAmazon;
            private DangerousSettings _dangerousSettings;

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
                return new PurchasesConfiguration(_apiKey, _appUserId, _observerMode, _userDefaultsSuiteName,
                    _useAmazon, _dangerousSettings);
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
        }

        public override string ToString()
        {
            return
                $"{nameof(ApiKey)}: {ApiKey}\n" +
                $"{nameof(AppUserId)}: {AppUserId}\n" +
                $"{nameof(ObserverMode)}: {ObserverMode}\n" +
                $"{nameof(UserDefaultsSuiteName)}: {UserDefaultsSuiteName}\n" +
                $"{nameof(UseAmazon)}: {UseAmazon}\n" +
                $"{nameof(DangerousSettings)}: {DangerousSettings}";
        }
    }
}