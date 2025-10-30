using System.Threading.Tasks;
using RevenueCatUI;
using UnityEngine;

namespace DefaultNamespace
{
    public class PaywallsPresenterAPITests : MonoBehaviour
    {
        private PaywallResult _presentResult;
        private PaywallResult _presentIfNeededResult;
        private string _presentResultNativeString;
        private string _presentIfNeededNativeString;
        private PaywallResultType _presentResultType;
        private PaywallResultType _presentIfNeededResultType;

        private void Start()
        {
            PaywallOptions defaultOptions = new PaywallOptions(displayCloseButton: true);
            PaywallOptions offeringOptions = new PaywallOptions(null, displayCloseButton: false);

            Task<PaywallResult> presentTask = PaywallsPresenter.Present(defaultOptions);
            Task<PaywallResult> presentIfNeededTask = PaywallsPresenter.PresentIfNeeded(
                "premium_entitlement",
                offeringOptions);

            presentTask.ContinueWith(task =>
            {
                _presentResult = task.Result;
                _presentResultType = _presentResult.Result;
                _presentResultNativeString = _presentResultType.ToNativeString();
            });

            presentIfNeededTask.ContinueWith(task =>
            {
                _presentIfNeededResult = task.Result;
                _presentIfNeededResultType = _presentIfNeededResult.Result;
                _presentIfNeededNativeString = _presentIfNeededResultType.ToNativeString();
                _presentIfNeededResultType = PaywallResultTypeExtensions.FromNativeString(_presentIfNeededNativeString);
            });
        }
    }
}

