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

            PaywallsBehaviour.CustomVariableType stringType = PaywallsBehaviour.CustomVariableType.String;
            PaywallsBehaviour.CustomVariableType numberType = PaywallsBehaviour.CustomVariableType.Number;
            PaywallsBehaviour.CustomVariableType booleanType = PaywallsBehaviour.CustomVariableType.Boolean;

            PaywallsBehaviour.CustomVariableEntry entry = new PaywallsBehaviour.CustomVariableEntry();
            entry.key = "player_name";
            entry.type = stringType;
            entry.value = "John";

            PaywallsBehaviour.CustomVariableEntry numberEntry = new PaywallsBehaviour.CustomVariableEntry();
            numberEntry.key = "level";
            numberEntry.type = numberType;
            numberEntry.value = "42";

            PaywallsBehaviour.CustomVariableEntry boolEntry = new PaywallsBehaviour.CustomVariableEntry();
            boolEntry.key = "is_premium";
            boolEntry.type = booleanType;
            boolEntry.value = "true";

            behaviour.PresentPaywall();
        }
    }
}
