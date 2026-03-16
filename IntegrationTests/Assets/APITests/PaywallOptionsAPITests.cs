using System.Collections.Generic;
using UnityEngine;
using RevenueCatUI;

namespace DefaultNamespace
{
    public class PaywallOptionsAPITests : MonoBehaviour
    {
        private void Start()
        {
            // Test PaywallOptions with no parameters
            PaywallOptions options1 = new PaywallOptions();

            // Test PaywallOptions with displayCloseButton only
            PaywallOptions options2 = new PaywallOptions(displayCloseButton: true);

            // Test PaywallOptions with customVariables only
            PaywallOptions options3 = new PaywallOptions(
                customVariables: new Dictionary<string, CustomVariableValue>
                {
                    { "player_name", CustomVariableValue.String("John") },
                    { "level", CustomVariableValue.Number(5) },
                    { "is_premium", CustomVariableValue.Boolean(true) }
                }
            );

            // Test PaywallOptions with displayCloseButton and customVariables
            PaywallOptions options4 = new PaywallOptions(
                displayCloseButton: true,
                customVariables: new Dictionary<string, CustomVariableValue>
                {
                    { "player_name", CustomVariableValue.String("Jane") },
                    { "coins", CustomVariableValue.Number(1000) },
                    { "has_subscription", CustomVariableValue.Boolean(false) }
                }
            );

            // Test PaywallOptions with offering
            Purchases.Offering offering = null;
            PaywallOptions options5 = new PaywallOptions(offering: offering);

            // Test PaywallOptions with offering and displayCloseButton
            PaywallOptions options6 = new PaywallOptions(
                offering: offering,
                displayCloseButton: true
            );

            // Test PaywallOptions with offering and customVariables
            PaywallOptions options7 = new PaywallOptions(
                offering: offering,
                customVariables: new Dictionary<string, CustomVariableValue>
                {
                    { "coins", CustomVariableValue.Number(1000) }
                }
            );

            // Test PaywallOptions with all parameters
            PaywallOptions options8 = new PaywallOptions(
                offering: offering,
                displayCloseButton: true,
                customVariables: new Dictionary<string, CustomVariableValue>
                {
                    { "player_name", CustomVariableValue.String("Test") },
                    { "level", CustomVariableValue.Number(42) },
                    { "coins_balance", CustomVariableValue.Number(9999) }
                }
            );

            // Test PaywallOptions with presentationConfiguration
            PaywallOptions options9 = new PaywallOptions(
                presentationConfiguration: PaywallPresentationConfiguration.FullScreen
            );

            // Test PaywallOptions with presentationConfiguration and customVariables
            PaywallOptions options10 = new PaywallOptions(
                displayCloseButton: true,
                customVariables: new Dictionary<string, CustomVariableValue>
                {
                    { "player_name", CustomVariableValue.String("Test") },
                    { "is_vip", CustomVariableValue.Boolean(true) }
                },
                presentationConfiguration: PaywallPresentationConfiguration.FullScreen
            );

            // Test PaywallOptions with offering, customVariables, and presentationConfiguration
            PaywallOptions options11 = new PaywallOptions(
                offering: offering,
                displayCloseButton: true,
                customVariables: new Dictionary<string, CustomVariableValue>
                {
                    { "level", CustomVariableValue.Number(99) }
                },
                presentationConfiguration: PaywallPresentationConfiguration.FullScreen
            );

            // Test positional argument compatibility (must not break existing callers)
            PaywallOptions positional1 = new PaywallOptions(true);
            PaywallOptions positional2 = new PaywallOptions(true, PaywallPresentationConfiguration.FullScreen);
            PaywallOptions positional3 = new PaywallOptions(offering, true);
            PaywallOptions positional4 = new PaywallOptions(offering, true, PaywallPresentationConfiguration.FullScreen);

            // Test CustomVariableValue factory methods
            CustomVariableValue stringValue = CustomVariableValue.String("test");
            CustomVariableValue numberValue = CustomVariableValue.Number(42);
            CustomVariableValue booleanValue = CustomVariableValue.Boolean(true);
        }
    }
}
