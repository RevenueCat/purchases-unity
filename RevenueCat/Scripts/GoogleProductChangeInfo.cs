using JetBrains.Annotations;

public partial class Purchases
{
    /// <summary>
    /// Holds the information used when upgrading from another sku. For Android use only.
    /// </summary>
    public class GoogleProductChangeInfo
    {
        /**
        * The old product identifier to change from.
        */
        public readonly string OldProductIdentifier;
        /**
        * The ProrationMode to use when upgrading the given oldProductIdentifier.
        */
        public readonly ProrationMode ProrationMode;

        public GoogleProductChangeInfo(string OldProductIdentifier, ProrationMode ProrationMode)
        {
            this.OldProductIdentifier = OldProductIdentifier;
            this.ProrationMode = ProrationMode;
        }
    }
}
