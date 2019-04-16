1. Update to the latest SDK versions (update the update-sdks.sh and run update-sdks.sh and Android/build_and_install.sh).
1. Update the VERSION file.
1. Add an entry to CHANGELOG.md
1. `git commit -am "Preparing for version x.y.z"`
1. `git tag x.y.z`
1. `git push origin master && git push --tags`
1. Use Unity to create a `Purchases.unitypackage` from the Assets/Plugins folder
1. Create a new release in github and upload
1. Update docs link to new unity package
