
#if UNITY_EDITOR && UNITY_IOS

using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

public static class XcodeSwiftVersionPostProcess
{
    // set callbackOrder to 999 to ensure this runs as the last post process step
    [PostProcessBuild(999)]
    public static void OnPostProcessBuild(BuildTarget buildTarget, string path)
    {
        if (buildTarget == BuildTarget.iOS)
        {
            Debug.Log("Installing for iOS. Setting ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES to NO and adding StoreKit");
            ModifyFrameworks(path);
            AddStoreKitFramework(path);
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

}
#endif
