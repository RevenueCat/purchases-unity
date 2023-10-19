## Instructions
- Download the `Purchases.unitypackage` in the release.
- Open your Unity Project
- Select Import package -> Custom package
<img width="471" alt="Screen Shot 2021-11-24 at 3 55 50 PM" src="https://user-images.githubusercontent.com/664544/143326927-764cb381-30a7-4d8d-8f3a-3c45c1e9d67f.png">

- Select Purchases.unitypackage and make sure all of the files are selected and press import

<img width="472" alt="Screen Shot 2021-11-24 at 3 56 10 PM" src="https://user-images.githubusercontent.com/664544/143326950-ec8d5993-cd9e-468a-9a9a-27fee8a63519.png">

- If `Package Manager Resolver` asks to solve conflicts, choose library versions and select OK

- Select Use Amazon in the Editor

<img width="370" alt="Screen Shot 2021-11-24 at 3 57 01 PM" src="https://user-images.githubusercontent.com/664544/143327015-c0563d7f-df10-41c3-a150-9d14988e7148.png">

If calling setup on runtime, you can select “Use Runtime Setup” and call setup this way.

```c#
var builder = PurchasesConfiguration.Builder.Init("amazon_specific_api_key")
    .SetUseAmazon(true);
purchases.Setup(builder.Build());
```

- Due to technical limitations, RevenueCat will only validate purchases made in production or in Live App Testing and won't validate purchases made with the Amazon App Tester. You can read more about the different testing environments in [our Amazon (Beta) docs](https://docs.revenuecat.com/docs/amazon-store-beta#sandbox-testing).

- Due to technical limitations with the Amazon SDK, `SyncPurchases` doesn't work when on Amazon observer mode. In order to sync purchases with RevenueCat you have to send the current active subscriptions when Unity IAP initializes and after every successful purchase. For example:

 ```c#
public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
{
    m_StoreController = controller;
    storeExtensionProvider = extensions;
    var purchases = GetComponent<Purchases>();
    purchases.SetDebugLogsEnabled(true);
    foreach (Product product in controller.products.all)
    {
        if (product.hasReceipt) {
            var amazonExtensions = storeExtensionProvider.GetExtension<IAmazonExtensions>();
            var userId = amazonExtensions.amazonUserId;
            purchases.SyncObserverModeAmazonPurchase( 
                product.definition.id,
                product.transactionID,
                userId,
                product.metadata.isoCurrencyCode,
                Decimal.ToDouble(product.metadata.localizedPrice)
            );
        }
    }
}

public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
{
    var purchases = GetComponent<Purchases>();
    
    var amazonExtensions = storeExtensionProvider.GetExtension<IAmazonExtensions>();
    var userId = amazonExtensions.amazonUserId;
    purchases.SyncObserverModeAmazonPurchase(
        e.purchasedProduct.definition.id,
        e.purchasedProduct.transactionID,
        userId,
        e.purchasedProduct.metadata.isoCurrencyCode,
        Decimal.ToDouble(e.purchasedProduct.metadata.localizedPrice)
    );
    return PurchaseProcessingResult.Complete;
}
 ```
