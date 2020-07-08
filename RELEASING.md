1. Update to the latest SDK versions:
   1. Update the versions in RevenueCatDependencies.xml for Android and iOS
   1. Update the Android version in the `purchasesunit/build.gradle`
1. Update the VERSION file and the platformFlavorVersion number in `PurchasesUnityHelper.m`
1. Update the VERSIONS.md file.
1. Add an entry to CHANGELOG.md
1. `git commit -am "Preparing for version x.y.z"`
1. `git tag x.y.z`
1. `git push origin master && git push --tags`
1. Run unity using the terminal with the flag `-gvh_disable` (something like `/Applications/Unity/Hub/Editor/2019.3.0f3/Unity.app/Contents/MacOS/Unity -gvh_disable -projectPath Subtester`). This is to make sure the VersionManager of the Play Services Resolver doesn't run. Remove the Play Services Resolver plugin. Import the latest version of the plugin. Use Unity to create a `Purchases.unitypackage` from the Assets folder. Don't include the Scenes or the contents of `Plugins/Android` (these will be downloaded by the plugin). Include the Play Services Resolver folder. More info on this plugin at https://github.com/googlesamples/unity-jar-resolver#getting-started
1. Add `com.android.billingclient.billing-2.0.3-no-service.aar` to `Revenuecat/Plugins/Android`. This special `aar` is the billing client without the `InAppBillingService.aidl`. Create a `Purchases_NoInAppBillingService.unitypackage` but this time don't include the `RevenueCatDependencies.xml` so that the Play Services Resolver doesn't resolve anything.
1. Create a new release in github and upload both packages.
1. Update docs link to new unity package


## How to modify billing client to remove IInAppBillingService.aidl

```
unzip com.android.billingclient.billing-2.0.3.aar -d tempFolder
unzip tempFolder/classes.jar -d tempFolder/classes
rm -rf tempFolder/classes/com/android/vending/billing/IInAppBillingService*
jar cvf tempFolder/classes.jar -C tempFolder/classes/ .
rm -rf tempFolder/classes/
jar cvf com.android.billingclient.billing-2.0.3.aar -C tempFolder/ .
```

