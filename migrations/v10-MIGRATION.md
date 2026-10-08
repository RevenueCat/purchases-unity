## v10 API Changes

### New Minimum Unity Version

This release raises the minimum supported Unity version to **2022.3**.

### Unity's External Dependency Manager replaces EDM4U

Google has deprecated the External Dependency Manager for Unity (EDM4U) and will archive its repository. This release adopts Unity's official [External Dependency Manager](https://docs.unity3d.com/Packages/com.unity.external-dependency-manager@2.1/manual/index.html) (`com.unity.external-dependency-manager`) instead. Both read the same `*Dependencies.xml` files, so native dependencies resolve the same way.

- If you install the plugin through the Unity Package Manager or OpenUPM, Unity's External Dependency Manager is now a dependency of the package and is installed automatically.
- If you install the plugin from `Purchases.unitypackage`, Google's EDM4U is no longer bundled. On first import, if the project has no dependency manager, the plugin installs Unity's External Dependency Manager through the Package Manager.
- If your project already uses Google's EDM4U (on its own or through another plugin), you can keep using it. Nothing changes until you decide to migrate.

To migrate an existing project from EDM4U:

1. Install `External Dependency Manager` from `Window > Package Manager > Unity Registry`.
2. When prompted, select `Use Unity's External Dependency Manager`. Your existing resolver settings are migrated automatically.
3. Once no other package depends on EDM4U, remove it (delete `Assets/ExternalDependencyManager` for `.unitypackage` installs, or uninstall `com.google.external-dependency-manager` for UPM installs).

You can switch back at any time from `Assets > External Dependency Manager > Change To Alternative EDM`.
