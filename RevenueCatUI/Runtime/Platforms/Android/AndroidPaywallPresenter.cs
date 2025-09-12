#if UNITY_ANDROID && !UNITY_EDITOR
using System;
using System.Threading.Tasks;
using UnityEngine;
using RevenueCat.UI.Internal;

namespace RevenueCat.UI.Platforms
{
    internal class AndroidPaywallPresenter : IPaywallPresenter
    {
        private readonly AndroidJavaClass _plugin;
        private TaskCompletionSource<PaywallResult> _current;

        public AndroidPaywallPresenter()
        {
            _plugin = new AndroidJavaClass("com.revenuecat.unity.RevenueCatUI");
            RevenueCatUICallbackHandler.Initialize();
            RevenueCatUICallbackHandler.SetAndroidPresenter(this);
        }

        public bool IsSupported()
        {
            try { return _plugin.CallStatic<bool>("isSupported"); }
            catch { return false; }
        }

        public Task<PaywallResult> PresentPaywallAsync(PaywallOptions options)
        {
            _current = new TaskCompletionSource<PaywallResult>();
            try
            {
                var offering = options?.OfferingIdentifier;
                _plugin.CallStatic("presentPaywall", offering);
            }
            catch (Exception)
            {
                _current.TrySetResult(PaywallResult.Error);
            }
            return _current.Task;
        }

        public Task<PaywallResult> PresentPaywallIfNeededAsync(string requiredEntitlementIdentifier, PaywallOptions options)
        {
            _current = new TaskCompletionSource<PaywallResult>();
            try
            {
                var offering = options?.OfferingIdentifier;
                _plugin.CallStatic("presentPaywallIfNeeded", requiredEntitlementIdentifier, offering);
            }
            catch (Exception)
            {
                _current.TrySetResult(PaywallResult.Error);
            }
            return _current.Task;
        }

        // Called from Java via UnitySendMessage
        public void OnPaywallResult(string resultData)
        {
            if (_current == null) return;
            try
            {
                var token = resultData?.Split('|')[0] ?? "ERROR";
                var type = PaywallResultTypeExtensions.FromNativeString(token);
                _current.TrySetResult(new PaywallResult(type));
            }
            catch
            {
                _current.TrySetResult(PaywallResult.Error);
            }
        }
    }
}
#endif
