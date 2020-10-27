1. Update to the latest SDK versions in RevenueCatDependencies.xml for Android and iOS
1. Update the VERSION file and the platformFlavorVersion number in `PurchasesUnityHelper.m` and `PurchasesWrapper.java`
1. Update the VERSIONS.md file.
1. Add an entry to CHANGELOG.md
1. `git commit -am "Preparing for version x.y.z"`
1. `git tag x.y.z`
1. `git push origin master && git push --tags`
1. Run `./scripts/create-unity-package.sh`
1. Create a new release in github and upload `Purchases.unitypackage`.
