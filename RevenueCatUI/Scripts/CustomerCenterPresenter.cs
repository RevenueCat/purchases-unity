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
        /// <param name="callbacks">Placeholder for future callback support.</param>
        /// <returns>A task that completes when the presentation ends.</returns>
        public static async Task Present(CustomerCenterCallbacks callbacks = null)
        {
            try
            {
                Debug.Log("[RevenueCatUI] Presenting Customer Center...");
                var presenter = CustomerCenterPlatformPresenter.Instance;
                await presenter.PresentAsync(callbacks ?? CustomerCenterCallbacks.None);
                Debug.Log("[RevenueCatUI] Customer Center finished.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[RevenueCatUI] Error presenting Customer Center: {e.Message}");
            }
        }
    }
}
