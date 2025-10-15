namespace RevenueCatUI
{
    /// <summary>
    /// Placeholder container for upcoming Customer Center callback support.
    /// </summary>
    public sealed class CustomerCenterCallbacks
    {
        internal static readonly CustomerCenterCallbacks None = new CustomerCenterCallbacks();

        // Callbacks will be added in a future PR; keeping this empty confirms the
        // new API surface compiles across all platforms without behavior changes.
        public CustomerCenterCallbacks()
        {
        }
    }
}
