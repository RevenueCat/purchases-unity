using System.Threading.Tasks;
using UnityEngine;
using RevenueCat.UI;

namespace RevenueCat
{
    /// <summary>
    /// MonoBehaviour helper that forwards to the static <see cref="UIPresentation"/> facade so paywalls can be driven from scenes.
    /// </summary>
    public class UIBehaviour : MonoBehaviour
    {
        /// <summary>
        /// Presents a paywall configured in the RevenueCat dashboard.
        /// </summary>
        /// <param name="options">Options for presenting the paywall.</param>
        /// <returns>A <see cref="PaywallResult"/> describing the outcome.</returns>
        public Task<PaywallResult> PresentPaywall(PaywallOptions options = null)
        {
            return UIPresentation.PresentPaywallAsync(gameObject?.name, options);
        }

        /// <summary>
        /// Presents a paywall only if the user does not have the specified entitlement.
        /// </summary>
        /// <param name="requiredEntitlementIdentifier">Entitlement identifier to check before presenting.</param>
        /// <param name="options">Options for presenting the paywall.</param>
        /// <returns>A <see cref="PaywallResult"/> describing the outcome.</returns>
        public Task<PaywallResult> PresentPaywallIfNeeded(string requiredEntitlementIdentifier, PaywallOptions options = null)
        {
            return UIPresentation.PresentPaywallIfNeededAsync(gameObject?.name, requiredEntitlementIdentifier, options);
        }

        /// <summary>
        /// Checks if the Paywall UI is available on the current platform.
        /// Returns true on iOS/Android device builds when paywall is supported; otherwise false.
        /// </summary>
        /// <returns>True if UI is supported on this platform, otherwise false.</returns>
        public bool IsSupported()
        {
            return UIPresentation.IsSupported();
        }
    }
}
