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
        /// Whether the customer center is supported on this platform.
        /// Returns true on iOS/Android device builds; false on other platforms
        /// (Editor, Windows, macOS, WebGL, etc.).
        /// </summary>
        /// <returns>True if supported on the current platform, otherwise false.</returns>
        bool IsSupported();
    }
} 
