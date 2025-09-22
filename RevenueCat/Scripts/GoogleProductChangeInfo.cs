using JetBrains.Annotations;

namespace RevenueCat
{
    /// <summary>
    /// Holds the information used when upgrading from another sku. For Android use only.
    /// </summary>
    public class GoogleProductChangeInfo
    {
        /**
        * The old product identifier to change from.
        */
        public  string OldProductIdentifier;
        /**
        * The ProrationMode to use when upgrading the given oldProductIdentifier.
        */
        public  ProrationMode ProrationMode;

        public GoogleProductChangeInfo(string OldProductIdentifier, ProrationMode ProrationMode)
        {
            this.OldProductIdentifier = OldProductIdentifier;
            this.ProrationMode = ProrationMode;
        }
    }
}
