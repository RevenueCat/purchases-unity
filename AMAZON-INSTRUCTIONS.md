- Download the `.unitypackage` in this release
- Open your Unity Project
- Select Import package -> Custom package
<img width="471" alt="Screen Shot 2021-11-24 at 3 55 50 PM" src="https://user-images.githubusercontent.com/664544/143326927-764cb381-30a7-4d8d-8f3a-3c45c1e9d67f.png">

- Select Purchases_Amazon.unityPackage and make sure all of the files are selected and press import

<img width="472" alt="Screen Shot 2021-11-24 at 3 56 10 PM" src="https://user-images.githubusercontent.com/664544/143326950-ec8d5993-cd9e-468a-9a9a-27fee8a63519.png">

- If Package Manager Resolver asks to solve conflicts, choose library versions and select OK

- Download the .zip from [Amazon](https://amzndevresources.com/iap/sdk/AmazonInAppPurchasing_Android.zip) and unzip 

- Drag and drop the Amazon in-app-purchasing library into Assets -> Plugins -> Android

<img width="639" alt="Screen Shot 2021-11-24 at 3 56 41 PM" src="https://user-images.githubusercontent.com/664544/143326994-70cbcf7f-57c2-495c-8279-12e540f72e4a.png">

- Select Use Amazon in the Editor

<img width="370" alt="Screen Shot 2021-11-24 at 3 57 01 PM" src="https://user-images.githubusercontent.com/664544/143327015-c0563d7f-df10-41c3-a150-9d14988e7148.png">

If calling setup on runtime, using Runtime Setup, you can select “Use Runtime Setup” and call setup this way.

```
       var builder = PurchasesConfiguration.Builder.Init("amazon_specific_api_key")
            .SetUseAmazon(true);
        purchases.Setup(builder.Build());
```

Due to some limitations, RevenueCat will only validate purchases made in production or in Live App Testing and won't validate purchases made with the Amazon App Tester.
