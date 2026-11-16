# Internal Git-sourced CocoaPods smoke check

The committed Podfile explicitly sources all four RevenueCat/PHC pods from Git.
The integration project's callback 45 copies this fixture after either resolver
creates its Podfile (40), before pod installation (50). It is test-only and does
not ship with either SDK package. This fixture contains no unrelated pods to retain.

Both Unity EDM and Google EDM4U CI export the normal integration project, then
export the minimal `CocoaPodsSmokeTest` scene twice. Every export must retain the
committed Podfile. macOS installs/builds each workspace and checks core/UI resource
bundles. Maestro observes the result of configuring Purchases with a fake key and
calling `GetAppUserId()` through the C# and native bridge. No backend key is needed.

SPM is the supported Apple installation route. Before merging `11.0-dev` into main,
replace the PHC branch with its release tag (SDK-4602). The PHC bump lane maintains
future PHC/native pins. Recheck callback order when upgrading either resolver.
