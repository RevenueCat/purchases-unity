1. Update to the latest SDK versions:
   1. Update the ios version in update-sdks.sh
   1. Run update-sdks.sh
   1. Update the Android version in RevenueCatDependencies.xml
   1. Update the Android version in the `purchasesunit/build.gradle`
   1. Download the latest common files, replace them and make the changes to make them compatible with Unity.
   1. Run `Android/build_and_install.sh`.
1. Update the VERSION file.
1. Update the VERSIONS.md file.
1. Add an entry to CHANGELOG.md
1. `git commit -am "Preparing for version x.y.z"`
1. `git tag x.y.z`
1. `git push origin master && git push --tags`
1. Run unity using the terminal with the flag `-gvh_disable`. Remove the Play Services Resolver plugin. This is to make sure the VersionManager of the Play Services Resolver doesn't run. Import the latest version of the plugin. Use Unity to create a `Purchases.unitypackage` from the Assets folder. Don't include the Scenes and make sure `Plugins/Android` is empty. Include the Play Services Resolver folder. (https://github.com/googlesamples/unity-jar-resolver#getting-started)
1. Create a new release in github and upload
1. Update docs link to new unity package
