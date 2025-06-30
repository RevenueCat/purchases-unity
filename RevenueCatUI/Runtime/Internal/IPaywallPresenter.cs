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
        /// <param name="options">Paywall presentation options</param>
        /// <returns>Result of the paywall presentation</returns>
        Task<PaywallResult> PresentPaywallAsync(PaywallOptions options);

        /// <summary>
        /// Presents a paywall only if the user does not have the specified entitlement.
        /// </summary>
        /// <param name="requiredEntitlementIdentifier">Required entitlement identifier</param>
        /// <param name="options">Paywall presentation options</param>
        /// <returns>Result of the paywall presentation</returns>
        Task<PaywallResult> PresentPaywallIfNeededAsync(string requiredEntitlementIdentifier, PaywallOptions options);

        /// <summary>
        /// Checks if paywall presentation is supported on this platform.
        /// </summary>
        /// <returns>True if supported, false otherwise</returns>
        bool IsSupported();
    }
}
