using System.Threading.Tasks;
using UnityEngine;

namespace RevenueCat.UI
{
    /// <summary>
    /// Platform-agnostic factory for customer center presenters.
    /// </summary>
    internal static class CustomerCenterPresenter
    {
        private static ICustomerCenterPresenter _instance;

        /// <summary>
        /// Gets the platform-specific customer center presenter instance.
        /// </summary>
        internal static ICustomerCenterPresenter Instance
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

        private static ICustomerCenterPresenter CreatePlatformPresenter()
        {
#if REVENUECAT_UI_STUBS
            return new Platforms.Stub.StubCustomerCenterPresenter();
#elif UNITY_IOS && !UNITY_EDITOR
            return new Platforms.IOSCustomerCenterPresenter();
#elif UNITY_ANDROID && !UNITY_EDITOR
            return new Platforms.AndroidCustomerCenterPresenter();
#else
            return new UnsupportedCustomerCenterPresenter();
#endif
        }
    }

    /// <summary>
    /// Fallback presenter for unsupported platforms.
    /// </summary>
    internal class UnsupportedCustomerCenterPresenter : ICustomerCenterPresenter
    {
        public Task PresentCustomerCenterAsync()
        {
            Debug.LogWarning("[RevenueCatUI] Customer center presentation is not supported on this platform.");
            return Task.CompletedTask;
        }

        public bool IsSupported()
        {
            return false;
        }
    }
} 
