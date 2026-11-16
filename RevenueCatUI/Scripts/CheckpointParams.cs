using RevenueCatUI.Internal;
using System.Collections.Generic;

namespace RevenueCatUI
{
    /// <summary>Experimental, invite-only. How the SDK presents the flow selected by a checkpoint.</summary>
    [System.Obsolete(CheckpointsApi.ExperimentalMessage)]
    public enum FlowPresentationMode
    {
        Default,
        ModalFullScreen,
        ModalSheet
    }

    /// <summary>Per-call options for the experimental, invite-only checkpoints API. It may change or be removed in any release.</summary>
    [System.Obsolete(CheckpointsApi.ExperimentalMessage)]
    public class CheckpointParams
    {
        /// <summary>Values used in targeting rules and in the presented paywall.</summary>
        public Dictionary<string, CustomVariableValue> CustomVariables { get; }

        /// <summary>How the SDK presents the selected flow.</summary>
        public FlowPresentationMode PresentationMode { get; }

        /// <summary>
        /// Presents the selected offering with app-owned UI. Null uses the presenter set with
        /// <see cref="Checkpoints.SetPaywallPresenter"/> when the checkpoint is called, if any.
        /// </summary>
        public CheckpointPaywallPresenter PaywallPresenter { get; }

        /// <summary>
        /// Uses the SDK's presenter even when a global presenter is set. Takes precedence over <see cref="PaywallPresenter"/>.
        /// </summary>
        public bool UseNativePaywallPresenter { get; }

        public CheckpointParams(Dictionary<string, CustomVariableValue> customVariables = null,
            FlowPresentationMode presentationMode = FlowPresentationMode.Default,
            CheckpointPaywallPresenter paywallPresenter = null,
            bool useNativePaywallPresenter = false)
        {
            CustomVariables = customVariables;
            PresentationMode = presentationMode;
            PaywallPresenter = paywallPresenter;
            UseNativePaywallPresenter = useNativePaywallPresenter;
        }

        internal string CustomVariablesToJsonString() =>
            new PaywallOptions(customVariables: CustomVariables).CustomVariablesToJsonString();

        internal string NativePresentationMode
        {
            get
            {
                switch (PresentationMode)
                {
                    case FlowPresentationMode.ModalFullScreen: return "MODAL_FULL_SCREEN";
                    case FlowPresentationMode.ModalSheet: return "MODAL_SHEET";
                    default: return "DEFAULT";
                }
            }
        }
    }
}
