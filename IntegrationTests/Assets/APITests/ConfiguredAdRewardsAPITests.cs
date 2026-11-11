using System;
using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace
{
    public class ConfiguredAdRewardsAPITests : MonoBehaviour
    {
        private void Check(Purchases purchases)
        {
            purchases.GetAdRewards("ad-unit-id", (rewards, error) =>
            {
                if (error != null || rewards == null) return;

                Purchases.AdRewardConfiguration primary = rewards.Reward;
                IReadOnlyList<Purchases.AdRewardConfiguration> additional = rewards.MoreRewards;
                switch (primary)
                {
                    case Purchases.AdRewardConfiguration.VirtualCurrency currency:
                        string code = currency.Code;
                        int amount = currency.Amount;
                        break;
                    case Purchases.AdRewardConfiguration.Entitlement entitlement:
                        string identifier = entitlement.Identifier;
                        TimeSpan duration = entitlement.Duration;
                        break;
                    case Purchases.AdRewardConfiguration.Unsupported unsupported:
                        break;
                }
            });

            Purchases.ConfiguredAdRewards absent = Purchases.ConfiguredAdRewards.NoConfiguredReward;
            Purchases.AdRewardConfiguration unknown = Purchases.AdRewardConfiguration.Unsupported.Instance;
        }
    }
}
