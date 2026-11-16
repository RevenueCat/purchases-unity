using System;
using System.Runtime.InteropServices;
using UnityEngine;

// Built only into the integration fixture's dedicated smoke scene.
public class CocoaPodsSmokeTest : MonoBehaviour
{
#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void CocoaPodsSmokeShowResult(string result);
#endif

    private void Start()
    {
        string result;
        try
        {
            const string appUserId = "git-cocoapods-smoke-test";
            var purchases = GetComponent<Purchases>();
            purchases.Configure(Purchases.PurchasesConfiguration.Builder
                .Init("appl_cocoapods_smoke_test").SetAppUserId(appUserId).Build());
            if (purchases.GetAppUserId() != appUserId)
                throw new InvalidOperationException("The native bridge returned the wrong app user ID");
            result = "COCOAPODS_SMOKE_PASSED";
        }
        catch (Exception error)
        {
            result = "COCOAPODS_SMOKE_FAILED: " + error.Message;
        }

        Debug.Log(result);
#if UNITY_IOS && !UNITY_EDITOR
        CocoaPodsSmokeShowResult(result);
#endif
    }
}
