using UnityEditor;
using UnityEditor.Build.Reporting;
using System;
using System.IO;
using System.Linq;
using Google;

static class BuildScript
{
    private static readonly string BuildPathIOS = "build/ios";
    private static readonly string BuildPathAndroid = "build/android/MaestroTestApp.apk";

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

        // Maestro drives the app on the iOS Simulator, so IL2CPP has to emit a simulator
        // slice. With the default device SDK the generated il2cpp.a is device-only and
        // linking UnityFramework for iphonesimulator fails. The simulator architecture
        // defaults to the deprecated x86_64, which the Apple silicon CI runners can't
        // link against.
        PlayerSettings.iOS.sdkVersion = iOSSdkVersion.SimulatorSDK;
        PlayerSettings.iOS.simulatorSdkArchitecture = AppleMobileArchitectureSimulator.ARM64;

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

    public static void Resolve()
    {
        var sdkRoot = Environment.GetEnvironmentVariable("ANDROID_HOME_GAME_CI");
        var jdkPath = Environment.GetEnvironmentVariable("JAVA_HOME_GAME_CI");
        if (!string.IsNullOrEmpty(sdkRoot))
        {
            EditorPrefs.SetString("AndroidSdkRoot", sdkRoot);
        }
        if (!string.IsNullOrEmpty(jdkPath))
        {
            EditorPrefs.SetString("JdkPath", jdkPath);
            EditorPrefs.SetInt("JdkUseEmbedded", 0);
        }

        Console.WriteLine(":: Resolving Android dependencies");
        // PlayServicesResolver initializes lazily, so resolution is chained through the Version Handler.
        VersionHandler.UpdateCompleteMethods = new[] { ":BuildScript:ResolverEnabled" };
        VersionHandler.UpdateNow();
    }

    public static void ResolverEnabled()
    {
        VersionHandler.UpdateCompleteMethods = new string[0];
        var result = (bool)VersionHandler.InvokeStaticMethod(
            VersionHandler.FindClass("Google.JarResolver", "GooglePlayServices.PlayServicesResolver"),
            "ResolveSync", args: new object[] { true }, namedArgs: null);
        Console.WriteLine(":: ResolveSync result " + result);
        EditorApplication.Exit(result ? 0 : 1);
    }
}
