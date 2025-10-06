using System;
using UnityEngine;
using UnityEngine.Events;

namespace RevenueCatUI
{
    /// <summary>
    /// MonoBehaviour component for presenting RevenueCat paywalls from the Unity Editor.
    /// Provides an alternative to PaywallsPresenter for developers who prefer configuring
    /// paywalls through Unity's Inspector interface.
    /// </summary>
    [AddComponentMenu("RevenueCat/Paywalls Behaviour")]
    public class PaywallsBehaviour : MonoBehaviour
    {
        [Header("Paywall Options")]
        [Tooltip("The identifier of the offering to present. Leave empty to use the current offering.")]
        [SerializeField] private string offeringIdentifier;
        
        [Tooltip("Whether to display a close button on the paywall (only for original template paywalls).")]
        [SerializeField] private bool displayCloseButton = false;

        [Header("Conditional Presentation")]
        [Tooltip("If set, the paywall will only be presented if the user doesn't have this entitlement.")]
        [SerializeField] private string requiredEntitlementIdentifier;

        [Header("Auto Presentation")]
        [Tooltip("Automatically present the paywall when this component starts.")]
        [SerializeField] private bool presentOnStart = false;

        [Header("Events")]
        [Tooltip("Invoked when the paywall presentation is complete with the result.")]
        public PaywallResultEvent OnPaywallResult = new PaywallResultEvent();

        private bool isPresenting = false;

        public string OfferingIdentifier
        {
            get => offeringIdentifier;
            set => offeringIdentifier = value;
        }

        public bool DisplayCloseButton
        {
            get => displayCloseButton;
            set => displayCloseButton = value;
        }

        public string RequiredEntitlementIdentifier
        {
            get => requiredEntitlementIdentifier;
            set => requiredEntitlementIdentifier = value;
        }

        private void Start()
        {
            if (presentOnStart)
            {
                PresentPaywall();
            }
        }

        /// <summary>
        /// Presents the paywall with the configured options.
        /// Can be called from Unity UI buttons or programmatically.
        /// </summary>
        public async void PresentPaywall()
        {
            if (isPresenting)
            {
                Debug.LogWarning("[RevenueCatUI] Paywall is already being presented.");
                return;
            }

            if (!PaywallsPresenter.IsSupported())
            {
                Debug.LogWarning("[RevenueCatUI] Paywall UI is not supported on this platform.");
                HandleResult(PaywallResult.Error);
                return;
            }

            isPresenting = true;

            try
            {
                var options = CreateOptions();
                PaywallResult result;

                if (!string.IsNullOrEmpty(requiredEntitlementIdentifier))
                {
                    result = await PaywallsPresenter.PresentIfNeeded(requiredEntitlementIdentifier, options);
                }
                else
                {
                    result = await PaywallsPresenter.Present(options);
                }

                HandleResult(result);
            }
            catch (Exception e)
            {
                Debug.LogError($"[RevenueCatUI] Exception in PaywallsBehaviour: {e.Message}");
                HandleResult(PaywallResult.Error);
            }
            finally
            {
                isPresenting = false;
            }
        }

        /// <summary>
        /// Presents a paywall only if the user does not have the specified entitlement.
        /// </summary>
        /// <param name="entitlementIdentifier">Entitlement identifier to check before presenting</param>
        public async void PresentPaywallIfNeeded(string entitlementIdentifier)
        {
            if (string.IsNullOrEmpty(entitlementIdentifier))
            {
                Debug.LogError("[RevenueCatUI] Entitlement identifier cannot be null or empty.");
                HandleResult(PaywallResult.Error);
                return;
            }

            if (isPresenting)
            {
                Debug.LogWarning("[RevenueCatUI] Paywall is already being presented.");
                return;
            }

            if (!PaywallsPresenter.IsSupported())
            {
                Debug.LogWarning("[RevenueCatUI] Paywall UI is not supported on this platform.");
                HandleResult(PaywallResult.Error);
                return;
            }

            isPresenting = true;

            try
            {
                var options = CreateOptions();
                var result = await PaywallsPresenter.PresentIfNeeded(entitlementIdentifier, options);
                HandleResult(result);
            }
            catch (Exception e)
            {
                Debug.LogError($"[RevenueCatUI] Exception in PaywallsBehaviour: {e.Message}");
                HandleResult(PaywallResult.Error);
            }
            finally
            {
                isPresenting = false;
            }
        }

        /// <summary>
        /// Checks if the Paywall UI is available on the current platform.
        /// Returns true on iOS/Android device builds when paywall is supported;
        /// returns false on other platforms (Editor, Windows, macOS, WebGL, etc.).
        /// </summary>
        /// <returns>True if UI is supported on this platform, otherwise false.</returns>
        public bool IsPaywallSupported()
        {
            return PaywallsPresenter.IsSupported();
        }

        private PaywallOptions CreateOptions()
        {
            return new PaywallOptions
            {
                OfferingIdentifier = string.IsNullOrEmpty(offeringIdentifier) ? null : offeringIdentifier,
                DisplayCloseButton = displayCloseButton
            };
        }

        private void HandleResult(PaywallResult result)
        {
            if (result == null)
            {
                Debug.LogError("[RevenueCatUI] Received null PaywallResult.");
                OnPaywallResult?.Invoke(new PaywallResult(PaywallResultType.Error));
                return;
            }

            OnPaywallResult?.Invoke(result);
        }

        [Serializable]
        public class PaywallResultEvent : UnityEvent<PaywallResult> { }
    }
}

