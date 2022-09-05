### Important changes
Starting with this version you need to update to Unity IAP 4.4.0+ if you are using Unity IAP together with RevenueCat in your project. We removed support for older versions of Unity IAP since it upgraded to Android billing client 4.
### API Changes
* `StoreTransaction`: `RevenueCatId` and `ProductId` have been deprecated in favor of `TransactionIdentifier` and `ProductIdentifier` respectively. (#145) via Toni Rico (@tonidero)
### Bugfixes
* Fix example compatibility with Unity 2020 (#139) via Andy Boedo (@aboedo)
### Other Changes
* Subtester: Fix unity android export (#142) via Andy Boedo (@aboedo)
* Update AMAZON-INSTRUCTIONS.md (#143) via Andy Boedo (@aboedo)
