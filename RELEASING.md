1. Update to the latest SDK versions (update the update-sdks.sh and run Android/build_and_install.sh).
2. Update the VERSION file.
3. Add an entry to CHANGELOG.md
4. `git commit -am "Preparing for version x.y.z"`
5. `git tag x.y.z`
6. `git push origin master && git push --tags`
7. Use Unity to create a `Purchases.unitypackage` from the Assets/Plugins folder
8. Create a new release in github and upload
