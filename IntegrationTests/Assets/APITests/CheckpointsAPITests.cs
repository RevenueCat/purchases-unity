#pragma warning disable CS0618 // Checkpoints are experimental on purpose.
using System;
using System.Collections.Generic;
using RevenueCatUI;
using UnityEngine;

namespace DefaultNamespace
{
    public class CheckpointsAPITests : MonoBehaviour
    {
        private void CheckpointAPI()
        {
            var parameters = new CheckpointParams(
                customVariables: new Dictionary<string, CustomVariableValue> { { "source", CustomVariableValue.String("unity") } },
                presentationMode: FlowPresentationMode.ModalSheet);
            Dictionary<string, CustomVariableValue> variables = parameters.CustomVariables;
            FlowPresentationMode mode = parameters.PresentationMode;
            Action detach = Checkpoints.Checkpoint("upgrade", result =>
            {
                Purchases.CustomerInfo customer = result.Customer;
                FlowResult flow = result.Flow;
                if (flow == null) return;
                IReadOnlyList<ObtainedEntitlement> entitlements = flow.ObtainedEntitlements;
                IReadOnlyList<ObtainedProduct> products = flow.ObtainedProducts;
                foreach (var entitlement in entitlements) { Purchases.EntitlementInfo info = entitlement.EntitlementInfo; }
                foreach (var product in products) { Purchases.StoreProduct info = product.Product; }
            }, parameters);
            detach();
            Action detachDefault = Checkpoints.Checkpoint("upgrade", result => { });
            detachDefault();

            CheckpointPaywallPresenter presenter = (PaywallPresentationParams presentation, Action<PaywallPresentationResult> completion) =>
            {
                string identifier = presentation.CheckpointIdentifier;
                IReadOnlyDictionary<string, CustomVariableValue> customVariables = presentation.CustomVariables;
                FlowPresentationMode presentationMode = presentation.PresentationMode;
                Purchases.Offering offering = presentation.Offering;
                completion(PaywallPresentationResult.Continued);
                completion(PaywallPresentationResult.Closed);
                completion(PaywallPresentationResult.NavigatedBack);
            };
            Checkpoints.SetPaywallPresenter(presenter);
            Checkpoints.SetPaywallPresenter(null);
            var presenterParameters = new CheckpointParams(paywallPresenter: presenter, useNativePaywallPresenter: true);
            CheckpointPaywallPresenter paywallPresenter = presenterParameters.PaywallPresenter;
            bool useNativePaywallPresenter = presenterParameters.UseNativePaywallPresenter;
        }
    }
}
