# Internal Git-sourced CocoaPods smoke check

The committed Podfile explicitly sources all four RevenueCat/PHC pods from Git.
The integration project's export hook replaces the generated Podfile with this
fixture before either resolver installs pods. It is test-only and does not ship
with either SDK package. This fixture contains no unrelated pods to retain.

Both Unity EDM and Google EDM4U CI export the normal integration project, then
export the minimal `CocoaPodsSmokeTest` scene once. The smoke export must retain the
committed Podfile. macOS installs/builds each workspace and checks core/UI resource
bundles. The app writes the result of configuring Purchases with a fake key and calling
`GetAppUserId()` through the C# and native bridge; CI asserts that result file. No backend key is needed.

SPM is the supported Apple installation route. The PHC bump lane maintains
PHC/native tags. Recheck callback order when upgrading either resolver.
