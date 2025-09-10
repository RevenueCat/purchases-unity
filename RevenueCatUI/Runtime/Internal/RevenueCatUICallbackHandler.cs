using UnityEngine;

namespace RevenueCat.UI.Internal
{
    /// <summary>
    /// Handles callbacks from native RevenueCat UI platforms
    /// </summary>
    internal class RevenueCatUICallbackHandler : MonoBehaviour
    {
        private static RevenueCatUICallbackHandler _instance;

#if REVENUECAT_UI_NATIVE && UNITY_ANDROID && !UNITY_EDITOR
        private static Platforms.AndroidPaywallPresenter _androidPresenter;
        private static Platforms.AndroidCustomerCenterPresenter _androidCustomerCenterPresenter;
#endif

        public static void Initialize()
        {
            if (_instance == null)
            {
                var go = new GameObject("RevenueCatUI");
                _instance = go.AddComponent<RevenueCatUICallbackHandler>();
                DontDestroyOnLoad(go);
            }
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        public static void SetAndroidPresenter(Platforms.AndroidPaywallPresenter presenter)
        {
            _androidPresenter = presenter;
        }

        public static void SetAndroidCustomerCenterPresenter(Platforms.AndroidCustomerCenterPresenter presenter)
        {
            _androidCustomerCenterPresenter = presenter;
        }
#endif

        // Called from Android via UnitySendMessage
        public void OnPaywallResult(string resultData)
        {
#if REVENUECAT_UI_NATIVE && UNITY_ANDROID && !UNITY_EDITOR
            _androidPresenter?.OnPaywallResult(resultData);
#endif
        }

        // Called from Android via UnitySendMessage
        public void OnCustomerCenterResult(string resultData)
        {
#if REVENUECAT_UI_NATIVE && UNITY_ANDROID && !UNITY_EDITOR
            _androidCustomerCenterPresenter?.OnCustomerCenterResult(resultData);
#endif
        }
    }
} 
