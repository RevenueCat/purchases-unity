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
        /// The Customer Center allows users to manage their subscriptions, view purchase history,
        /// request refunds (iOS only), and access support options - all configured through the
        /// RevenueCat dashboard.
        /// </summary>
        /// <param name="callbacks">Optional callbacks for Customer Center events such as restore operations,
        /// refund requests, feedback surveys, and management options.</param>
        /// <returns>A task that completes when the presentation ends.</returns>
        /// <example>
        /// <code>
        /// var callbacks = new CustomerCenterCallbacks
        /// {
        ///     OnRestoreCompleted = (args) => Debug.Log($"Restore completed: {args.CustomerInfo}"),
        ///     OnRestoreFailed = (args) => Debug.LogError($"Restore failed: {args.Error.Message}"),
        ///     OnFeedbackSurveyCompleted = (args) => Debug.Log($"Survey completed: {args.FeedbackSurveyOptionId}")
        /// };
        /// await CustomerCenterPresenter.Present(callbacks);
        /// </code>
        /// </example>
        public static async Task Present(CustomerCenterCallbacks callbacks = null)
        {
            try
            {
                var presenter = CustomerCenterPlatformPresenter.Instance;
                await presenter.PresentAsync(callbacks ?? CustomerCenterCallbacks.None);
            }
            catch (Exception e)
            {
                Debug.LogError($"[RevenueCatUI] Error presenting Customer Center: {e.Message}");
            }
        }
    }
}
