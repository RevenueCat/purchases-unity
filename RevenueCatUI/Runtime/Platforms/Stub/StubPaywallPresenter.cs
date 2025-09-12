using System;
using System.Threading.Tasks;
using UnityEngine;

namespace RevenueCat.UI.Platforms.Stub
{
    /// <summary>
    /// Stub implementation that simulates paywall behavior without native SDKs.
    /// Enabled when scripting define symbol REVENUECAT_UI_STUBS is present.
    /// </summary>
    internal class StubPaywallPresenter : IPaywallPresenter
    {
        public bool IsSupported()
        {
            return true;
        }

        public async Task<PaywallResult> PresentPaywallAsync(PaywallOptions options)
        {
            Debug.Log("[RevenueCatUI][Stub] PresentPaywall called. Simulating user cancel.");
            await Task.Yield();
            return PaywallResult.Cancelled;
        }

        public async Task<PaywallResult> PresentPaywallIfNeededAsync(string requiredEntitlementIdentifier, PaywallOptions options)
        {
            Debug.Log($"[RevenueCatUI][Stub] PresentPaywallIfNeeded called for entitlement '{requiredEntitlementIdentifier}'. Returning NotNeeded.");
            await Task.Yield();
            return PaywallResult.NotNeeded;
        }
    }
}

