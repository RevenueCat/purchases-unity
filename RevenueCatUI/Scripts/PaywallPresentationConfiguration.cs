namespace RevenueCatUI
{
    /// <summary>
    /// Controls how paywalls are presented on iOS.
    /// </summary>
    public sealed class IOSPaywallPresentationStyle
    {
        /// <summary>
        /// Presents the paywall as a full-screen view controller.
        /// </summary>
        public static readonly IOSPaywallPresentationStyle FullScreen = new IOSPaywallPresentationStyle("fullScreen");

        /// <summary>
        /// Presents the paywall as a modal sheet. This is the default iOS behavior.
        /// </summary>
        public static readonly IOSPaywallPresentationStyle Sheet = new IOSPaywallPresentationStyle("sheet");

        internal string Value { get; }

        private IOSPaywallPresentationStyle(string value)
        {
            Value = value;
        }
    }

    /// <summary>
    /// Controls how paywalls are presented on Android.
    /// Currently only full-screen presentation is supported.
    /// </summary>
    public sealed class AndroidPaywallPresentationStyle
    {
        /// <summary>
        /// Presents the paywall as a full-screen activity. This is the default and only supported Android behavior.
        /// </summary>
        public static readonly AndroidPaywallPresentationStyle FullScreen = new AndroidPaywallPresentationStyle("fullScreen");

        internal string Value { get; }

        private AndroidPaywallPresentationStyle(string value)
        {
            Value = value;
        }
    }

    /// <summary>
    /// Configuration for how a paywall should be presented on each platform.
    /// Each platform field is optional; when null, the platform's default presentation style is used.
    /// </summary>
    public class PaywallPresentationConfiguration
    {
        /// <summary>
        /// The presentation style for iOS. Defaults to sheet if not specified.
        /// </summary>
        public IOSPaywallPresentationStyle IOS { get; }

        /// <summary>
        /// The presentation style for Android. Defaults to full screen if not specified.
        /// </summary>
        public AndroidPaywallPresentationStyle Android { get; }

        /// <summary>
        /// Creates a new PaywallPresentationConfiguration.
        /// </summary>
        /// <param name="ios">iOS presentation style. If null, uses the default (sheet).</param>
        /// <param name="android">Android presentation style. If null, uses the default (full screen).</param>
        public PaywallPresentationConfiguration(
            IOSPaywallPresentationStyle ios = null,
            AndroidPaywallPresentationStyle android = null)
        {
            IOS = ios;
            Android = android;
        }
    }
}
