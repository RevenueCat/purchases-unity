namespace RevenueCatUI
{
    /// <summary>
    /// Controls how paywalls are presented on iOS.
    /// </summary>
    public sealed class IOSPaywallPresentationStyle
    {
        /// <summary>
        /// Presents the paywall as a full-screen view controller.
        /// <para>
        /// Unlike sheets, a full-screen paywall decides its own orientation: it can rotate to any orientation the app
        /// allows for its window. Unity normally limits that to the game's <c>Screen.orientation</c> / autorotation
        /// settings, so the paywall follows the game. The exception is a game that changes <c>Screen.orientation</c>
        /// and presents the paywall before Unity applies the change (same frame): Unity defers the change while a
        /// view controller is presented, so the paywall comes up in the previous orientation and the game only rotates
        /// after it is dismissed (requires Info.plist to list both orientations). Use <see cref="FullScreenLandscape"/>
        /// or <see cref="FullScreenPortrait"/> to pin the paywall in that case.
        /// </para>
        /// </summary>
        public static readonly IOSPaywallPresentationStyle FullScreen = new IOSPaywallPresentationStyle("fullScreen");

        /// <summary>
        /// Presents the paywall as a full-screen view controller kept in landscape (rotating between landscape left
        /// and right when both are allowed), even if the app would otherwise let it rotate to portrait.
        /// <para>
        /// The paywall can only restrict the orientations the app allows, not add to them: landscape must be permitted
        /// by Info.plist and by the game's current <c>Screen.orientation</c> / autorotation settings (Unity limits the app
        /// window to those). If it isn't, a warning is logged and the paywall behaves like <see cref="FullScreen"/>.
        /// To present in an orientation the game is currently locked out of, change the game's orientation first (e.g.
        /// <c>Screen.autorotateToLandscapeLeft = true; Screen.orientation = ScreenOrientation.LandscapeLeft;</c>) and
        /// restore it after the paywall is dismissed. A pending <c>Screen.orientation</c> change made in the same frame
        /// is applied before the paywall is presented, so the game rotates together with the paywall.
        /// </para>
        /// </summary>
        public static readonly IOSPaywallPresentationStyle FullScreenLandscape = new IOSPaywallPresentationStyle("fullScreen", "landscape");

        /// <summary>
        /// Presents the paywall as a full-screen view controller kept in portrait (including upside-down portrait when
        /// allowed). Subject to the same constraints as <see cref="FullScreenLandscape"/>: portrait must be permitted by
        /// Info.plist and by the game's current orientation settings, otherwise a warning is logged and the paywall
        /// behaves like <see cref="FullScreen"/>.
        /// </summary>
        public static readonly IOSPaywallPresentationStyle FullScreenPortrait = new IOSPaywallPresentationStyle("fullScreen", "portrait");

        /// <summary>
        /// Presents the paywall as a modal sheet. This is the default iOS behavior.
        /// On iPad this covers most of the screen; use <see cref="FormSheet"/> for a smaller, centered modal.
        /// Sheets always follow the presenting view controller's orientation, i.e. the game's.
        /// </summary>
        public static readonly IOSPaywallPresentationStyle Sheet = new IOSPaywallPresentationStyle("sheet");

        /// <summary>
        /// Presents the paywall as a form sheet: a smaller, centered modal on iPad.
        /// On iPhone this renders the same as <see cref="Sheet"/> (the standard bottom sheet).
        /// Requires a PurchasesHybridCommon version that supports the `presentationMode` option;
        /// on older versions it falls back to <see cref="Sheet"/>.
        /// </summary>
        public static readonly IOSPaywallPresentationStyle FormSheet = new IOSPaywallPresentationStyle("formSheet");

        internal string Value { get; }

        /// <summary>
        /// Forced interface orientation for full-screen styles ("landscape" or "portrait"), or null to follow Info.plist.
        /// </summary>
        internal string Orientation { get; }

        internal bool IsFullScreen => Value == "fullScreen";

        private IOSPaywallPresentationStyle(string value, string orientation = null)
        {
            Value = value;
            Orientation = orientation;
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
    /// <example>
    /// <code>
    /// // Full screen on all platforms:
    /// var options = new PaywallOptions(
    ///     presentationConfiguration: PaywallPresentationConfiguration.FullScreen
    /// );
    /// await PaywallsPresenter.Present(options);
    ///
    /// // Full screen on iOS only (Android is always full screen):
    /// var options = new PaywallOptions(
    ///     presentationConfiguration: new PaywallPresentationConfiguration(
    ///         ios: IOSPaywallPresentationStyle.FullScreen
    ///     )
    /// );
    /// await PaywallsPresenter.Present(options);
    ///
    /// // Full screen on iOS, pinned to landscape (see IOSPaywallPresentationStyle.FullScreenLandscape):
    /// var options = new PaywallOptions(
    ///     presentationConfiguration: new PaywallPresentationConfiguration(
    ///         ios: IOSPaywallPresentationStyle.FullScreenLandscape
    ///     )
    /// );
    /// await PaywallsPresenter.Present(options);
    ///
    /// // Default behavior (sheet on iOS, full screen on Android):
    /// await PaywallsPresenter.Present();
    /// </code>
    /// </example>
    /// </summary>
    public class PaywallPresentationConfiguration
    {
        /// <summary>
        /// Full-screen presentation on all platforms.
        /// </summary>
        public static readonly PaywallPresentationConfiguration FullScreen =
            new PaywallPresentationConfiguration(
                ios: IOSPaywallPresentationStyle.FullScreen,
                android: AndroidPaywallPresentationStyle.FullScreen
            );

        /// <summary>
        /// Default platform behavior (sheet on iOS, full screen on Android).
        /// </summary>
        public static readonly PaywallPresentationConfiguration Default =
            new PaywallPresentationConfiguration();

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
