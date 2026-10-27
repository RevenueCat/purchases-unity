
#if UNITY_EDITOR && (UNITY_IOS || UNITY_VISIONOS)

using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

public static class XcodeSwiftVersionPostProcess
{
    private const string CopyResourceBundlesPhaseName = "[RevenueCat] Copy Swift Package Resource Bundles";
    private const string CopyResourceBundlesFileName = "RevenueCatCopySwiftPackageResources.sh";
    private const string CopyResourceBundlesPhaseScript = "/bin/sh \"$SRCROOT/" + CopyResourceBundlesFileName + "\"";

    // Work around missing SPM resources when packages are linked through UnityFramework:
    // https://github.com/googlesamples/unity-jar-resolver/issues/766
    // https://github.com/googlesamples/unity-jar-resolver/issues/779
    // Monitor Unity's com.unity.external-dependency-manager releases.
    // Remove once supported Unity/resolver versions embed these bundles without this phase;
    // verify paywall presentation from both a normal build and an archive before removing it.
    // Clean Xcode build products after switching dependency managers or removing RevenueCat UI.
    // Inspect active package products instead of copying stale bundles left in build products.
    // Archive products can be symlinks; rsync must dereference them.
    private const string CopyResourceBundlesScript = @"set -e

project_file=""${PROJECT_FILE_PATH}/project.pbxproj"" # Other post-processors may add packages after ours.
core=false
ui=false
for target in __UNITY_FRAMEWORK_GUID__ __UNITY_MAIN_GUID__; do
  /usr/libexec/PlistBuddy -c ""Print :objects:${target}:isa"" ""$project_file"" >/dev/null
  index=0
  while product=$(/usr/libexec/PlistBuddy -c ""Print :objects:${target}:packageProductDependencies:${index}"" ""$project_file"" 2>/dev/null); do
    name=$(/usr/libexec/PlistBuddy -c ""Print :objects:${product}:productName"" ""$project_file"")
    case ""$name"" in
      PurchasesHybridCommon|RevenueCat) core=true ;;
      PurchasesHybridCommonUI|RevenueCatUI) core=true; ui=true ;;
    esac
    index=$((index + 1))
  done
done
bundles=""""
if [ ""$core"" = true ]; then bundles=""RevenueCat_RevenueCat.bundle""; fi
if [ ""$ui"" = true ]; then bundles=""$bundles RevenueCat_RevenueCatUI.bundle""; fi

app_resources=""${TARGET_BUILD_DIR}/${UNLOCALIZED_RESOURCES_FOLDER_PATH}""
for bundle in $bundles; do
  printf '%s\n' ""[RevenueCat] Copying $bundle to $app_resources/""
  rsync -aL --delete ""${BUILT_PRODUCTS_DIR}/${bundle}"" ""$app_resources/""
  if [ ""$ACTION"" = install ]; then
    printf '%s\n' ""[RevenueCat] Copying $bundle to ${INSTALL_DIR}/${UNLOCALIZED_RESOURCES_FOLDER_PATH}/""
    mkdir -p ""${INSTALL_DIR}/${UNLOCALIZED_RESOURCES_FOLDER_PATH}""
    rsync -aL --delete ""${BUILT_PRODUCTS_DIR}/${bundle}"" ""${INSTALL_DIR}/${UNLOCALIZED_RESOURCES_FOLDER_PATH}/""
  fi
done
";

    // Run after the dependency manager has updated the Xcode project.
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

        // Refresh the exported script on every build while keeping the phase itself stable.
        string script = CopyResourceBundlesScript
            .Replace("__UNITY_FRAMEWORK_GUID__", project.GetUnityFrameworkTargetGuid())
            .Replace("__UNITY_MAIN_GUID__", project.GetUnityMainTargetGuid());
        File.WriteAllText(Path.Combine(path, CopyResourceBundlesFileName), script);

        string mainTargetGuid = project.GetUnityMainTargetGuid();
        // Appending to an existing export keeps the phases from the previous build.
        if (project.GetShellScriptBuildPhaseForTarget(mainTargetGuid, CopyResourceBundlesPhaseName, "/bin/sh", CopyResourceBundlesPhaseScript) != null)
        {
            return;
        }
        project.AddShellScriptBuildPhase(mainTargetGuid, CopyResourceBundlesPhaseName, "/bin/sh", CopyResourceBundlesPhaseScript);

        project.WriteToFile(projPath);
    }

}
#endif
