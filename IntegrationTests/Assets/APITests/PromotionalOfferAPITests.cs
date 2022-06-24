using System;
using UnityEngine;

namespace DefaultNamespace
{
    public class PromotionalOfferAPITests : MonoBehaviour
    {
        private void Start()
        {
            Purchases.PromotionalOffer promotionalOffer = new Purchases.PromotionalOffer(null);
            string identifier = promotionalOffer.Identifier;
            string keyIdentifier = promotionalOffer.KeyIdentifier;
            string nonce = promotionalOffer.Nonce;
            string signature = promotionalOffer.Signature;
            long timestamp = promotionalOffer.Timestamp;
        }
    }
}