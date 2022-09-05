### ⚠️ Important: If you're using RevenueCat along with Unity IAP side-by-side ⚠️
Starting with this version you need to use Unity IAP 4.4.0+ if you are using Unity IAP together with RevenueCat in your project. You can update from Unity Package Manager.
If you need to use an older version of Unity IAP, you can continue to use purchases-unity < 4.2.0.
### API Changes
* `StoreTransaction`: `RevenueCatId` and `ProductId` have been deprecated in favor of `TransactionIdentifier` and `ProductIdentifier` respectively. (#145) via Toni Rico (@tonidero)
### Bugfixes
* Fix example compatibility with Unity 2020 (#139) via Andy Boedo (@aboedo)
### Other Changes
* Subtester: Fix unity android export (#142) via Andy Boedo (@aboedo)
* Update AMAZON-INSTRUCTIONS.md (#143) via Andy Boedo (@aboedo)
