using System;
using JetBrains.Annotations;
using RevenueCat.SimpleJSON;

public partial class Purchases
{
    /// Type containing information of installment subscriptions.
    /// Supported in Google Play (via SubscriptionOption) and iOS (via StoreProduct).
    public class InstallmentsInfo
    {
        /// Number of payments the customer commits to in order to purchase the subscription.
        public readonly int CommitmentPaymentsCount;

        /// After the commitment payments are complete, the number of payments the user commits to upon a renewal.
        /// On iOS this is always equal to CommitmentPaymentsCount.
        public readonly int RenewalCommitmentPaymentsCount;

        /// Duration of each installment, specified in ISO 8601 format. For example, P1M equates to one month.
        /// iOS only. Null on Android.
        [CanBeNull] public readonly string CommitmentInstallmentPeriod;

        /// Price charged for each installment billing period.
        /// iOS only. Null on Android.
        public readonly float? InstallmentBillingPrice;

        /// Formatted price charged for each installment billing period, including its currency sign.
        /// iOS only. Null on Android.
        [CanBeNull] public readonly string InstallmentBillingPriceString;

        /// Total duration of the customer's installment commitment, specified in ISO 8601 format.
        /// iOS only. Null on Android.
        [CanBeNull] public readonly string CommitmentTotalPeriod;

        /// Total price the customer commits to paying across all installments.
        /// iOS only. Null on Android.
        public readonly float? CommitmentTotalPrice;

        /// Formatted total price the customer commits to paying across all installments, including its currency sign.
        /// iOS only. Null on Android.
        [CanBeNull] public readonly string CommitmentTotalPriceString;

        /// The billing plan used for the installments.
        /// iOS only. Null on Android.
        public readonly BillingPlanType? BillingPlanType;

        public InstallmentsInfo(JSONNode response)
        {
            CommitmentPaymentsCount = response["commitmentPaymentsCount"];
            RenewalCommitmentPaymentsCount = response["renewalCommitmentPaymentsCount"];
            CommitmentInstallmentPeriod = response["commitmentInstallmentPeriod"];
            InstallmentBillingPriceString = response["installmentBillingPriceString"];
            CommitmentTotalPeriod = response["commitmentTotalPeriod"];
            CommitmentTotalPriceString = response["commitmentTotalPriceString"];

            var installmentBillingPriceNode = response["installmentBillingPrice"];
            if (installmentBillingPriceNode != null && !installmentBillingPriceNode.IsNull)
            {
                InstallmentBillingPrice = installmentBillingPriceNode.AsFloat;
            }

            var commitmentTotalPriceNode = response["commitmentTotalPrice"];
            if (commitmentTotalPriceNode != null && !commitmentTotalPriceNode.IsNull)
            {
                CommitmentTotalPrice = commitmentTotalPriceNode.AsFloat;
            }

            var billingPlanTypeNode = response["billingPlanType"];
            if (billingPlanTypeNode != null && !billingPlanTypeNode.IsNull)
            {
                // Enum.TryParse also accepts numeric strings and comma-separated names, so only keep
                // values that are exactly the name of a defined member.
                if (Enum.TryParse(billingPlanTypeNode.Value, out Purchases.BillingPlanType billingPlanType)
                    && Enum.GetName(typeof(Purchases.BillingPlanType), billingPlanType) == billingPlanTypeNode.Value)
                {
                    BillingPlanType = billingPlanType;
                }
                else
                {
                    BillingPlanType = Purchases.BillingPlanType.UNKNOWN;
                }
            }
        }

        public override string ToString()
        {
            return $"{nameof(CommitmentPaymentsCount)}: {CommitmentPaymentsCount}\n" +
                $"{nameof(RenewalCommitmentPaymentsCount)}: {RenewalCommitmentPaymentsCount}\n" +
                $"{nameof(CommitmentInstallmentPeriod)}: {CommitmentInstallmentPeriod}\n" +
                $"{nameof(InstallmentBillingPrice)}: {InstallmentBillingPrice}\n" +
                $"{nameof(InstallmentBillingPriceString)}: {InstallmentBillingPriceString}\n" +
                $"{nameof(CommitmentTotalPeriod)}: {CommitmentTotalPeriod}\n" +
                $"{nameof(CommitmentTotalPrice)}: {CommitmentTotalPrice}\n" +
                $"{nameof(CommitmentTotalPriceString)}: {CommitmentTotalPriceString}\n" +
                $"{nameof(BillingPlanType)}: {BillingPlanType}\n";
        }
    }
}
