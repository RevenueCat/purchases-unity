using System.Threading.Tasks;

namespace RevenueCat.UI
{
    /// <summary>
    /// Internal interface for presenting paywalls.
    /// Implemented by platform-specific presenters.
    /// </summary>
    internal interface IPaywallPresenter
    {
        /// <summary>
        /// Presents a paywall with the given options.
        /// </summary>
        /// <param name="gameObjectName">Name of GameObject to receive callbacks</param>
        /// <param name="options">Paywall presentation options</param>
        /// <returns>Result of the paywall presentation</returns>
        Task<PaywallResult> PresentPaywallAsync(string gameObjectName, PaywallOptions options);

        /// <summary>
        /// Presents a paywall only if the user does not have the specified entitlement.
        /// </summary>
        /// <param name="gameObjectName">Name of GameObject to receive callbacks</param>
        /// <param name="requiredEntitlementIdentifier">Required entitlement identifier</param>
        /// <param name="options">Paywall presentation options</param>
        /// <returns>Result of the paywall presentation</returns>
        Task<PaywallResult> PresentPaywallIfNeededAsync(string gameObjectName, string requiredEntitlementIdentifier, PaywallOptions options);

        /// <summary>
        /// Whether paywall presentation is supported on this platform.
        /// Returns true on iOS/Android device builds; false on other platforms
        /// (Editor, Windows, macOS, WebGL, etc.).
        /// </summary>
        /// <returns>True if supported on the current platform, otherwise false.</returns>
        bool IsSupported();
    }
}
