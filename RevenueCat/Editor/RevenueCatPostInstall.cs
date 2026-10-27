
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

    // Unity links packages to UnityFramework, so Xcode does not embed their resources in the app.
    // Inspect its active package products instead of copying stale bundles left in build products.
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
manifest=""${TARGET_TEMP_DIR}/RevenueCatCopiedResourceBundles.txt"" # Track identifiers to preserve CocoaPods replacements.
mkdir -p ""$TARGET_TEMP_DIR""
: > ""${manifest}.tmp""
for bundle in $bundles; do
  source=""${BUILT_PRODUCTS_DIR}/${bundle}""
  identifier=$(/usr/libexec/PlistBuddy -c ""Print :CFBundleIdentifier"" ""$source/Info.plist"")
  printf '%s|%s\n' ""$bundle"" ""$identifier"" >> ""${manifest}.tmp""
done

remove_previous_bundle() {
  destination=""$1/$bundle""
  current_identifier=$(/usr/libexec/PlistBuddy -c ""Print :CFBundleIdentifier"" ""$destination/Info.plist"" 2>/dev/null || true)
  if [ -n ""$identifier"" ] && [ ""$current_identifier"" = ""$identifier"" ]; then
    rm -rf ""$destination""
  fi
}
if [ -f ""$manifest"" ]; then
  while IFS='|' read -r bundle identifier; do
    case ""$bundle"" in
      RevenueCat_RevenueCat.bundle|RevenueCat_RevenueCatUI.bundle) ;;
      *) continue ;;
    esac
    case "" $bundles "" in *"" $bundle ""*) continue ;; esac
    remove_previous_bundle ""$app_resources""
    if [ ""$ACTION"" = install ]; then
      remove_previous_bundle ""${INSTALL_DIR}/${UNLOCALIZED_RESOURCES_FOLDER_PATH}""
    fi
  done < ""$manifest""
fi
mv ""${manifest}.tmp"" ""$manifest"" # Keep ownership information even if a subsequent copy fails.
for bundle in $bundles; do
  rsync -aL --delete ""${BUILT_PRODUCTS_DIR}/${bundle}"" ""$app_resources/""
  if [ ""$ACTION"" = install ]; then
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
