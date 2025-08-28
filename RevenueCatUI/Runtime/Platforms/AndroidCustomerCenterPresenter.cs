#if UNITY_ANDROID && !UNITY_EDITOR
using System;
using System.Threading.Tasks;
using UnityEngine;
using RevenueCat.UI.Internal;

namespace RevenueCat.UI.Platforms
{
    /// <summary>
    /// Android implementation of the customer center presenter.
    /// Uses RevenueCat Hybrid Common UI for Unity integration.
    /// </summary>
    internal class AndroidCustomerCenterPresenter : ICustomerCenterPresenter
    {
        private readonly AndroidJavaClass _plugin;
        private TaskCompletionSource<bool> _currentTask;

        public AndroidCustomerCenterPresenter()
        {
            _plugin = new AndroidJavaClass("com.revenuecat.unity.RevenueCatUIPlugin");
            RevenueCatUICallbackHandler.Initialize();
            RevenueCatUICallbackHandler.SetAndroidCustomerCenterPresenter(this);
        }

        public bool IsSupported()
        {
            try
            {
                return _plugin.CallStatic<bool>("isSupported");
            }
            catch (Exception e)
            {
                Debug.LogError($"RevenueCatUI: Error checking if customer center supported: {e.Message}");
                return false;
            }
        }

        public Task PresentCustomerCenterAsync()
        {
            if (_currentTask != null && !_currentTask.Task.IsCompleted)
            {
                _currentTask.TrySetCanceled();
            }

            _currentTask = new TaskCompletionSource<bool>();

            try
            {
                _plugin.CallStatic("presentCustomerCenter");
            }
            catch (Exception e)
            {
                Debug.LogError($"RevenueCatUI: Error presenting customer center: {e.Message}");
                _currentTask.TrySetResult(false);
            }

            return _currentTask.Task;
        }

        // Called from Java via UnitySendMessage
        public void OnCustomerCenterResult(string resultData)
        {
            if (_currentTask == null || _currentTask.Task.IsCompleted)
                return;

            try
            {
                string[] parts = resultData.Split('|');
                string resultString = parts.Length > 0 ? parts[0] : "ERROR";
                string message = parts.Length > 1 ? parts[1] : "";

                bool success = !resultString.Equals("ERROR", StringComparison.OrdinalIgnoreCase);
                _currentTask.TrySetResult(success);
            }
            catch (Exception e)
            {
                Debug.LogError($"RevenueCatUI: Error processing customer center result: {e.Message}");
                _currentTask.TrySetResult(false);
            }
        }
    }
}
#endif 