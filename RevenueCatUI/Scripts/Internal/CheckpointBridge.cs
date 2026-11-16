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
        internal const string CompletePaywallPresentationMethod = "completePaywallPresentation";
        internal const string ResolveErrorDialogMethod = "resolveErrorDialog";
        internal const string DetachCheckpointMethod = "detachCheckpoint";
        internal const string NavigatedBack = "NAVIGATED_BACK";
        internal const string UseDefaultErrorDialog = "{\"type\":\"useDefault\"}";

        private sealed class Subscription
        {
            internal Action<CheckpointResult> Callback;
            internal CheckpointPaywallPresenter Presenter;
            internal SynchronizationContext Context;
            internal string ActivePaywallRequestId;
            internal bool Detached;
            // Only the app's detach makes native answer pending requests itself.
            internal bool DetachedByApp;

            internal void Clear()
            {
                Detached = true;
                Callback = null;
                Presenter = null;
            }
        }

        private static readonly Dictionary<string, Subscription> Subscriptions = new Dictionary<string, Subscription>();
        private static readonly object Gate = new object();

        // Sends a request to the native checkpoint bridge: (method, first argument, second argument).
        // Edit Mode tests replace it to observe what would reach native.
        internal static Action<string, string, string> SendToNative = SendToPlatform;

        // Starts the native checkpoint: (identifier, parameters, callId, useCustomPaywallPresenter).
        internal static Action<string, CheckpointParams, string, bool> StartNative = StartOnPlatform;

        // Set by Checkpoints.SetPaywallPresenter. Each call captures it when it starts.
        internal static CheckpointPaywallPresenter PaywallPresenter;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            lock (Gate) Subscriptions.Clear();
            SendToNative = SendToPlatform;
            StartNative = StartOnPlatform;
            PaywallPresenter = null;
        }

        internal static Action Start(string identifier, Action<CheckpointResult> callback, CheckpointParams parameters)
        {
            var presenter = parameters.UseNativePaywallPresenter ? null : parameters.PaywallPresenter ?? PaywallPresenter;
            var callId = Guid.NewGuid().ToString();
            var detach = Register(callId, callback, presenter);
            try
            {
                StartNative(identifier, parameters, callId, presenter != null);
            }
            catch
            {
                detach();
                throw;
            }
            return detach;
        }

        internal static Action Register(string callId, Action<CheckpointResult> callback,
            CheckpointPaywallPresenter presenter = null)
        {
            var context = SynchronizationContext.Current;
            if (context == null)
                throw new InvalidOperationException("Call Checkpoint from the Unity main thread.");
            var subscription = new Subscription { Callback = callback, Presenter = presenter, Context = context };
            lock (Gate) Subscriptions.Add(callId, subscription);
            return () =>
            {
                string activePaywallRequestId;
                lock (Gate)
                {
                    if (subscription.Detached) return;
                    Subscriptions.Remove(callId);
                    subscription.Clear();
                    subscription.DetachedByApp = true;
                    activePaywallRequestId = subscription.ActivePaywallRequestId;
                }
                // Native keeps a paywall whose presenter already ran: the app still dismisses and completes it.
                SendToNative(DetachCheckpointMethod, callId, activePaywallRequestId);
            };
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
                Action<CheckpointResult> callback;
                lock (Gate)
                {
                    // Check again after posting: the screen may have detached in the meantime.
                    if (!Subscriptions.Remove(callId)) return;
                    callback = subscription.Callback;
                    subscription.Clear();
                }
                try
                {
                    callback(new CheckpointResult(JSON.Parse(resultJson)));
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
                    subscription.Clear();
                }
                Debug.LogError($"[RevenueCatUI] Checkpoint could not be started ({code}): {message}");
            }, null);
        }

        internal static void OnPaywallPresentationRequested(string callId, string requestId, string paramsJson)
        {
            Subscription subscription;
            lock (Gate) Subscriptions.TryGetValue(callId, out subscription);
            if (subscription?.Presenter == null)
            {
                SendToNative(CompletePaywallPresentationMethod, requestId, NavigatedBack);
                return;
            }
            subscription.Context.Post(_ => Present(callId, requestId, paramsJson, subscription), null);
        }

        private static void Present(string callId, string requestId, string paramsJson, Subscription subscription)
        {
            CheckpointPaywallPresenter presenter;
            bool detachedByApp;
            lock (Gate)
            {
                presenter = subscription.Presenter;
                detachedByApp = subscription.DetachedByApp;
                if (presenter != null) subscription.ActivePaywallRequestId = requestId;
            }
            if (presenter == null)
            {
                // The call ended while this request was queued. An app detach already backed it out natively.
                if (!detachedByApp) SendToNative(CompletePaywallPresentationMethod, requestId, NavigatedBack);
                return;
            }

            var completed = 0;
            void Complete(PaywallPresentationResult result)
            {
                if (Interlocked.Exchange(ref completed, 1) == 1) return;
                lock (Gate)
                {
                    if (subscription.ActivePaywallRequestId == requestId) subscription.ActivePaywallRequestId = null;
                    // Backing out ends the call; native won't pass it.
                    if (result == PaywallPresentationResult.NavigatedBack && !subscription.Detached)
                    {
                        Subscriptions.Remove(callId);
                        subscription.Clear();
                    }
                }
                // Reach native from the Unity main thread. Sending right away there keeps the completion
                // ahead of a detach that follows it, so native doesn't back the request out first.
                if (SynchronizationContext.Current == subscription.Context)
                    SendToNative(CompletePaywallPresentationMethod, requestId, result.ToNativeString());
                else
                    subscription.Context.Post(
                        __ => SendToNative(CompletePaywallPresentationMethod, requestId, result.ToNativeString()), null);
            }

            try
            {
                presenter(new PaywallPresentationParams(JSON.Parse(paramsJson)), Complete);
            }
            catch (Exception e)
            {
                Debug.LogError($"[RevenueCatUI] Checkpoint paywall presenter failed: {e}");
                Complete(PaywallPresentationResult.NavigatedBack);
            }
        }

        // Native only requests a dialog when the call has an error-dialog factory. None exists yet, so use the default.
        internal static void OnErrorDialogRequested(string callId, string requestId, string errorJson)
        {
            SendToNative(ResolveErrorDialogMethod, requestId, UseDefaultErrorDialog);
        }

        private static void SendToPlatform(string method, string first, string second)
        {
#if UNITY_IOS && !UNITY_EDITOR
            switch (method)
            {
                case CompletePaywallPresentationMethod: rcui_checkpoint_complete_paywall_presentation(first, second); break;
                case ResolveErrorDialogMethod: rcui_checkpoint_resolve_error_dialog(first, second); break;
                case DetachCheckpointMethod: rcui_checkpoint_detach(first, second); break;
            }
#elif UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (var plugin = new AndroidJavaClass("com.revenuecat.purchasesunity.ui.Checkpoints"))
                    plugin.CallStatic(method, first, second);
            }
            catch (Exception e)
            {
                Debug.LogError($"[RevenueCatUI] Checkpoint {method} failed: {e.Message}");
            }
#endif
        }

#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
        // A reloaded Unity runtime (for example, Unity as a Library) can't receive callbacks for calls started
        // before the reload. Matches the hybrid SDKs, which invalidate when their runtime starts.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InvalidateNativePresentations()
        {
            try
            {
#if UNITY_IOS
                rcui_checkpoint_invalidate();
#else
                using (var plugin = new AndroidJavaClass("com.revenuecat.purchasesunity.ui.Checkpoints"))
                    plugin.CallStatic("invalidate");
#endif
            }
            catch (Exception e)
            {
                Debug.LogError($"[RevenueCatUI] Failed to reset checkpoints: {e.Message}");
            }
        }

#endif

        private static void StartOnPlatform(string identifier, CheckpointParams parameters, string callId,
            bool useCustomPaywallPresenter)
        {
            const bool hasErrorDialogFactory = false;
#if UNITY_IOS && !UNITY_EDITOR
            rcui_checkpoint(identifier, parameters.CustomVariablesToJsonString(), parameters.NativePresentationMode, callId,
                useCustomPaywallPresenter, hasErrorDialogFactory, NativePassedCallback, NativeErrorCallback,
                NativePaywallPresentationCallback, NativeErrorDialogCallback);
#elif UNITY_ANDROID && !UNITY_EDITOR
            using (var plugin = new AndroidJavaClass("com.revenuecat.purchasesunity.ui.Checkpoints"))
                plugin.CallStatic("checkpoint", identifier, parameters.CustomVariablesToJsonString(), parameters.NativePresentationMode, callId,
                    useCustomPaywallPresenter, hasErrorDialogFactory, NativeCallbacks);
#else
            throw new PlatformNotSupportedException("Checkpoints require a native iOS or Android build.");
#endif
        }

#if UNITY_IOS && !UNITY_EDITOR
        private delegate void PassedCallback(string callId, string resultJson);
        private delegate void ErrorCallback(string callId, string code, string message);
        private delegate void RequestCallback(string callId, string requestId, string payloadJson);
        private static readonly PassedCallback NativePassedCallback = NativeOnPassed;
        private static readonly ErrorCallback NativeErrorCallback = NativeOnError;
        private static readonly RequestCallback NativePaywallPresentationCallback = NativeOnPaywallPresentationRequested;
        private static readonly RequestCallback NativeErrorDialogCallback = NativeOnErrorDialogRequested;

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void rcui_checkpoint(string identifier, string customVariablesJson, string presentationMode,
            string callId, bool useCustomPaywallPresenter, bool hasErrorDialogFactory,
            PassedCallback passedCallback, ErrorCallback errorCallback,
            RequestCallback paywallPresentationCallback, RequestCallback errorDialogCallback);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void rcui_checkpoint_complete_paywall_presentation(string requestId, string result);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void rcui_checkpoint_resolve_error_dialog(string requestId, string outcomeJson);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void rcui_checkpoint_detach(string callId, string activePaywallRequestId);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void rcui_checkpoint_invalidate();

        [AOT.MonoPInvokeCallback(typeof(PassedCallback))]
        private static void NativeOnPassed(string callId, string resultJson) => OnPassed(callId, resultJson);

        [AOT.MonoPInvokeCallback(typeof(ErrorCallback))]
        private static void NativeOnError(string callId, string code, string message) => OnError(callId, code, message);

        [AOT.MonoPInvokeCallback(typeof(RequestCallback))]
        private static void NativeOnPaywallPresentationRequested(string callId, string requestId, string paramsJson) =>
            OnPaywallPresentationRequested(callId, requestId, paramsJson);

        [AOT.MonoPInvokeCallback(typeof(RequestCallback))]
        private static void NativeOnErrorDialogRequested(string callId, string requestId, string errorJson) =>
            OnErrorDialogRequested(callId, requestId, errorJson);
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

            [UnityEngine.Scripting.Preserve]
            public void onPaywallPresentationRequested(string callId, string requestId, string paramsJson) =>
                OnPaywallPresentationRequested(callId, requestId, paramsJson);

            [UnityEngine.Scripting.Preserve]
            public void onErrorDialogRequested(string callId, string requestId, string errorJson) =>
                OnErrorDialogRequested(callId, requestId, errorJson);
        }
#endif
    }
}
