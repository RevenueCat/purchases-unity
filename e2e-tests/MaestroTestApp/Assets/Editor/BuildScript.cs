using UnityEditor;
using UnityEditor.Build.Reporting;
using System;
using System.IO;
using System.Linq;

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
            options = BuildOptions.None
        };

        var report = BuildPipeline.BuildPlayer(buildPlayerOptions);

        if (report.summary.result != BuildResult.Succeeded)
        {
            throw new Exception("Build failed with " + report.summary.totalErrors + " error(s)");
        }

        Console.WriteLine(":: Build succeeded. Output: " + BuildPathAndroid);
    }
}
