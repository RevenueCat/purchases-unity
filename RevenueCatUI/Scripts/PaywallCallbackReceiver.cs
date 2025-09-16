using UnityEngine;

namespace RevenueCat.UI
{
    // Ensures there's a GameObject to receive UnitySendMessage callbacks from native code
    // Optional UnitySendMessage fallback receiver. Disabled by default.
    // Use RevenueCatUI.EnableUnityMessageFallback to create/register this receiver.
    [UnityEngine.Scripting.Preserve]
    internal class PaywallCallbackReceiver : MonoBehaviour
    {
        private static string s_receiverName = "RevenueCatUIReceiver";
        private static PaywallCallbackReceiver _instance;

        internal static void EnsureExists(string receiverName = null)
        {
            if (_instance != null) return;
            if (!string.IsNullOrEmpty(receiverName)) s_receiverName = receiverName;

            var go = GameObject.Find(s_receiverName) ?? new GameObject(s_receiverName);
            _instance = go.GetComponent<PaywallCallbackReceiver>() ?? go.AddComponent<PaywallCallbackReceiver>();
            DontDestroyOnLoad(go);
        }

        internal static void Disable()
        {
            if (_instance == null) return;
            var go = _instance.gameObject;
            _instance = null;
            if (go != null)
            {
                Object.Destroy(go);
            }
        }

        // Called via UnitySendMessage from native iOS layer
        // Method name must match exactly
        [UnityEngine.Scripting.Preserve]
        public void _rcuiPaywallResult(string payload)
        {
#if UNITY_IOS && !UNITY_EDITOR
            Platforms.IOSPaywallPresenter.ReceiveResultFromUnityMessage(payload);
#else
            Debug.Log($"[RevenueCatUI] Received paywall result callback on unsupported platform: {payload}");
#endif
        }
    }
}
