using System;
using System.Threading;
using UnityEngine;
using RevenueCat.SimpleJSON;

namespace RevenueCatUI.Internal
{
    /// <summary>
    /// Internal bridge dispatching paywall listener events from native to the user's
    /// PaywallListener. Both platforms deliver events through a single generic
    /// (eventName, payloadJson) channel so new events can be added without bridge changes.
    ///
    /// Native callbacks arrive without Unity's SynchronizationContext set (AndroidJavaProxy
    /// on Android, C function pointers on iOS), so the context is captured at present-time
    /// and events are Posted through it to run on the Unity main thread.
    /// </summary>
    internal static class PaywallListenerBridge
    {
        private const string EventPurchaseStarted = "onPurchaseStarted";
        private const string EventPurchaseCompleted = "onPurchaseCompleted";
        private const string EventPurchaseError = "onPurchaseError";
        private const string EventPurchaseCancelled = "onPurchaseCancelled";
        private const string EventRestoreStarted = "onRestoreStarted";
        private const string EventRestoreCompleted = "onRestoreCompleted";
        private const string EventRestoreError = "onRestoreError";

        private static PaywallListener s_currentListener;
        private static SynchronizationContext s_mainThreadContext;

        internal static bool HasListener => s_currentListener != null;

        internal static void SetCurrentListener(PaywallListener listener)
        {
            s_currentListener = listener;
            // Capture Unity's main thread SynchronizationContext.
            // This is called from the presenter on the main thread.
            s_mainThreadContext = SynchronizationContext.Current;
        }

        internal static void ClearCurrentListener()
        {
            s_currentListener = null;
        }

        /// <summary>
        /// Called from native for each paywall event. Posts to the Unity main thread.
        /// The listener is captured here (on the native thread, before the terminal result
        /// clears it) so events still queued when the presentation ends are not dropped.
        /// </summary>
        internal static void OnPaywallEvent(string eventName, string payloadJson)
        {
            if (string.IsNullOrEmpty(eventName)) return;

            var listener = s_currentListener;
            if (listener == null) return;

            if (s_mainThreadContext != null)
            {
                s_mainThreadContext.Post(_ => DispatchEvent(listener, eventName, payloadJson), null);
            }
            else
            {
                DispatchEvent(listener, eventName, payloadJson);
            }
        }

        private static void DispatchEvent(PaywallListener listener, string eventName, string payloadJson)
        {
            try
            {
                DispatchEventToListener(listener, eventName, payloadJson);
            }
            finally
            {
                if (IsTerminalEvent(eventName))
                {
                    NotifyTerminalEventProcessed();
                }
            }
        }

        private static void DispatchEventToListener(PaywallListener listener, string eventName, string payloadJson)
        {
            try
            {
                var payload = string.IsNullOrEmpty(payloadJson) ? null : JSON.Parse(payloadJson);
                switch (eventName)
                {
                    case EventPurchaseStarted:
                        listener.OnPurchaseStarted?.Invoke(new Purchases.Package(payload["package"]));
                        break;
                    case EventPurchaseCompleted:
                        var transactionNode = payload["storeTransaction"];
                        var transaction = transactionNode == null || transactionNode.IsNull
                            ? null
                            : new Purchases.StoreTransaction(transactionNode);
                        listener.OnPurchaseCompleted?.Invoke(new Purchases.CustomerInfo(payload["customerInfo"]), transaction);
                        break;
                    case EventPurchaseError:
                        listener.OnPurchaseError?.Invoke(new Purchases.Error(payload["error"]));
                        break;
                    case EventPurchaseCancelled:
                        listener.OnPurchaseCancelled?.Invoke();
                        break;
                    case EventRestoreStarted:
                        listener.OnRestoreStarted?.Invoke();
                        break;
                    case EventRestoreCompleted:
                        listener.OnRestoreCompleted?.Invoke(new Purchases.CustomerInfo(payload["customerInfo"]));
                        break;
                    case EventRestoreError:
                        listener.OnRestoreError?.Invoke(new Purchases.Error(payload["error"]));
                        break;
                    default:
                        Debug.LogWarning($"[RevenueCatUI] Unknown paywall event '{eventName}'; ignoring.");
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[RevenueCatUI] Error handling paywall event '{eventName}': {e.Message}");
            }
        }

        private static bool IsTerminalEvent(string eventName)
        {
            return eventName == EventPurchaseCompleted
                || eventName == EventPurchaseError
                || eventName == EventPurchaseCancelled
                || eventName == EventRestoreCompleted
                || eventName == EventRestoreError;
        }

        /// <summary>
        /// Acknowledges to native that a terminal event has been dispatched.
        /// On Android this clears FLAG_NOT_FOCUSABLE on the paywall dialog, which is kept
        /// set during a purchase/restore so Unity's window regains focus (and its player
        /// loop resumes) when the billing activity finishes. The clear is deferred to this
        /// acknowledgement so the dialog does not take focus back before Unity has pumped
        /// the queued events.
        /// </summary>
        private static void NotifyTerminalEventProcessed()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using var cls = new AndroidJavaClass("com.revenuecat.purchasesunity.ui.RevenueCatUI");
                cls.CallStatic("notifyPaywallListenerEventProcessed");
            }
            catch (Exception e)
            {
                Debug.LogError($"[RevenueCatUI] Failed to notify paywall event processed: {e.Message}");
            }
#endif
        }
    }
}
