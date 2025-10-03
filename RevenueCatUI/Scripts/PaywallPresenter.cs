using System.Threading.Tasks;
using UnityEngine;

namespace RevenueCat.Internal.UI
{
    /// <summary>
    /// Platform-agnostic factory for paywall presenters.
    /// </summary>
    internal static class PaywallPresenter
    {
        private static IPaywallPresenter _instance;

        /// <summary>
        /// Gets the platform-specific paywall presenter instance.
        /// </summary>
        internal static IPaywallPresenter Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = CreatePlatformPresenter();
                }
                return _instance;
            }
        }

        private static IPaywallPresenter CreatePlatformPresenter()
        {
#if UNITY_IOS && !UNITY_EDITOR
            return new Platforms.IOSPaywallPresenter();
#elif UNITY_ANDROID && !UNITY_EDITOR
            return new Platforms.AndroidPaywallPresenter();
#else
            // Editor/other: unsupported implementation (no UI)
            return new UnsupportedPaywallPresenter();
#endif
        }
    }

    /// <summary>
    /// Fallback presenter for unsupported platforms.
    /// </summary>
    internal class UnsupportedPaywallPresenter : IPaywallPresenter
    {
        public Task<RevenueCat.PaywallResult> PresentPaywallAsync(RevenueCat.PaywallOptions options)
        {
            Debug.LogWarning("[RevenueCatUI] Paywall presentation is not supported on this platform.");
            return Task.FromResult(RevenueCat.PaywallResult.Error);
        }

        public Task<RevenueCat.PaywallResult> PresentPaywallIfNeededAsync(string requiredEntitlementIdentifier, RevenueCat.PaywallOptions options)
        {
            Debug.LogWarning("[RevenueCatUI] Paywall presentation is not supported on this platform.");
            return Task.FromResult(RevenueCat.PaywallResult.Error);
        }

        public bool IsSupported()
        {
            return false;
        }
    }
}
