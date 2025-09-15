#if UNITY_ANDROID && !UNITY_EDITOR
using System;
using System.Threading.Tasks;
using UnityEngine;
using RevenueCat.UI.Internal;

namespace RevenueCat.UI.Platforms
{
    internal class AndroidCustomerCenterPresenter : ICustomerCenterPresenter
    {
        private readonly AndroidJavaClass _plugin;
        private TaskCompletionSource<bool> _current;

        public AndroidCustomerCenterPresenter()
        {
            _plugin = new AndroidJavaClass("com.revenuecat.unity.RevenueCatUI");
            RevenueCatUICallbackHandler.Initialize();
            RevenueCatUICallbackHandler.SetAndroidCustomerCenterPresenter(this);
        }

        public bool IsSupported()
        {
            try { return _plugin.CallStatic<bool>("isSupported"); }
            catch { return false; }
        }

        public Task PresentCustomerCenterAsync()
        {
            _current = new TaskCompletionSource<bool>();
            try { _plugin.CallStatic("presentCustomerCenter"); }
            catch { _current.TrySetResult(false); }
            return _current.Task;
        }

        // Called from Java via UnitySendMessage
        public void OnCustomerCenterResult(string resultData)
        {
            _current?.TrySetResult(true);
        }
    }
}
#endif
