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
            s_current = new TaskCompletionSource<bool>();
            rcui_presentCustomerCenter(OnDone);
            return s_current.Task;
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
