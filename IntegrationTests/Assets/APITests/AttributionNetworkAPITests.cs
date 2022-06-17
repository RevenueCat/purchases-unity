using UnityEngine;


public class AttributionNetworkAPITests : MonoBehaviour
{
    private void Start()
    {
        Purchases.AttributionNetwork network = Purchases.AttributionNetwork.ADJUST;
        switch (network)
        {
            case Purchases.AttributionNetwork.ADJUST:
            case Purchases.AttributionNetwork.BRANCH:
            case Purchases.AttributionNetwork.TENJIN:
            case Purchases.AttributionNetwork.FACEBOOK:
            case Purchases.AttributionNetwork.APPSFLYER:
            case Purchases.AttributionNetwork.APPLE_SEARCH_ADS:
                break;
        }
    }
}