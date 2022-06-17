using System;
using UnityEngine;

namespace DefaultNamespace
{
    public class PromotionalOfferAPITests : MonoBehaviour
    {
        private void Start()
        {
            // TODO: update the properties to lowercase + readonly
            Purchases.PromotionalOffer promotionalOffer = new Purchases.PromotionalOffer(null);
            string identifier = promotionalOffer.identifier;
            string keyIdentifier = promotionalOffer.keyIdentifier;
            string nonce = promotionalOffer.nonce;
            string signature = promotionalOffer.signature;
            long timestamp = promotionalOffer.timestamp;
        }
    }
}