using System.Collections.Generic;

namespace RevenueCatUI
{
    /// <summary>How the SDK presents the flow selected by a checkpoint.</summary>
    public enum FlowPresentationMode
    {
        Default,
        ModalFullScreen,
        ModalSheet
    }

    /// <summary>Per-call options for the experimental, invite-only checkpoints API.</summary>
    public class CheckpointParams
    {
        /// <summary>Values used in targeting rules and in the presented paywall.</summary>
        public Dictionary<string, CustomVariableValue> CustomVariables { get; }

        /// <summary>How the SDK presents the selected flow.</summary>
        public FlowPresentationMode PresentationMode { get; }

        public CheckpointParams(Dictionary<string, CustomVariableValue> customVariables = null,
            FlowPresentationMode presentationMode = FlowPresentationMode.Default)
        {
            CustomVariables = customVariables;
            PresentationMode = presentationMode;
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
