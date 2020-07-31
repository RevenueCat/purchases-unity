1. Update to the latest SDK versions:
   1. Update the versions in RevenueCatDependencies.xml for Android and iOS
   1. Update the Android version in the `purchasesunit/build.gradle`
1. Update the VERSION file and the platformFlavorVersion number in `PurchasesUnityHelper.m`
1. Update the VERSIONS.md file.
1. Add an entry to CHANGELOG.md
1. `git commit -am "Preparing for version x.y.z"`
1. `git tag x.y.z`
1. `git push origin master && git push --tags`
1. Run `./scripts/create-unity-package.sh`
1. Create a new release in github and upload both packages.
1. Update docs link to new unity package. Update the version in [here](https://docs.revenuecat.com/docs/unity#1-add-the-purchases-unity-package) and the common version in [here](https://docs.revenuecat.com/docs/unity#installation-with-unity-iap-side-by-side)


## How to modify billing client to remove IInAppBillingService.aidl

```
unzip com.android.billingclient.billing-2.0.3.aar -d tempFolder
unzip tempFolder/classes.jar -d tempFolder/classes
rm -rf tempFolder/classes/com/android/vending/billing/IInAppBillingService*
jar cvf tempFolder/classes.jar -C tempFolder/classes/ .
rm -rf tempFolder/classes/
jar cvf com.android.billingclient.billing-2.0.3.aar -C tempFolder/ .
```

