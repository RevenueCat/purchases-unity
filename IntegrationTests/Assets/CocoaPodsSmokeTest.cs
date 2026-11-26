using System;
using System.IO;
using UnityEngine;

// Built only into the integration fixture's dedicated smoke scene.
[DefaultExecutionOrder(1)] // Purchases initializes its native wrapper in Start().
public class CocoaPodsSmokeTest : MonoBehaviour
{

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
            result = "COCOAPODS_SMOKE_FAILED: " + error;
        }

        Debug.Log(result);
        File.WriteAllText(Path.Combine(Application.persistentDataPath, "cocoapods-smoke-result.txt"), result);
    }
}
