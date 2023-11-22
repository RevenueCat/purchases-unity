using System;
using UnityEngine;

namespace DefaultNamespace
{
    public class VerificationResultAPITests : MonoBehaviour
    {
        private void Start()
        {
            Purchases.VerificationResult verificationResult = Purchases.VerificationResult.NotRequested;
            switch (verificationResult)
            {
                case Purchases.VerificationResult.NotRequested:
                case Purchases.VerificationResult.Verified:
                case Purchases.VerificationResult.Failed:
                case Purchases.VerificationResult.VerifiedOnDevice:
                    break;
            }
        }
    }
}
