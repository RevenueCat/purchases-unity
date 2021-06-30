1. Start a branch release/x.y.z
1. Update to the latest SDK versions in RevenueCatDependencies.xml for Android and iOS
1. Update the VERSION file, the platformFlavorVersion number in `PurchasesUnityHelper.m` and  the PLUGIN_VERSION number `PurchasesWrapper.java`
1. Update the VERSIONS.md file.
1. Add an entry to CHANGELOG.md
1. Make a PR, merge when approved
1. Make a tag and push
1. CircleCI will run a job `export-package` that will create a `Purchases.unitypackage` for you. Find the package in the artifacts section of the job
1. Create a new release in github and upload `Purchases.unitypackage`.
