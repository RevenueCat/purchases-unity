using System.Threading.Tasks;
using UnityEngine;

namespace RevenueCat.UI.Platforms.Stub
{
    /// <summary>
    /// Stub implementation that simulates Customer Center presentation.
    /// Enabled when scripting define symbol REVENUECAT_UI_STUBS is present.
    /// </summary>
    internal class StubCustomerCenterPresenter : ICustomerCenterPresenter
    {
        public bool IsSupported()
        {
            return true;
        }

        public async Task PresentCustomerCenterAsync()
        {
            Debug.Log("[RevenueCatUI][Stub] PresentCustomerCenter called. Completing immediately.");
            await Task.Yield();
        }
    }
}

