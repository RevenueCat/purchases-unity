using JetBrains.Annotations;

public partial class Purchases
{
    /// <summary>
    /// Holds the information used when upgrading from another sku. For Android use only.
    /// </summary>
    public class GoogleProductChangeInfo
    {
        /**
        * The old product identifier to upgrade from.
        */
        public readonly string OldProductIdentifier;
        /**
        * The ProrationMode to use when upgrading the given oldProductIdentifier.
        */
        [CanBeNull] public readonly ProrationMode ProrationMode;
    }
}
