using System;
using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace
{
    public class SubscriptionOptionAPITests : MonoBehaviour
    {
        private void Start()
        {
            Purchases.SubscriptionOption subscriptionOption = new Purchases.SubscriptionOption(null);
            string ID = subscriptionOption.ID;
            string StoreProductId = subscriptionOption.StoreProductId;
            string ProductId = subscriptionOption.ProductId;
            Purchases.SubscriptionOption.PricingPhase pricingPhase = subscriptionOption.PricingPhases[0];
            Purchases.SubscriptionOption.PeriodUnit unit = pricingPhase.BillingPeriod.Unit;
            int value = pricingPhase.BillingPeriod.Value;
            string ISO8601 = pricingPhase.BillingPeriod.ISO8601;
            Purchases.SubscriptionOption.RecurrenceMode recurrenceMode = pricingPhase.RecurrenceMode;
            int BillingCycleCount = pricingPhase.BillingCycleCount;
            string Formatted = pricingPhase.Price.Formatted;
            int AmountMicros = pricingPhase.Price.AmountMicros;
            string CurrencyCode = pricingPhase.Price.CurrencyCode;
            Purchases.SubscriptionOption.OfferPaymentMode OfferPaymentMode = pricingPhase.OfferPaymentMode;
            Purchases.SubscriptionOption.PricingPhase FullPricePhase = subscriptionOption.FullPricePhase;
            Purchases.SubscriptionOption.PricingPhase FreePhase = subscriptionOption.FreePhase;
            Purchases.SubscriptionOption.PricingPhase IntroPhase = subscriptionOption.IntroPhase;
            string Tag = subscriptionOption.Tags[0];
            bool IsBasePlan = subscriptionOption.IsBasePlan;
            bool IsPrepaid =  subscriptionOption.IsPrepaid;
            string PresentedOfferingIdentifier = subscriptionOption.PresentedOfferingIdentifier;
        }
    }
}