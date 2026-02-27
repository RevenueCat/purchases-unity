using UnityEditor;
using UnityEditor.Build.Reporting;
using System;
using System.IO;
using System.Linq;

static class BuildScript
{
    private static readonly string BuildPath = "build/ios";

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
            locationPathName = BuildPath,
            target = BuildTarget.iOS,
            options = BuildOptions.None
        };

        var report = BuildPipeline.BuildPlayer(buildPlayerOptions);

        if (report.summary.result != BuildResult.Succeeded)
        {
            throw new Exception("Build failed with " + report.summary.totalErrors + " error(s)");
        }

        Console.WriteLine(":: Build succeeded. Output: " + BuildPath);
    }
}
