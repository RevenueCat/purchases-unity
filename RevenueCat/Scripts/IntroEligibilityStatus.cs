public partial class Purchases
{
    public enum IntroEligibilityStatus
    {
        /// RevenueCat doesn't have enough information to determine eligibility.
        IntroEligibilityStatusUnknown = 0,

        /// The user is not eligible for a free trial or intro pricing for this product.
        IntroEligibilityStatusIneligible = 1,

        /// The user is eligible for a free trial or intro pricing for this product.
        IntroEligibilityStatusEligible = 2
    }
}