#if UNITY_IOS && !UNITY_EDITOR
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace RevenueCat.UI.Platforms
{
    internal class IOSCustomerCenterPresenter : ICustomerCenterPresenter
    {
        private delegate void CustomerCenterCallback();

        [DllImport("__Internal")] private static extern void rcui_presentCustomerCenter(CustomerCenterCallback cb);
        [DllImport("__Internal")] private static extern bool rcui_isSupported();

        private static TaskCompletionSource<bool> s_current;

        public bool IsSupported() => rcui_isSupported();

        public Task PresentCustomerCenterAsync()
        {
            if (s_current != null && !s_current.Task.IsCompleted)
            {
                UnityEngine.Debug.LogWarning("[RevenueCatUI][iOS] Customer Center already in progress; rejecting new request.");
                return Task.CompletedTask;
            }

            var tcs = new TaskCompletionSource<bool>();
            s_current = tcs;
            try
            {
                rcui_presentCustomerCenter(OnDone);
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"[RevenueCatUI][iOS] Exception in presentCustomerCenter: {e.Message}");
                tcs.TrySetResult(false);
                s_current = null;
            }
            return tcs.Task;
        }

        [AOT.MonoPInvokeCallback(typeof(CustomerCenterCallback))]
        private static void OnDone()
        {
            s_current?.TrySetResult(true);
            s_current = null;
        }
    }
}
#endif
