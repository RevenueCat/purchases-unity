using System;
using RevenueCatUI.Internal;

namespace RevenueCatUI
{
    /// <summary>Experimental, invite-only checkpoints configured in the RevenueCat dashboard.</summary>
    /// <remarks>
    /// This API is experimental and only available to invited customers. It may change or be
    /// removed in any release, so don't rely on it without talking to RevenueCat first.
    /// Using it produces an obsolete warning at compile time on purpose.
    /// </remarks>
    [System.Obsolete(CheckpointsApi.ExperimentalMessage)]
    public static class Checkpoints
    {
        /// <summary>
        /// Calls onPassed once, on the Unity main thread, only when the app can continue past
        /// the checkpoint. It is not called when the user backs out, a blocking error occurs,
        /// or another checkpoint owns presentation. The SDK presents errors with its native UI.
        /// A checkpoint that cannot start (for example, Purchases is not configured or iOS is older
        /// than 15) logs an error and does not call onPassed.
        /// Must be called from the Unity main thread. Requires iOS 15+ or Android, outside the Editor.
        /// </summary>
        /// <remarks>Experimental and invite-only. This API may change or be removed in any release.</remarks>
        /// <example>
        /// <code>
        /// private Action detachCheckpoint;
        ///
        /// void Upgrade()
        /// {
        ///     detachCheckpoint?.Invoke();
        ///     detachCheckpoint = Checkpoints.Checkpoint("upgrade", result =>
        ///     {
        ///         // Continue the app here. result.Customer has the latest customer info.
        ///         // result.Flow, when present, has ObtainedEntitlements and ObtainedProducts.
        ///     }, new CheckpointParams(
        ///         customVariables: new Dictionary&lt;string, CustomVariableValue&gt;
        ///         {
        ///             { "level", CustomVariableValue.Number(42) }
        ///         },
        ///         presentationMode: FlowPresentationMode.ModalSheet));
        /// }
        ///
        /// void OnDestroy() => detachCheckpoint?.Invoke();
        /// </code>
        /// </example>
        /// <returns>
        /// A function that detaches the callback. Call it when its owning screen is destroyed,
        /// including calls that never pass. Detaching does not dismiss a flow already on screen.
        /// </returns>
        public static Action Checkpoint(string identifier, Action<CheckpointResult> onPassed,
            CheckpointParams parameters = null)
        {
            if (string.IsNullOrEmpty(identifier)) throw new ArgumentException("A checkpoint identifier is required.", nameof(identifier));
            if (onPassed == null) throw new ArgumentNullException(nameof(onPassed));

#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
            return CheckpointBridge.Start(identifier, onPassed, parameters ?? new CheckpointParams());
#else
            throw new PlatformNotSupportedException("Checkpoints require a native iOS or Android build.");
#endif
        }
    }
}
