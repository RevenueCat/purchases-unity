using System;
using RevenueCatUI.Internal;

namespace RevenueCatUI
{
    /// <summary>Experimental, invite-only checkpoints configured in the RevenueCat dashboard.</summary>
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
