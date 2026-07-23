using UnityEngine;

namespace RevenueCat.Tester
{
    /// <summary>
    /// Loads RevenueCat API keys from a JSON resource file and applies them to the Purchases component.
    /// This allows local development without modifying inspector settings.
    /// </summary>
    public static class LocalApiKeys
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void ApplyLocalKeys()
        {
            var keysAsset = Resources.Load<TextAsset>("local-api-keys");
            if (keysAsset == null)
            {
                return;
            }

            var purchases = Object.FindFirstObjectByType<Purchases>();
            if (purchases == null)
            {
                return;
            }

            try
            {
                var keysData = JsonUtility.FromJson<ApiKeysData>(keysAsset.text);

                bool anyKeyApplied = false;

                if (!string.IsNullOrEmpty(keysData.apple))
                {
                    purchases.revenueCatAPIKeyApple = keysData.apple;
                    anyKeyApplied = true;
                }

                if (!string.IsNullOrEmpty(keysData.google))
                {
                    purchases.revenueCatAPIKeyGoogle = keysData.google;
                    anyKeyApplied = true;
                }

                if (!string.IsNullOrEmpty(keysData.amazon))
                {
                    purchases.revenueCatAPIKeyAmazon = keysData.amazon;
                    anyKeyApplied = true;
                }

                if (anyKeyApplied)
                {
                    Debug.Log("[LocalApiKeys] Applied local RevenueCat API keys from Resources/local-api-keys.json");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[LocalApiKeys] Failed to parse local-api-keys.json: {ex.Message}");
            }
        }

        [System.Serializable]
        private class ApiKeysData
        {
            public string apple;
            public string google;
            public string amazon;
        }
    }
}
