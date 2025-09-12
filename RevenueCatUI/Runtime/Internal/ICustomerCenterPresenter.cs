using System.Threading.Tasks;

namespace RevenueCat.UI
{
    /// <summary>
    /// Internal interface for presenting customer center.
    /// Implemented by platform-specific presenters.
    /// </summary>
    internal interface ICustomerCenterPresenter
    {
        /// <summary>
        /// Presents the customer center.
        /// </summary>
        /// <returns>Task that completes when the customer center is dismissed</returns>
        Task PresentCustomerCenterAsync();

        /// <summary>
        /// Checks if customer center presentation is supported on this platform.
        /// </summary>
        /// <returns>True if supported, false otherwise</returns>
        bool IsSupported();
    }
} 