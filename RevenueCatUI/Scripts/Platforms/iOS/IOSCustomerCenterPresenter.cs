#if UNITY_IOS && !UNITY_EDITOR
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using RevenueCatUI.Internal;

namespace RevenueCatUI.Platforms
{
    internal class IOSCustomerCenterPresenter : ICustomerCenterPresenter
    {
        private delegate void CustomerCenterResultCallback(string result);

        [DllImport("__Internal")] private static extern void rcui_presentCustomerCenter(CustomerCenterResultCallback cb);

        private static TaskCompletionSource<bool> s_current;

        public Task PresentAsync(CustomerCenterCallbacks callbacks)
        {
            if (s_current != null && !s_current.Task.IsCompleted)
            {
                UnityEngine.Debug.LogWarning("[RevenueCatUI][iOS] Customer Center presentation already in progress; rejecting new request.");
                return s_current.Task;
            }

            var tcs = new TaskCompletionSource<bool>();
            s_current = tcs;
            try
            {
                rcui_presentCustomerCenter(OnCompleted);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[RevenueCatUI][iOS] Exception in presentCustomerCenter: {e.Message}");
                tcs.TrySetResult(false);
                s_current = null;
            }
            return tcs.Task;
        }

        [AOT.MonoPInvokeCallback(typeof(CustomerCenterResultCallback))]
        private static void OnCompleted(string result)
        {
            try
            {
                var token = (result ?? "ERROR");
                UnityEngine.Debug.Log($"[RevenueCatUI][iOS] Customer Center completed with token '{token}'.");
                s_current?.TrySetResult(true);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[RevenueCatUI][iOS] Failed to handle Customer Center completion: {e.Message}");
                s_current?.TrySetResult(false);
            }
            finally
            {
                s_current = null;
            }
        }
    }
}
#endif
