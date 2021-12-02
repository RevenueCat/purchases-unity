- Download the `.unitypackage` in the release
- Open your Unity Project
- Select Import package -> Custom package
<img width="471" alt="Screen Shot 2021-11-24 at 3 55 50 PM" src="https://user-images.githubusercontent.com/664544/143326927-764cb381-30a7-4d8d-8f3a-3c45c1e9d67f.png">

- Select Purchases_Amazon.unityPackage and make sure all of the files are selected and press import

<img width="472" alt="Screen Shot 2021-11-24 at 3 56 10 PM" src="https://user-images.githubusercontent.com/664544/143326950-ec8d5993-cd9e-468a-9a9a-27fee8a63519.png">

- If `Package Manager Resolver` asks to solve conflicts, choose library versions and select OK

- If your app also targets the Google Play Store, you would want to conditionally exclude the billingclient to prevent duplicated classes. This can be done by adding the following to your `mainTemplate.gradle` to prevent duplicated classes when Unity IAP is targeting the Play Store. If you don't have a `mainTemplate.gradle`, make sure you have `Custom Main Gradle Template` selected in the `Android Player Settings`. That should create a `mainTemplate.gradle` inside the Assets/Plugins/Android.

```
dependencies {
    ...
    
    // ** ADD THIS **
    // Only exclude if billing client 3 is already in the project
    if (!project(":unityLibrary").fileTree(dir: 'libs', include: ['billing-3*.aar']).isEmpty()) {
        configurations.all {
            exclude group: 'com.android.billingclient', module: 'billing'
        }
    }
}
```


- Perform a Resolve using the editor option `Assets/External Dependency Manager/Android Resolver/Resolve` menu. This will add the right dependencies to the `mainTemplate.gradle`.

- In observer mode, adding the Amazon in-app-purchasing library is not necessary since it will be added by Unity IAP when targeting the Amazon Store

- Select Use Amazon in the Editor

<img width="370" alt="Screen Shot 2021-11-24 at 3 57 01 PM" src="https://user-images.githubusercontent.com/664544/143327015-c0563d7f-df10-41c3-a150-9d14988e7148.png">

If calling setup on runtime, you can select “Use Runtime Setup” and call setup this way.

```
       var builder = PurchasesConfiguration.Builder.Init("amazon_specific_api_key")
            .SetUseAmazon(true);
        purchases.Setup(builder.Build());
```

Due to some limitations, RevenueCat will only validate purchases made in production or in Live App Testing and won't validate purchases made with the Amazon App Tester. You can read more about the different testing environments in [our Amazon (Beta) docs](https://docs.revenuecat.com/docs/amazon-store-beta#sandbox-testing).