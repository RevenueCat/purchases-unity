using UnityEditor;
using System.Linq;
using System;
using Google;

public class BuildCommand
{
    private const string BUILD_OPTIONS_ENV_VAR = "BuildOptions";
    private const string SCRIPTING_BACKEND_ENV_VAR = "SCRIPTING_BACKEND";

    public static string GetArgument(string name)
    {
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].Contains(name))
            {
                return args[i + 1];
            }
        }
        return null;
    }

    public static string[] GetEnabledScenes()
    {
        return (
            from scene in EditorBuildSettings.scenes
            where scene.enabled
            where !string.IsNullOrEmpty(scene.path)
            select scene.path
        ).ToArray();
    }

    public static BuildTarget GetBuildTarget()
    {
        string buildTargetName = GetArgument("customBuildTarget");
        Console.WriteLine(":: Received customBuildTarget " + buildTargetName);

        if (buildTargetName.ToLower() == "android")
        {
#if !UNITY_5_6_OR_NEWER
			// https://issuetracker.unity3d.com/issues/buildoptions-dot-acceptexternalmodificationstoplayer-causes-unityexception-unknown-project-type-0
			// Fixed in Unity 5.6.0
			// side effect to fix android build system:
			EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Internal;
#endif
        }

        if (TryConvertToEnum(buildTargetName, out BuildTarget target))
            return target;

        Console.WriteLine($":: {nameof(buildTargetName)} \"{buildTargetName}\" not defined on enum {nameof(BuildTarget)}, using {nameof(BuildTarget.NoTarget)} enum to build");

        return BuildTarget.NoTarget;
    }

    public static string GetBuildPath()
    {
        string buildPath = GetArgument("customBuildPath");
        Console.WriteLine(":: Received customBuildPath " + buildPath);
        if (buildPath == "")
        {
            throw new Exception("customBuildPath argument is missing");
        }
        return buildPath;
    }

    public static string GetBuildName()
    {
        string buildName = GetArgument("customBuildName");
        Console.WriteLine(":: Received customBuildName " + buildName);
        if (buildName == "")
        {
            throw new Exception("customBuildName argument is missing");
        }
        return buildName;
    }

    public static string GetFixedBuildPath(BuildTarget buildTarget, string buildPath, string buildName)
    {
        if (buildTarget.ToString().ToLower().Contains("windows")) {
            buildName += ".exe";
        } else if (buildTarget == BuildTarget.Android) {
#if UNITY_2018_3_OR_NEWER
            buildName += EditorUserBuildSettings.buildAppBundle ? ".aab" : ".apk";
#else
            buildName += ".apk";
#endif
        }
        return buildPath + buildName;
    }

    public static BuildOptions GetBuildOptions()
    {
        if (TryGetEnv(BUILD_OPTIONS_ENV_VAR, out string envVar)) {
            string[] allOptionVars = envVar.Split(',');
            BuildOptions allOptions = BuildOptions.None;
            BuildOptions option;
            string optionVar;
            int length = allOptionVars.Length;

            Console.WriteLine($":: Detecting {BUILD_OPTIONS_ENV_VAR} env var with {length} elements ({envVar})");

            for (int i = 0; i < length; i++) {
                optionVar = allOptionVars[i];

                if (TryConvertToEnum(optionVar, out option)) {
                    allOptions |= option;
                }
                else {
                    Console.WriteLine($":: Cannot convert {optionVar} to {nameof(BuildOptions)} enum, skipping it.");
                }
            }

            return allOptions;
        }

        return BuildOptions.None;
    }

    // https://stackoverflow.com/questions/1082532/how-to-tryparse-for-enum-value
    public static bool TryConvertToEnum<TEnum>(string strEnumValue, out TEnum value)
    {
        if (!Enum.IsDefined(typeof(TEnum), strEnumValue))
        {
            value = default;
            return false;
        }

        value = (TEnum)Enum.Parse(typeof(TEnum), strEnumValue);
        return true;
    }

    public static bool TryGetEnv(string key, out string value)
    {
        value = Environment.GetEnvironmentVariable(key);
        return !string.IsNullOrEmpty(value);
    }

    public static void SetScriptingBackendFromEnv(BuildTarget platform) {
        var targetGroup = BuildPipeline.GetBuildTargetGroup(platform);
        if (TryGetEnv(SCRIPTING_BACKEND_ENV_VAR, out string scriptingBackend)) {
            if (TryConvertToEnum(scriptingBackend, out ScriptingImplementation backend)) {
                Console.WriteLine($":: Setting ScriptingBackend to {backend}");
                PlayerSettings.SetScriptingBackend(targetGroup, backend);
            } else {
                string possibleValues = string.Join(", ", Enum.GetValues(typeof(ScriptingImplementation)).Cast<ScriptingImplementation>());
                throw new Exception($"Could not find '{scriptingBackend}' in ScriptingImplementation enum. Possible values are: {possibleValues}");
            }
        } else {
            var defaultBackend = PlayerSettings.GetDefaultScriptingBackend(targetGroup);
            Console.WriteLine($":: Using project's configured ScriptingBackend (should be {defaultBackend} for tagetGroup {targetGroup}");
        }
    }

    public static void Resolve()
    {
        Console.WriteLine(":::::::: AndroidSdkRoot " + Environment.GetEnvironmentVariable("ANDROID_HOME"));
        Console.WriteLine(":::::::: JdkPath " + Environment.GetEnvironmentVariable("JAVA_HOME"));
        
        // I was facing this error
        // https://forum.unity.com/threads/unable-to-find-java-android-sdk-google-play-services-in-unity.694576/
        EditorPrefs.SetString("AndroidSdkRoot", Environment.GetEnvironmentVariable("ANDROID_HOME"));
        EditorPrefs.SetString("JdkPath", Environment.GetEnvironmentVariable("JAVA_HOME"));
        EditorPrefs.SetInt("JdkUseEmbedded", 0);
        
        Console.WriteLine(":: Resolving");
        
        VersionHandler.UpdateCompleteMethods = new [] {
            ":BuildCommand:ResolverEnabled"
        };
        VersionHandler.UpdateNow();
    }
    
    public static void ResolverEnabled()
    {
        Console.WriteLine(":: ResolverEnabled");
        VersionHandler.UpdateCompleteMethods = new string[0];
        
        var buildTarget = GetBuildTarget();
        
        var result = true;
        if (buildTarget == BuildTarget.Android)
        {
            result = (bool) VersionHandler.InvokeStaticMethod(
                VersionHandler.FindClass("Google.JarResolver",
                    "GooglePlayServices.PlayServicesResolver"),
                "ResolveSync", args: new object[] {true},
                namedArgs: null);
        }
        
        Console.WriteLine(":: ResolveSync Result " + result);
        EditorApplication.Exit(result ? 0 : 1);
    }

    public static void PerformBuild()
    {
        Console.WriteLine(":: Performing build");

        var buildTarget = GetBuildTarget();

        // if (buildTarget == BuildTarget.iOS) PlayerSettings.iOS.sdkVersion = iOSSdkVersion.SimulatorSDK;
        
        EditorPrefs.SetBool("Google.IOSResolver.PodfileGenerationEnabled", true);

        var buildPath      = GetBuildPath();
        var buildName      = GetBuildName();
        var buildOptions   = GetBuildOptions();
        var fixedBuildPath = GetFixedBuildPath(buildTarget, buildPath, buildName);

        SetScriptingBackendFromEnv(buildTarget);

        var buildReport = BuildPipeline.BuildPlayer(GetEnabledScenes(), fixedBuildPath, buildTarget, buildOptions);

        if (buildReport.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            throw new Exception($"Build ended with {buildReport.summary.result} status");

        Console.WriteLine(":: Done with build");
    }

}
