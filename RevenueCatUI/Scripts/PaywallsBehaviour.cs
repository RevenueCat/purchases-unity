using System.Threading.Tasks;
using UnityEngine;

namespace RevenueCatUI
{
    /// <summary>
    /// MonoBehaviour helper that forwards to the static PaywallsPresenter API so paywalls can be driven from scenes.
    /// </summary>
    public class PaywallsBehaviour : MonoBehaviour
    {
        /// <summary>
        /// Presents a paywall configured in the RevenueCat dashboard.
        /// </summary>
        /// <param name="options">Options for presenting the paywall.</param>
        /// <returns>A <see cref="PaywallResult"/> describing the outcome.</returns>
        public async Task<PaywallResult> PresentPaywall(PaywallOptions options = null)
        {
            return await PaywallsPresenter.Present(options);
        }

        /// <summary>
        /// Presents a paywall only if the user does not have the specified entitlement.
        /// </summary>
        /// <param name="requiredEntitlementIdentifier">Entitlement identifier to check before presenting.</param>
        /// <param name="options">Options for presenting the paywall.</param>
        /// <returns>A <see cref="PaywallResult"/> describing the outcome.</returns>
        public async Task<PaywallResult> PresentPaywallIfNeeded(string requiredEntitlementIdentifier, PaywallOptions options = null)
        {
            return await PaywallsPresenter.PresentIfNeeded(requiredEntitlementIdentifier, options);
        }
    }
}
