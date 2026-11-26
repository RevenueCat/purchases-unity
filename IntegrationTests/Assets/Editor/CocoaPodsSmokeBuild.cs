using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class CocoaPodsSmokeBuild
{
    // Both tested resolvers generate at 40 and install at 50. Keep this hook in
    // the test project: replacing the generated declarations avoids duplicate sources.
    [PostProcessBuild(45)]
    public static void InstallGitPodfile(BuildTarget target, string path)
    {
        if (target != BuildTarget.iOS ||
            Environment.GetEnvironmentVariable("RC_GIT_OVERRIDE_ENABLED") != "1") return;
        var podfile = Path.Combine(path, "Podfile");
        if (!File.Exists(podfile))
            throw new InvalidOperationException("Expected the resolver's Podfile before callback 45");
        File.Copy(GitPodfilePath(), podfile, true);
    }

    private static string GitPodfilePath()
    {
        return Path.GetFullPath(Path.Combine(Application.dataPath,
            "../../e2e-tests/CocoaPodsSmokeTest/Podfile"));
    }

    public static void Build()
    {
        var output = Environment.GetEnvironmentVariable("COCOAPODS_SMOKE_BUILD_PATH");
        if (string.IsNullOrEmpty(output))
            throw new InvalidOperationException("COCOAPODS_SMOKE_BUILD_PATH is required");
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var smokeObject = new GameObject("CocoaPodsSmokeTest", typeof(Purchases), typeof(CocoaPodsSmokeTest));
        smokeObject.GetComponent<Purchases>().useRuntimeSetup = true;
        const string scenePath = "Assets/Scenes/CocoaPodsSmokeTest.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        PlayerSettings.productName = "CocoaPodsSmokeTest";
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, "com.revenuecat.unitycocoapodssmoke");
        PlayerSettings.iOS.sdkVersion = iOSSdkVersion.SimulatorSDK;
        PlayerSettings.iOS.simulatorSdkArchitecture = AppleMobileArchitectureSimulator.ARM64;

        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { scenePath }, locationPathName = output,
            target = BuildTarget.iOS,
            options = BuildOptions.None
        });
        if (report.summary.result != BuildResult.Succeeded || report.summary.totalErrors != 0)
            throw new InvalidOperationException("CocoaPods smoke export failed: " + report.summary.result);
        if (File.ReadAllText(Path.Combine(output, "Podfile")) != File.ReadAllText(GitPodfilePath()))
            throw new InvalidOperationException("Unity export changed the Git-sourced Podfile");
    }
}
