using System.Threading.Tasks;

namespace RevenueCatUI.Internal
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
        /// <param name="options">Paywall presentation options</param>
        /// <returns>Result of the paywall presentation</returns>
        Task<RevenueCatUI.PaywallResult> PresentPaywallAsync(RevenueCatUI.PaywallOptions options);

        /// <summary>
        /// Presents a paywall only if the user does not have the specified entitlement.
        /// </summary>
        /// <param name="requiredEntitlementIdentifier">Required entitlement identifier</param>
        /// <param name="options">Paywall presentation options</param>
        /// <returns>Result of the paywall presentation</returns>
        Task<RevenueCatUI.PaywallResult> PresentPaywallIfNeededAsync(string requiredEntitlementIdentifier, RevenueCatUI.PaywallOptions options);

        /// <summary>
        /// Whether paywall presentation is supported on this platform.
        /// Returns true on iOS/Android device builds; false on other platforms
        /// (Editor, Windows, macOS, WebGL, etc.).
        /// </summary>
        /// <returns>True if supported on the current platform, otherwise false.</returns>
        bool IsSupported();
    }
}
