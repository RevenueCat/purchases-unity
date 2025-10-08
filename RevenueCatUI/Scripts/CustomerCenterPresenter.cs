using System;
using System.Threading.Tasks;
using UnityEngine;
using RevenueCatUI.Internal;

namespace RevenueCatUI
{
    /// <summary>
    /// Main interface for presenting the RevenueCat Customer Center experience.
    /// </summary>
    public static class CustomerCenterPresenter
    {
        /// <summary>
        /// Presents the Customer Center UI modally.
        /// </summary>
        /// <returns>A task describing the outcome of the presentation.</returns>
        public static async Task<CustomerCenterResult> Present()
        {
            try
            {
                Debug.Log("[RevenueCatUI] Presenting Customer Center...");
                var presenter = CustomerCenterPlatformPresenter.Instance;
                var result = await presenter.PresentAsync();
                Debug.Log($"[RevenueCatUI] Customer Center finished with result: {result.Result}");
                return result;
            }
            catch (Exception e)
            {
                Debug.LogError($"[RevenueCatUI] Error presenting Customer Center: {e.Message}");
                return CustomerCenterResult.Error;
            }
        }
    }
}
