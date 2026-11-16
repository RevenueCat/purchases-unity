#pragma warning disable CS0618 // Checkpoints are experimental on purpose.
using System;
using System.Threading.Tasks;
using RevenueCat.SimpleJSON;
using RevenueCatUI.Internal;

namespace RevenueCatUI
{
    /// <summary>
    /// Experimental, invite-only. Chooses how the SDK handles a checkpoint error, including purchase
    /// synchronization errors after a custom paywall presenter completes. Called on the Unity main thread.
    /// The SDK waits for the returned task. Throwing, a faulted task, or a null outcome uses the SDK's default dialog.
    /// </summary>
    [System.Obsolete(CheckpointsApi.ExperimentalMessage)]
    public delegate Task<CheckpointErrorDialogOutcome> CheckpointErrorDialogFactory(CheckpointError error);

    /// <summary>Experimental, invite-only. The kind of checkpoint failure. New values may be added.</summary>
    [System.Obsolete(CheckpointsApi.ExperimentalMessage)]
    public enum CheckpointErrorKind
    {
        /// <summary>A kind this version of the SDK doesn't know.</summary>
        Unknown,

        /// <summary>The selected flow could not be shown.</summary>
        FlowCouldNotBeShown,

        /// <summary>An action in the flow, such as a purchase, failed.</summary>
        FlowActionFailed,

        /// <summary>The flow's result, such as a purchase, could not be synchronized.</summary>
        FlowResultSyncFailed
    }

    /// <summary>Experimental, invite-only. An error the SDK asks the app to choose a dialog for.</summary>
    [System.Obsolete(CheckpointsApi.ExperimentalMessage)]
    public class CheckpointError
    {
        public CheckpointErrorKind Kind { get; }

        /// <summary>The <see cref="Purchases.Error"/> code of the underlying error.</summary>
        public int Code { get; }

        public string Message { get; }

        internal CheckpointError(JSONNode response)
        {
            switch (response["kind"].Value)
            {
                case "flowCouldNotBeShown": Kind = CheckpointErrorKind.FlowCouldNotBeShown; break;
                case "flowActionFailed": Kind = CheckpointErrorKind.FlowActionFailed; break;
                case "flowResultSyncFailed": Kind = CheckpointErrorKind.FlowResultSyncFailed; break;
                default: Kind = CheckpointErrorKind.Unknown; break;
            }
            Code = response["code"].AsInt;
            Message = response["message"].Value;
        }
    }

    /// <summary>Experimental, invite-only. Content for the SDK's error dialog. Both strings must be non-empty.</summary>
    [System.Obsolete(CheckpointsApi.ExperimentalMessage)]
    public class CheckpointErrorDialog
    {
        public string Message { get; }
        public string PrimaryButtonTitle { get; }

        public CheckpointErrorDialog(string message, string primaryButtonTitle)
        {
            Message = message;
            PrimaryButtonTitle = primaryButtonTitle;
        }
    }

    /// <summary>Experimental, invite-only. What the SDK should do with a checkpoint error.</summary>
    [System.Obsolete(CheckpointsApi.ExperimentalMessage)]
    public class CheckpointErrorDialogOutcome
    {
        private readonly string _type;
        private readonly CheckpointErrorDialog _dialog;

        private CheckpointErrorDialogOutcome(string type, CheckpointErrorDialog dialog)
        {
            _type = type;
            _dialog = dialog;
        }

        /// <summary>Shows custom content in the SDK's dialog. Empty content uses the default dialog.</summary>
        public static CheckpointErrorDialogOutcome Show(CheckpointErrorDialog dialog) =>
            new CheckpointErrorDialogOutcome("show", dialog ?? throw new ArgumentNullException(nameof(dialog)));

        /// <summary>Shows the SDK's default error dialog.</summary>
        public static CheckpointErrorDialogOutcome UseDefault() => new CheckpointErrorDialogOutcome("useDefault", null);

        /// <summary>Shows no error dialog.</summary>
        public static CheckpointErrorDialogOutcome Suppress() => new CheckpointErrorDialogOutcome("suppress", null);

        internal string ToJsonString()
        {
            if (_dialog != null && (string.IsNullOrEmpty(_dialog.Message) || string.IsNullOrEmpty(_dialog.PrimaryButtonTitle)))
                return CheckpointBridge.UseDefaultErrorDialog;
            var json = new JSONObject();
            json["type"] = _type;
            if (_dialog != null)
            {
                var dialog = new JSONObject();
                dialog["message"] = _dialog.Message;
                dialog["buttonTitle"] = _dialog.PrimaryButtonTitle;
                json["dialog"] = dialog;
            }
            return json.ToString();
        }
    }
}
