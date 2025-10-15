using System.Threading.Tasks;
using RevenueCatUI;

namespace RevenueCatUI.Internal
{
    /// <summary>
    /// Platform abstraction for presenting the RevenueCat Customer Center experience.
    /// </summary>
    internal interface ICustomerCenterPresenter
    {
        /// <summary>
        /// Presents the Customer Center UI modally.
        /// </summary>
        /// <param name="callbacks">Callback container reserved for future expansion.</param>
        /// <returns>A task describing the outcome of the presentation.</returns>
        Task<CustomerCenterResult> PresentAsync(CustomerCenterCallbacks callbacks);
    }
}
