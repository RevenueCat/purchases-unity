using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
using System;
using System.IO;
using System.Linq;
using System.Xml;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

static class BuildScript
{
    private static readonly string BuildPathIOS = "build/ios";
    private static readonly string BuildPathAndroid = "build/android/MaestroTestApp.apk";

    // The MaestroTestApp doesn't ship the External Dependency Manager, so the
    // native PurchasesHybridCommon dependency is injected here via Swift Package
    // Manager instead of CocoaPods. This mirrors the SPM integration path used
    // by customers and keeps the RevenueCat native `.m` files (compiled into the
    // UnityFramework target) able to resolve their `@import PurchasesHybridCommon`
    // (and transitively `RevenueCat`) modules.
    private const string PhcSwiftPackageUrl = "https://github.com/RevenueCat/purchases-hybrid-common.git";
    private const string PhcDependenciesPath =
        "Packages/com.revenuecat.purchases-unity/Plugins/Editor/RevenueCatDependencies.xml";

    static string[] GetEnabledScenes()
    {
        return (
            from scene in EditorBuildSettings.scenes
            where scene.enabled
            where !string.IsNullOrEmpty(scene.path)
            select scene.path
        ).ToArray();
    }

    [MenuItem("Build/Build iOS")]
    public static void BuildIOS()
    {
        SceneSetup.SetupScene();
        var scenes = GetEnabledScenes();
        if (scenes.Length == 0)
        {
            Console.WriteLine(":: No scenes found in EditorBuildSettings, looking for scene files...");
            scenes = Directory.GetFiles("Assets/Scenes", "*.unity", SearchOption.AllDirectories);
        }

        if (scenes.Length == 0)
        {
            throw new Exception("No scenes found to build.");
        }

        Console.WriteLine(":: Building iOS with scenes:");
        foreach (var scene in scenes)
        {
            Console.WriteLine("::   " + scene);
        }

        var buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = BuildPathIOS,
            target = BuildTarget.iOS,
            options = BuildOptions.None
        };

        var report = BuildPipeline.BuildPlayer(buildPlayerOptions);

        if (report.summary.result != BuildResult.Succeeded)
        {
            throw new Exception("Build failed with " + report.summary.totalErrors + " error(s)");
        }

        Console.WriteLine(":: Build succeeded. Output: " + BuildPathIOS);
    }

    [MenuItem("Build/Build Android")]
    public static void BuildAndroid()
    {
        SceneSetup.SetupScene();
        var scenes = GetEnabledScenes();
        if (scenes.Length == 0)
        {
            Console.WriteLine(":: No scenes found in EditorBuildSettings, looking for scene files...");
            scenes = Directory.GetFiles("Assets/Scenes", "*.unity", SearchOption.AllDirectories);
        }

        if (scenes.Length == 0)
        {
            throw new Exception("No scenes found to build.");
        }

        Console.WriteLine(":: Building Android with scenes:");
        foreach (var scene in scenes)
        {
            Console.WriteLine("::   " + scene);
        }

        var buildDir = Path.GetDirectoryName(BuildPathAndroid);
        if (!Directory.Exists(buildDir))
        {
            Directory.CreateDirectory(buildDir);
        }

        var buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = BuildPathAndroid,
            target = BuildTarget.Android,
            // A development build produces a debuggable APK, which the RevenueCat SDK
            // requires before it accepts the Test Store API key the E2E tests configure.
            // A release build instead shows a "Test Store API key used in release build"
            // dialog and closes the app before any test can run.
            options = BuildOptions.Development
        };

        var report = BuildPipeline.BuildPlayer(buildPlayerOptions);

        if (report.summary.result != BuildResult.Succeeded)
        {
            throw new Exception("Build failed with " + report.summary.totalErrors + " error(s)");
        }

        Console.WriteLine(":: Build succeeded. Output: " + BuildPathAndroid);
    }

#if UNITY_IOS
    [PostProcessBuild(1000)]
    public static void AddPurchasesHybridCommonSwiftPackage(BuildTarget target, string pathToBuiltProject)
    {
        if (target != BuildTarget.iOS)
        {
            return;
        }

        var projectPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
        var project = new PBXProject();
        project.ReadFromFile(projectPath);

        // RevenueCat's native plugin `.m` files are compiled into the
        // UnityFramework target, so the Swift package must be linked there.
        var frameworkTargetGuid = project.GetUnityFrameworkTargetGuid();
        var version = ResolvePurchasesHybridCommonVersion();

        var packageGuid = project.AddRemotePackageReferenceAtVersion(PhcSwiftPackageUrl, version);
        project.AddRemotePackageFrameworkToProject(frameworkTargetGuid, "PurchasesHybridCommon", packageGuid, false);
        project.AddRemotePackageFrameworkToProject(frameworkTargetGuid, "PurchasesHybridCommonUI", packageGuid, false);

        project.WriteToFile(projectPath);

        Console.WriteLine($":: Added PurchasesHybridCommon Swift Package ({version}) to the UnityFramework target");
    }

    private static string ResolvePurchasesHybridCommonVersion()
    {
        if (!File.Exists(PhcDependenciesPath))
        {
            throw new Exception($"Could not find RevenueCat dependencies file at {PhcDependenciesPath}");
        }

        var document = new XmlDocument();
        document.Load(PhcDependenciesPath);

        var version = document.SelectSingleNode("//remoteSwiftPackage")?.Attributes?["version"]?.Value;
        if (string.IsNullOrEmpty(version))
        {
            throw new Exception($"Could not resolve the PurchasesHybridCommon Swift Package version from {PhcDependenciesPath}");
        }

        return version;
    }
#endif
}
