using System;
using UnityEngine;

namespace DefaultNamespace
{
    public class IntroEligibilityStatusAPITests : MonoBehaviour
    {
        private void Start()
        {
            Purchases.IntroEligibilityStatus status = Purchases.IntroEligibilityStatus.IntroEligibilityStatusEligible;
            switch (status)
            {
                case Purchases.IntroEligibilityStatus.IntroEligibilityStatusEligible:
                case Purchases.IntroEligibilityStatus.IntroEligibilityStatusIneligible:
                case Purchases.IntroEligibilityStatus.IntroEligibilityStatusUnknown:
                case Purchases.IntroEligibilityStatus.IntroEligibilityStatusNoIntroOfferExists:
                    break;
            }
        }
    }
}