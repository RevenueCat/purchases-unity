### Subtester sample app
This is a sample Unity app that has RevenueCat setup. 
In order to use it: 
- Video instructions: https://www.loom.com/share/bf8d07331a5b43c083efdf6b0f3ec724

1. Open the project with Unity
1. Open the Main scene
1. Select the PurchasesManager, and make sure that it has a PurchasesListener and Purchases object attached. If they're not attached, remove other objects from the PurchasesManager and add a PurchasesListener and Purchases object. 
1. Set the RevenueCat API key in the Purchases object
1. Set the appUserId if needed in the Purchases object
1. Set the Parent Panel, Button Prefab and Customer Info Label objects in the PurchasesListener. 

### Automated Edit Mode tests

Executable tests live in `Assets/Tests/EditMode`. After importing `Purchases.unitypackage` and
`PurchasesUI.unitypackage` into this project, run them from **Window > General > Test Runner** by selecting
**EditMode** and **Run All**.

CircleCI exports and imports both packages before running the same suite, so the tests cover the packaged SDK
that users install.
