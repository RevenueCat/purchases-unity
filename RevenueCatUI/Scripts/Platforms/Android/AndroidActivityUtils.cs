#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine;

namespace RevenueCatUI.Platforms
{
    internal static class AndroidActivityUtils
    {
        internal static AndroidJavaObject GetCurrentActivity()
        {
            // AndroidApplication.currentActivity exists only in Unity 6+, so rely on UnityPlayer.currentActivity to keep compatibility with older Unity versions.
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                return unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            }
        }
    }
}
#endif

