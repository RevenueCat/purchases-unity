#pragma warning disable CS0618 // Checkpoints are experimental on purpose.
using System;
using System.Collections.Generic;
using RevenueCat.SimpleJSON;
using RevenueCatUI.Internal;

namespace RevenueCatUI
{
    /// <summary>
    /// Experimental, invite-only. Presents a checkpoint-selected offering with app-owned UI.
    /// Dismiss that UI before calling <paramref name="completion"/>. Completion is honored at most once.
    /// Throwing from the presenter backs out without passing the checkpoint.
    /// </summary>
    /// <remarks>
    /// Called on the Unity main thread. Purchase through the offering's packages as usual: the native SDK
    /// synchronizes purchases after completion and only then passes the checkpoint.
    /// </remarks>
    [System.Obsolete(CheckpointsApi.ExperimentalMessage)]
    public delegate void CheckpointPaywallPresenter(PaywallPresentationParams parameters,
        Action<PaywallPresentationResult> completion);

    /// <summary>Experimental, invite-only. How an app-owned checkpoint paywall ended.</summary>
    [System.Obsolete(CheckpointsApi.ExperimentalMessage)]
    public enum PaywallPresentationResult
    {
        /// <summary>Continue after the native SDK synchronizes purchases.</summary>
        Continued,

        /// <summary>The user closed the paywall. Continue after purchase synchronization.</summary>
        Closed,

        /// <summary>The user navigated back. Stop without calling the checkpoint callback.</summary>
        NavigatedBack
    }

    /// <summary>Experimental, invite-only. What an app-owned checkpoint paywall should present.</summary>
    [System.Obsolete(CheckpointsApi.ExperimentalMessage)]
    public class PaywallPresentationParams
    {
        /// <summary>The identifier passed to <see cref="Checkpoints.Checkpoint"/>.</summary>
        public string CheckpointIdentifier { get; }

        /// <summary>The checkpoint's custom variables, for rendering in the paywall.</summary>
        public IReadOnlyDictionary<string, CustomVariableValue> CustomVariables { get; }

        /// <summary>The presentation mode resolved for this flow.</summary>
        public FlowPresentationMode PresentationMode { get; }

        /// <summary>The offering to present. Purchase its packages to keep the offering context.</summary>
        public Purchases.Offering Offering { get; }

        internal PaywallPresentationParams(JSONNode response)
        {
            if (!response["checkpointIdentifier"].IsString || !response["offering"].IsObject)
                throw new ArgumentException("Malformed checkpoint paywall parameters.");
            CheckpointIdentifier = response["checkpointIdentifier"];
            Offering = new Purchases.Offering(response["offering"]);

            var variables = new Dictionary<string, CustomVariableValue>();
            foreach (var entry in response["customVariables"])
            {
                if (entry.Value.IsString) variables[entry.Key] = CustomVariableValue.String(entry.Value.Value);
                else if (entry.Value.IsNumber) variables[entry.Key] = CustomVariableValue.Number(entry.Value.AsDouble);
                else if (entry.Value.IsBoolean) variables[entry.Key] = CustomVariableValue.Boolean(entry.Value.AsBool);
            }
            CustomVariables = variables;

            switch (response["presentationMode"].Value)
            {
                case "MODAL_FULL_SCREEN": PresentationMode = FlowPresentationMode.ModalFullScreen; break;
                case "MODAL_SHEET": PresentationMode = FlowPresentationMode.ModalSheet; break;
                default: PresentationMode = FlowPresentationMode.Default; break;
            }
        }
    }

    internal static class PaywallPresentationResultExtensions
    {
        internal static string ToNativeString(this PaywallPresentationResult result)
        {
            switch (result)
            {
                case PaywallPresentationResult.Continued: return "CONTINUED";
                case PaywallPresentationResult.Closed: return "CLOSED";
                default: return CheckpointBridge.NavigatedBack;
            }
        }
    }
}
