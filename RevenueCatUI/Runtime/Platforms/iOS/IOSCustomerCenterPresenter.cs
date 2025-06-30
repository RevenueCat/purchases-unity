#if UNITY_IOS && !UNITY_EDITOR
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;
using RevenueCat.UI;

namespace RevenueCat.UI.Platforms
{
    internal class IOSCustomerCenterPresenter : ICustomerCenterPresenter
    {
        #region DLL Imports
        
        [DllImport("__Internal")]
        private static extern void initializeRevenueCatUI();
        
        [DllImport("__Internal")]
        private static extern void presentCustomerCenter(CustomerCenterResultCallback callback);
        
        [DllImport("__Internal")]
        private static extern bool isRevenueCatUISupported();
        
        private delegate void CustomerCenterResultCallback();
        
        #endregion

        private TaskCompletionSource<bool> currentCustomerCenterTcs;
        private static TaskCompletionSource<bool> staticCurrentCustomerCenterTcs;

        public IOSCustomerCenterPresenter()
        {
            // Initialize the native iOS bridge
            initializeRevenueCatUI();
        }

        public bool IsSupported()
        {
            return isRevenueCatUISupported();
        }

        public async Task PresentCustomerCenterAsync()
        {
            if (currentCustomerCenterTcs != null && !currentCustomerCenterTcs.Task.IsCompleted)
            {
                Debug.LogWarning("[RevenueCatUI] Customer center is already being presented. Cancelling previous request.");
                currentCustomerCenterTcs.TrySetCanceled();
            }

            currentCustomerCenterTcs = new TaskCompletionSource<bool>();
            staticCurrentCustomerCenterTcs = currentCustomerCenterTcs; // Store for static callback

            try
            {
                Debug.Log("[RevenueCatUI] Presenting customer center");
                
                presentCustomerCenter(OnCustomerCenterResult);

                await currentCustomerCenterTcs.Task;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RevenueCatUI] Error presenting customer center: {ex.Message}");
                currentCustomerCenterTcs?.TrySetException(ex);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(CustomerCenterResultCallback))]
        private static void OnCustomerCenterResult()
        {
            Debug.Log("[RevenueCatUI] Customer center dismissed");
            
            var currentTcs = staticCurrentCustomerCenterTcs;
            if (currentTcs != null)
            {
                try
                {
                    currentTcs.TrySetResult(true);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[RevenueCatUI] Error handling customer center result: {ex.Message}");
                    currentTcs.TrySetResult(false);
                }
                finally
                {
                    staticCurrentCustomerCenterTcs = null; // Clear after handling
                }
            }
        }
    }
}
#endif 