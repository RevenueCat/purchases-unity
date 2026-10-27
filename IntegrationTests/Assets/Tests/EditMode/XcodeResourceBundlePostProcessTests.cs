#if UNITY_IOS
using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.iOS.Xcode;

namespace RevenueCat.Tests
{
    public class XcodeResourceBundlePostProcessTests
    {
        private const string PhaseName = "[RevenueCat] Copy Swift Package Resource Bundles";
        private const string ScriptFileName = "RevenueCatCopySwiftPackageResources.sh";

        [Test]
        public void RepeatedPostProcessRefreshesScriptAndKeepsOneAppPhase()
        {
            string exportPath = Path.Combine(Path.GetTempPath(), "RevenueCat export " + Guid.NewGuid());
            try
            {
                string projectPath = PBXProject.GetPBXProjectPath(exportPath);
                Directory.CreateDirectory(Path.GetDirectoryName(projectPath));
                var project = CreateProject();
                string mainTarget = project.GetUnityMainTargetGuid();
                string frameworkTarget = project.GetUnityFrameworkTargetGuid();
                project.WriteToFile(projectPath);

                XcodeSwiftVersionPostProcess.OnPostProcessBuild(BuildTarget.iOS, exportPath);

                string scriptPath = Path.Combine(exportPath, ScriptFileName);
                string originalScript = File.ReadAllText(scriptPath);
                Assert.That(originalScript, Does.Contain(mainTarget).And.Contain(frameworkTarget));
                Assert.That(originalScript, Does.Not.Contain("__UNITY_"));
                project.ReadFromFile(projectPath);
                string originalPhase = project.GetShellScriptBuildPhaseForTarget(
                    mainTarget, PhaseName, "/bin/sh", "/bin/sh \"$SRCROOT/" + ScriptFileName + "\"");
                Assert.That(originalPhase, Is.Not.Null);

                // Simulate appending an export after the SDK's copy script has changed.
                File.WriteAllText(scriptPath, "outdated script");
                XcodeSwiftVersionPostProcess.OnPostProcessBuild(BuildTarget.iOS, exportPath);

                Assert.That(File.ReadAllText(scriptPath), Is.EqualTo(originalScript));
                project.ReadFromFile(projectPath);
                var copyPhases = project.GetAllBuildPhasesForTarget(mainTarget)
                    .Where(phase => project.GetBuildPhaseName(phase) == PhaseName).ToArray();
                Assert.That(copyPhases, Is.EqualTo(new[] { originalPhase }));
                Assert.That(project.GetAllBuildPhasesForTarget(frameworkTarget)
                    .Any(phase => project.GetBuildPhaseName(phase) == PhaseName), Is.False);
            }
            finally
            {
                if (Directory.Exists(exportPath)) Directory.Delete(exportPath, true);
            }
        }

        private static PBXProject CreateProject()
        {
            // PBXProject cannot initialize an empty project. Seed its root and groups,
            // then use Unity's API to create the same two targets as an iOS export.
            var project = new PBXProject();
            project.ReadFromString(@"{
                archiveVersion = 1;
                objectVersion = 50;
                objects = {
                    AAAAAAAAAAAAAAAAAAAAAAAA = {
                        isa = PBXProject;
                        mainGroup = BBBBBBBBBBBBBBBBBBBBBBBB;
                        productRefGroup = CCCCCCCCCCCCCCCCCCCCCCCC;
                        buildConfigurationList = DDDDDDDDDDDDDDDDDDDDDDDD;
                        targets = ();
                    };
                    BBBBBBBBBBBBBBBBBBBBBBBB = {
                        isa = PBXGroup;
                        children = (CCCCCCCCCCCCCCCCCCCCCCCC,);
                        sourceTree = ""<group>"";
                    };
                    CCCCCCCCCCCCCCCCCCCCCCCC = {
                        isa = PBXGroup;
                        children = ();
                        name = Products;
                        sourceTree = ""<group>"";
                    };
                    DDDDDDDDDDDDDDDDDDDDDDDD = {
                        isa = XCConfigurationList;
                        buildConfigurations = (EEEEEEEEEEEEEEEEEEEEEEEE,);
                        defaultConfigurationName = Debug;
                    };
                    EEEEEEEEEEEEEEEEEEEEEEEE = {
                        isa = XCBuildConfiguration;
                        buildSettings = {};
                        name = Debug;
                    };
                };
                rootObject = AAAAAAAAAAAAAAAAAAAAAAAA;
            }");
            string mainTarget = project.AddTarget("Unity-iPhone", "app", "com.apple.product-type.application");
            project.AddTarget("UnityFramework", "framework", "com.apple.product-type.framework");
            project.AddFrameworksBuildPhase(mainTarget);
            return project;
        }
    }
}
#endif
