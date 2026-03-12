using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace RevenueCatUI
{
    /// <summary>
    /// MonoBehaviour component for presenting RevenueCat Paywalls from the Unity Editor.
    /// Provides an alternative to PaywallsPresenter for developers who prefer configuring
    /// Paywalls through Unity's Inspector interface.
    /// </summary>
    [AddComponentMenu("RevenueCat/Paywalls Behaviour")]
    public class PaywallsBehaviour : MonoBehaviour
    {
        [Header("Paywall Options")]
        [Tooltip("The identifier of the offering to present. Leave empty to use the current offering.")]
        [SerializeField] private string offeringIdentifier;
        
        [Tooltip("Whether to display a close button on the paywall (only for original template RevenueCat Paywalls).")]
        [SerializeField] private bool displayCloseButton = false;

        [Header("Custom Variables (V2 Paywalls only)")]
        [Tooltip("Custom variables for text substitution using {{ custom.variable_name }} syntax.")]
        [SerializeField] private CustomVariableEntry[] customVariables = Array.Empty<CustomVariableEntry>();

        [Header("Conditional Presentation")]
        [Tooltip("If set, the paywall will only be presented if the user doesn't have this entitlement.")]
        [SerializeField] private string requiredEntitlementIdentifier;

        [Header("Events")]
        [Tooltip("Invoked when the user completes a purchase and the paywall is dismissed.")]
        public UnityEvent OnPurchased = new UnityEvent();
        
        [Tooltip("Invoked when the user restores purchases and the paywall is dismissed.")]
        public UnityEvent OnRestored = new UnityEvent();
        
        [Tooltip("Invoked when the user cancels the paywall and the paywall is dismissed.")]
        public UnityEvent OnCancelled = new UnityEvent();
        
        [Tooltip("Invoked when the paywall was not presented, for example when the user already has the required entitlement).")]
        public UnityEvent OnNotPresented = new UnityEvent();
        
        [Tooltip("Invoked when an error occurs.")]
        public UnityEvent OnError = new UnityEvent();

        public enum CustomVariableType
        {
            String,
            // Number,
            // Boolean,
            // Date,
        }

        [Serializable]
        public struct CustomVariableEntry
        {
            public string key;
            public CustomVariableType type;
            public string value;
        }

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

        private async void PresentPaywallIfNeeded(string entitlementIdentifier)
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

        private PaywallOptions CreateOptions()
        {
            var vars = GetCustomVariablesDictionary();
            if (string.IsNullOrEmpty(offeringIdentifier))
            {
                return new PaywallOptions(displayCloseButton, customVariables: vars);
            }

            return new PaywallOptions(offeringIdentifier, displayCloseButton, customVariables: vars);
        }

        private Dictionary<string, CustomVariableValue> GetCustomVariablesDictionary()
        {
            if (customVariables == null || customVariables.Length == 0) return null;

            var dict = new Dictionary<string, CustomVariableValue>();
            foreach (var entry in customVariables)
            {
                if (string.IsNullOrEmpty(entry.key)) continue;

                dict[entry.key] = entry.type switch
                {
                    // TODO: uncomment when CustomVariableValue supports these types
                    // CustomVariableType.Number when double.TryParse(entry.value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var n)
                    //     => CustomVariableValue.Number(n),
                    // CustomVariableType.Boolean when bool.TryParse(entry.value, out var b)
                    //     => CustomVariableValue.Boolean(b),
                    // CustomVariableType.Date when System.DateTime.TryParse(entry.value, out var d)
                    //     => CustomVariableValue.Date(d),
                    _ => CustomVariableValue.String(entry.value ?? "")
                };
            }
            return dict.Count > 0 ? dict : null;
        }

        private void HandleResult(PaywallResult result)
        {
            if (result == null)
            {
                Debug.LogWarning("[RevenueCatUI] Received null PaywallResult.");
                OnError?.Invoke();
                return;
            }

            switch (result.Result)
            {
                case PaywallResultType.Purchased:
                    if (OnPurchased.GetPersistentEventCount() == 0)
                    {
                        Debug.LogWarning("[RevenueCatUI] Paywall purchase completed but OnPurchased event has no listeners.");
                    }
                    OnPurchased?.Invoke();
                    break;
                case PaywallResultType.Restored:
                    if (OnRestored.GetPersistentEventCount() == 0)
                    {
                        Debug.LogWarning("[RevenueCatUI] Paywall restore completed but OnRestored event has no listeners.");
                    }
                    OnRestored?.Invoke();
                    break;
                case PaywallResultType.Cancelled:
                    if (OnCancelled.GetPersistentEventCount() == 0)
                    {
                        Debug.LogWarning("[RevenueCatUI] Paywall cancelled but OnCancelled event has no listeners.");
                    }
                    OnCancelled?.Invoke();
                    break;
                case PaywallResultType.NotPresented:
                    if (OnNotPresented.GetPersistentEventCount() == 0)
                    {
                        Debug.LogWarning("[RevenueCatUI] Paywall not presented but OnNotPresented event has no listeners.");
                    }
                    OnNotPresented?.Invoke();
                    break;
                case PaywallResultType.Error:
                    if (OnError.GetPersistentEventCount() == 0)
                    {
                        Debug.LogWarning("[RevenueCatUI] Paywall error occurred but OnError event has no listeners.");
                    }
                    OnError?.Invoke();
                    break;
            }
        }
    }
}

