#pragma warning disable CS0618 // Checkpoints are experimental on purpose.
using System;
using System.Collections.Generic;
using System.Threading;
using RevenueCat.SimpleJSON;
using UnityEngine;

namespace RevenueCatUI.Internal
{
    internal static class CheckpointBridge
    {
        private sealed class Subscription
        {
            internal Action<CheckpointResult> Callback;
            internal SynchronizationContext Context;
        }

        private static readonly Dictionary<string, Subscription> Subscriptions = new Dictionary<string, Subscription>();
        private static readonly object Gate = new object();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            lock (Gate) Subscriptions.Clear();
        }

        internal static Action Register(string callId, Action<CheckpointResult> callback)
        {
            var context = SynchronizationContext.Current;
            if (context == null)
                throw new InvalidOperationException("Call Checkpoint from the Unity main thread.");
            lock (Gate) Subscriptions.Add(callId, new Subscription { Callback = callback, Context = context });
            return () => { lock (Gate) Subscriptions.Remove(callId); };
        }

        internal static void OnPassed(string callId, string resultJson)
        {
            Subscription subscription;
            lock (Gate)
            {
                if (!Subscriptions.TryGetValue(callId, out subscription)) return;
            }
            subscription.Context.Post(_ =>
            {
                lock (Gate)
                {
                    // Check again after posting: the screen may have detached in the meantime.
                    if (!Subscriptions.Remove(callId)) return;
                }
                try
                {
                    subscription.Callback(new CheckpointResult(JSON.Parse(resultJson)));
                }
                catch (Exception e)
                {
                    Debug.LogError($"[RevenueCatUI] Error handling checkpoint result: {e}");
                }
            }, null);
        }

        internal static void OnError(string callId, string code, string message)
        {
            Subscription subscription;
            lock (Gate)
            {
                if (!Subscriptions.TryGetValue(callId, out subscription)) return;
            }
            subscription.Context.Post(_ =>
            {
                lock (Gate)
                {
                    if (!Subscriptions.Remove(callId)) return;
                }
                Debug.LogError($"[RevenueCatUI] Checkpoint could not be started ({code}): {message}");
            }, null);
        }

#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
        internal static Action Start(string identifier, Action<CheckpointResult> callback, CheckpointParams parameters)
        {
            var callId = Guid.NewGuid().ToString();
            var detach = Register(callId, callback);
            try
            {
#if UNITY_IOS
                rcui_checkpoint(identifier, parameters.CustomVariablesToJsonString(), parameters.NativePresentationMode, callId, NativePassedCallback, NativeErrorCallback);
#elif UNITY_ANDROID
                using (var plugin = new AndroidJavaClass("com.revenuecat.purchasesunity.ui.Checkpoints"))
                    plugin.CallStatic("checkpoint", identifier, parameters.CustomVariablesToJsonString(), parameters.NativePresentationMode, callId, NativeCallbacks);
#endif
            }
            catch
            {
                detach();
                throw;
            }
            return detach;
        }
#endif

#if UNITY_IOS && !UNITY_EDITOR
        private delegate void PassedCallback(string callId, string resultJson);
        private delegate void ErrorCallback(string callId, string code, string message);
        private static readonly PassedCallback NativePassedCallback = NativeOnPassed;
        private static readonly ErrorCallback NativeErrorCallback = NativeOnError;

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void rcui_checkpoint(string identifier, string customVariablesJson, string presentationMode,
            string callId, PassedCallback passedCallback, ErrorCallback errorCallback);

        [AOT.MonoPInvokeCallback(typeof(PassedCallback))]
        private static void NativeOnPassed(string callId, string resultJson) => OnPassed(callId, resultJson);

        [AOT.MonoPInvokeCallback(typeof(ErrorCallback))]
        private static void NativeOnError(string callId, string code, string message) => OnError(callId, code, message);
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
        private static readonly Callbacks NativeCallbacks = new Callbacks();

        private sealed class Callbacks : AndroidJavaProxy
        {
            internal Callbacks() : base("com.revenuecat.purchasesunity.ui.Checkpoints$Callbacks") { }

            [UnityEngine.Scripting.Preserve]
            public void onPassed(string callId, string resultJson) => OnPassed(callId, resultJson);

            [UnityEngine.Scripting.Preserve]
            public void onError(string callId, string code, string message) => OnError(callId, code, message);
        }
#endif
    }
}
