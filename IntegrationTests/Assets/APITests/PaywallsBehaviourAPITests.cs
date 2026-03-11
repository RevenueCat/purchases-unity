using RevenueCatUI;
using UnityEngine;
using UnityEngine.Events;

namespace DefaultNamespace
{
    public class PaywallsBehaviourAPITests : MonoBehaviour
    {
        private void Start()
        {
            PaywallsBehaviour behaviour = gameObject.AddComponent<PaywallsBehaviour>();

            string offeringId = behaviour.OfferingIdentifier;
            behaviour.OfferingIdentifier = "test_offering";

            bool closeButton = behaviour.DisplayCloseButton;
            behaviour.DisplayCloseButton = true;

            string entitlementId = behaviour.RequiredEntitlementIdentifier;
            behaviour.RequiredEntitlementIdentifier = "premium";

            UnityEvent onPurchased = behaviour.OnPurchased;
            UnityEvent onRestored = behaviour.OnRestored;
            UnityEvent onCancelled = behaviour.OnCancelled;
            UnityEvent onNotPresented = behaviour.OnNotPresented;
            UnityEvent onError = behaviour.OnError;

            behaviour.PresentPaywall();
        }
    }
}
