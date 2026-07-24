using UnityEngine;
using RevenueCatUI;

namespace DefaultNamespace
{
    public class PaywallListenerAPITests : MonoBehaviour
    {
        private void Start()
        {
            // Test PaywallListener with no callbacks
            PaywallListener emptyListener = new PaywallListener();

            // Test PaywallListener with all callbacks assigned via object initializer
            PaywallListener listener = new PaywallListener
            {
                OnPurchaseStarted = package => { Purchases.Package p = package; },
                OnPurchaseCompleted = (customerInfo, storeTransaction) =>
                {
                    Purchases.CustomerInfo info = customerInfo;
                    Purchases.StoreTransaction transaction = storeTransaction;
                },
                OnPurchaseError = error => { Purchases.Error e = error; },
                OnPurchaseCancelled = () => { },
                OnRestoreStarted = () => { },
                OnRestoreCompleted = customerInfo => { Purchases.CustomerInfo info = customerInfo; },
                OnRestoreError = error => { Purchases.Error e = error; }
            };

            // Test property setters and getters
            listener.OnPurchaseCancelled = null;
            System.Action<Purchases.Package> onPurchaseStarted = listener.OnPurchaseStarted;
            System.Action<Purchases.CustomerInfo, Purchases.StoreTransaction> onPurchaseCompleted = listener.OnPurchaseCompleted;
            System.Action<Purchases.Error> onPurchaseError = listener.OnPurchaseError;
            System.Action onPurchaseCancelled = listener.OnPurchaseCancelled;
            System.Action onRestoreStarted = listener.OnRestoreStarted;
            System.Action<Purchases.CustomerInfo> onRestoreCompleted = listener.OnRestoreCompleted;
            System.Action<Purchases.Error> onRestoreError = listener.OnRestoreError;

            // Test PaywallOptions with listener
            PaywallOptions options1 = new PaywallOptions(listener: listener);

            // Test PaywallOptions with offering and listener
            Purchases.Offering offering = null;
            PaywallOptions options2 = new PaywallOptions(
                offering: offering,
                displayCloseButton: true,
                listener: listener
            );

            // Test PaywallOptions with all parameters including listener
            PaywallOptions options3 = new PaywallOptions(
                offering: offering,
                displayCloseButton: true,
                presentationConfiguration: PaywallPresentationConfiguration.FullScreen,
                customVariables: new System.Collections.Generic.Dictionary<string, CustomVariableValue>
                {
                    { "level", CustomVariableValue.Number(1) }
                },
                listener: listener
            );
        }
    }
}
