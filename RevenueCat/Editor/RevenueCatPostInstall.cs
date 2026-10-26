
#if UNITY_EDITOR && (UNITY_IOS || UNITY_VISIONOS)

using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

public static class XcodeSwiftVersionPostProcess
{
    private const string CopyResourceBundlesPhaseName = "[RevenueCat] Copy Swift Package Resource Bundles";

    // Xcode embeds a Swift package's resource bundles only into an application target linked
    // against the package. The dependency manager links our packages against UnityFramework,
    // so without this phase the bundles never reach the app and RevenueCatUI's Bundle.module
    // lookup hits a fatalError when a paywall is presented. CocoaPods builds its bundles in
    // per-pod directories, so nothing matches there.
    private const string CopyResourceBundlesScript =
        "for bundle in \"${BUILT_PRODUCTS_DIR}\"/RevenueCat_*.bundle; do\n" +
        "  [ -d \"$bundle\" ] || continue\n" +
        "  rsync -a --delete \"$bundle\" \"${TARGET_BUILD_DIR}/${UNLOCALIZED_RESOURCES_FOLDER_PATH}/\"\n" +
        "  if [ \"$ACTION\" = \"install\" ]; then\n" +
        "    mkdir -p \"${INSTALL_DIR}/${UNLOCALIZED_RESOURCES_FOLDER_PATH}\"\n" +
        "    rsync -a --delete \"$bundle\" \"${INSTALL_DIR}/${UNLOCALIZED_RESOURCES_FOLDER_PATH}/\"\n" +
        "  fi\n" +
        "done\n";

    // set callbackOrder to 999 to ensure this runs as the last post process step
    [PostProcessBuild(999)]
    public static void OnPostProcessBuild(BuildTarget buildTarget, string path)
    {
        if (buildTarget == BuildTarget.iOS)
        {
            Debug.Log("Installing for iOS. Setting ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES and ENABLE_BITCODE to NO and adding StoreKit");
            ModifyFrameworks(path);
            AddStoreKitFramework(path);
            AddCopyResourceBundlesPhase(path);
        }
    }

    private static void ModifyFrameworks(string path)
    {
        string projPath = PBXProject.GetPBXProjectPath(path);
        var project = new PBXProject();
        project.ReadFromFile(projPath);

        string mainTargetGuid = project.GetUnityMainTargetGuid();

        foreach (var targetGuid in new[] { mainTargetGuid, project.GetUnityFrameworkTargetGuid() })
        {
            project.SetBuildProperty(targetGuid, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "NO");
            // Older Unity versions still try to compile with Bitcode, which new versions of PHC
            // are no longer compatible with.
            project.SetBuildProperty(targetGuid, "ENABLE_BITCODE", "NO");
        }

        project.SetBuildProperty(mainTargetGuid, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "YES");

        project.WriteToFile(projPath);
    }

    private static void AddStoreKitFramework(string path)
    {
        string projPath = PBXProject.GetPBXProjectPath(path);
        var project = new PBXProject();
        project.ReadFromFile(projPath);

        string mainTargetGUID = project.GetUnityMainTargetGuid();
        project.AddFrameworkToProject(mainTargetGUID, "StoreKit.framework", false);

        project.WriteToFile(projPath);
    }

    private static void AddCopyResourceBundlesPhase(string path)
    {
        string projPath = PBXProject.GetPBXProjectPath(path);
        var project = new PBXProject();
        project.ReadFromFile(projPath);

        string mainTargetGuid = project.GetUnityMainTargetGuid();
        // Appending to an existing export keeps the phases from the previous build.
        if (project.GetShellScriptBuildPhaseForTarget(mainTargetGuid, CopyResourceBundlesPhaseName, "/bin/sh", CopyResourceBundlesScript) != null)
        {
            return;
        }
        project.AddShellScriptBuildPhase(mainTargetGuid, CopyResourceBundlesPhaseName, "/bin/sh", CopyResourceBundlesScript);

        project.WriteToFile(projPath);
    }

}
#endif
