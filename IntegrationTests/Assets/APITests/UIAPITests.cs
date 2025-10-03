using System;
using UnityEngine;
using RevenueCat;
using RevenueCat.UI;

/// <summary>
/// Integration tests for RevenueCat UI APIs to ensure compatibility and availability.
/// These tests verify that the API surface is accessible and basic object creation works.
/// </summary>
public class UIAPITests : MonoBehaviour
{
    private void Start()
    {
        // Test PaywallOptions API surface
        PaywallOptions options1 = new PaywallOptions();
        PaywallOptions options2 = new PaywallOptions("premium_monthly");
        PaywallOptions options3 = new PaywallOptions("premium_yearly", true);
        
        // Test property access
        string offeringId = options1.OfferingIdentifier;
        bool displayCloseButton = options1.DisplayCloseButton;
        
        // Test property modification
        options1.OfferingIdentifier = "test_offering";
        options1.DisplayCloseButton = true;

        // Test PaywallResult API surface
        PaywallResult result1 = new PaywallResult(PaywallResultType.Purchased);
        PaywallResult result2 = new PaywallResult(PaywallResultType.Cancelled);
        
        // Test enum property access
        PaywallResultType resultType = result1.Result;
        
        // Test ToString
        string resultString = result1.ToString();

        // Test PaywallResultType extensions
        string nativeString = PaywallResultType.Purchased.ToNativeString();
        PaywallResultType fromNative = PaywallResultTypeExtensions.FromNativeString("PURCHASED");
        
        // Test all enum values can be converted
        PaywallResultType[] allTypes = {
            PaywallResultType.NotPresented,
            PaywallResultType.Cancelled,
            PaywallResultType.Error,
            PaywallResultType.Purchased,
            PaywallResultType.Restored
        };
        
        foreach (var type in allTypes)
        {
            string native = type.ToNativeString();
            PaywallResultType roundTrip = PaywallResultTypeExtensions.FromNativeString(native);
        }

        // Test UIPresentation static API
        bool isSupported = UIPresentation.IsSupported();
        
        // Test async method signatures (don't await in Start)
        var paywallTask = UIPresentation.PresentPaywallAsync();
        var paywallWithOptionsTask = UIPresentation.PresentPaywallAsync(options1);
        
        var paywallIfNeededTask = UIPresentation.PresentPaywallIfNeededAsync("premium");
        var paywallIfNeededWithOptionsTask = UIPresentation.PresentPaywallIfNeededAsync("premium", options1);

        // Test UIBehaviour MonoBehaviour
        UIBehaviour uiBehaviour = gameObject.AddComponent<UIBehaviour>();
        
        // Test UIBehaviour API surface
        bool behaviourSupported = uiBehaviour.IsSupported();
        var behaviourPaywallTask = uiBehaviour.PresentPaywall();
        var behaviourPaywallWithOptionsTask = uiBehaviour.PresentPaywall(options1);
        var behaviourPaywallIfNeededTask = uiBehaviour.PresentPaywallIfNeeded("premium");
        var behaviourPaywallIfNeededWithOptionsTask = uiBehaviour.PresentPaywallIfNeeded("premium", options1);

        Debug.Log("[UIAPITests] All UI API tests completed successfully");
    }
}