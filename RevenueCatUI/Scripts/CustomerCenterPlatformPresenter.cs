using System.Threading.Tasks;
using UnityEngine;
using RevenueCatUI;

namespace RevenueCatUI.Internal
{
    /// <summary>
    /// Factory responsible for providing the platform-specific implementation that presents the Customer Center UI.
    /// </summary>
    internal static class CustomerCenterPlatformPresenter
    {
        private static ICustomerCenterPresenter _instance;

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
#if UNITY_IOS && !UNITY_EDITOR
            return new Platforms.IOSCustomerCenterPresenter();
#elif UNITY_ANDROID && !UNITY_EDITOR
            return new Platforms.AndroidCustomerCenterPresenter();
#else
            return new UnsupportedCustomerCenterPresenter();
#endif
        }
    }

    internal class UnsupportedCustomerCenterPresenter : ICustomerCenterPresenter
    {
        public Task<CustomerCenterResult> PresentAsync()
        {
            Debug.LogWarning("[RevenueCatUI] Customer Center presentation is not supported on this platform.");
            return Task.FromResult(CustomerCenterResult.NotPresented);
        }
    }
}
