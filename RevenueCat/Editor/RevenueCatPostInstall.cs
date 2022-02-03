using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
 
namespace Editor
{
    public static class XcodeSwiftVersionPostProcess
    {
        [PostProcessBuild(999)]
        public static void OnPostProcessBuild(BuildTarget buildTarget, string path)
        {
            if (buildTarget == BuildTarget.iOS)
            {
                ModifyFrameworks(path);
                AddStoreKitFramework(path);
                SaveProject(path);
            }
        }
 
        private static void ModifyFrameworks(string path)
        {
            var project = GetProject(path);
 
            string mainTargetGuid = project.GetUnityMainTargetGuid();
           
            foreach (var targetGuid in new[] { mainTargetGuid, project.GetUnityFrameworkTargetGuid() })
            {
                project.SetBuildProperty(targetGuid, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "NO");
            }
           
            project.SetBuildProperty(mainTargetGuid, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "YES");
        }

        private static void AddStoreKitFramework(string path)
        {
            var project = GetProject(path);
            string mainTargetGUID = PBXProject.GetUnityMainTargetGuid();
            project.AddFrameworkToProject(mainTargetGUID, "StoreKit", false);
        }

        private static PBXProject GetProject(string path)
        {
            string projPath = PBXProject.GetPBXProjectPath(path);
            var project = new PBXProject();
            project.ReadFromFile(projPath);
            return project
        }

        private static void SaveProject(string path)
        {
            string projPath = PBXProject.GetPBXProjectPath(path);
            var project = GetProject(path);
            project.WriteToFile(projPath);
        }
        
    }
}
