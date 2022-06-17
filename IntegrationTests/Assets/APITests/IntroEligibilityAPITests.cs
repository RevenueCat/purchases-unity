using System;
using UnityEngine;

namespace DefaultNamespace
{
    public class IntroEligibilityAPITests : MonoBehaviour
    {
        private void Start()
        {
            Purchases.IntroEligibility introEligibility = new Purchases.IntroEligibility(null);
            Purchases.IntroEligibilityStatus status = introEligibility.Status;
            string description = introEligibility.Description;
        }
    }
}