﻿// Adapted from https://github.com/game-ci/unity3d-ci-example/blob/master/Assets/Scripts/Editor/BuildCommand.cs

using UnityEditor;
using System.Linq;
using System;
using System.IO;
using Google;

static class BuildCommand
{
    private const string KEYSTORE_PASS  = "KEYSTORE_PASS";
    private const string KEY_ALIAS_PASS = "KEY_ALIAS_PASS";
    private const string KEY_ALIAS_NAME = "KEY_ALIAS_NAME";
    private const string KEYSTORE       = "keystore.keystore";
    private const string BUILD_OPTIONS_ENV_VAR = "BuildOptions";
    private const string ANDROID_BUNDLE_VERSION_CODE = "BUNDLE_VERSION_CODE";
    private const string ANDROID_APP_BUNDLE = "BUILD_APP_BUNDLE";
    private const string SCRIPTING_BACKEND_ENV_VAR = "SCRIPTING_BACKEND";
    private const string VERSION_NUMBER_VAR = "VERSION_NUMBER_VAR";
    
    static string GetArgument(string name)
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

    static string[] GetEnabledScenes()
    {
        return (
            from scene in EditorBuildSettings.scenes
            where scene.enabled
            where !string.IsNullOrEmpty(scene.path)
            select scene.path
        ).ToArray();
    }

    static BuildTarget GetBuildTarget()
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

        if (buildTargetName.TryConvertToEnum(out BuildTarget target))
            return target;

        Console.WriteLine($":: {nameof(buildTargetName)} \"{buildTargetName}\" not defined on enum {nameof(BuildTarget)}, using {nameof(BuildTarget.NoTarget)} enum to build");

        return BuildTarget.NoTarget;
    }

    static string GetBuildPath()
    {
        string buildPath = GetArgument("customBuildPath");
        Console.WriteLine(":: Received customBuildPath " + buildPath);
        if (buildPath == "")
        {
            throw new Exception("customBuildPath argument is missing");
        }
        return buildPath;
    }

    static string GetBuildName()
    {
        string buildName = GetArgument("customBuildName");
        Console.WriteLine(":: Received customBuildName " + buildName);
        if (buildName == "")
        {
            throw new Exception("customBuildName argument is missing");
        }
        return buildName;
    }

    static string GetFixedBuildPath(BuildTarget buildTarget, string buildPath, string buildName)
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

    static BuildOptions GetBuildOptions()
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

                if (optionVar.TryConvertToEnum(out option)) {
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
    
    public static void Resolve()
    {
        Console.WriteLine(":::::::: AndroidSdkRoot " + Environment.GetEnvironmentVariable("ANDROID_HOME_GAME_CI"));
        Console.WriteLine(":::::::: JdkPath " + Environment.GetEnvironmentVariable("JAVA_HOME_GAME_CI"));
        
        // I was facing this error
        // https://forum.unity.com/threads/unable-to-find-java-android-sdk-google-play-services-in-unity.694576/
        EditorPrefs.SetString("AndroidSdkRoot", Environment.GetEnvironmentVariable("ANDROID_HOME_GAME_CI"));
        EditorPrefs.SetString("JdkPath", Environment.GetEnvironmentVariable("JAVA_HOME_GAME_CI"));
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

    // https://stackoverflow.com/questions/1082532/how-to-tryparse-for-enum-value
    static bool TryConvertToEnum<TEnum>(this string strEnumValue, out TEnum value)
    {
        if (!Enum.IsDefined(typeof(TEnum), strEnumValue))
        {
            value = default;
            return false;
        }

        value = (TEnum)Enum.Parse(typeof(TEnum), strEnumValue);
        return true;
    }

    static bool TryGetEnv(string key, out string value)
    {
        value = Environment.GetEnvironmentVariable(key);
        return !string.IsNullOrEmpty(value);
    }

    static void SetScriptingBackendFromEnv(BuildTarget platform) {
        var targetGroup = BuildPipeline.GetBuildTargetGroup(platform);
        if (TryGetEnv(SCRIPTING_BACKEND_ENV_VAR, out string scriptingBackend)) {
            if (scriptingBackend.TryConvertToEnum(out ScriptingImplementation backend)) {
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

    static void PerformBuild()
    {
        Console.WriteLine(":: Performing build");
        if (TryGetEnv(VERSION_NUMBER_VAR, out string bundleVersionNumber))
        {
            Console.WriteLine($":: Setting bundleVersionNumber to {bundleVersionNumber}");
            PlayerSettings.bundleVersion = bundleVersionNumber;
        }
        
        var buildTarget = GetBuildTarget();

        if (buildTarget == BuildTarget.Android) {
            HandleAndroidAppBundle();
            HandleAndroidBundleVersionCode();
            HandleAndroidKeystore();
        }

        var buildPath      = GetBuildPath();
        var buildName      = GetBuildName();
        var buildOptions   = GetBuildOptions();
        var fixedBuildPath = GetFixedBuildPath(buildTarget, buildPath, buildName);

        SetScriptingBackendFromEnv(buildTarget);

        // setting the scene manually because I was getting an error "can't build untitled scene." for iOS
        string[] scenes = { "Assets/Scenes/Main.unity" };
        var buildReport = BuildPipeline.BuildPlayer(scenes, fixedBuildPath, buildTarget, buildOptions);

        if (buildReport.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            throw new Exception($"Build ended with {buildReport.summary.result} status");

        Console.WriteLine(":: Done with build");
    }

    private static void HandleAndroidAppBundle()
    {
        if (TryGetEnv(ANDROID_APP_BUNDLE, out string value))
        {
#if UNITY_2018_3_OR_NEWER
            if (bool.TryParse(value, out bool buildAppBundle))
            {
                EditorUserBuildSettings.buildAppBundle = buildAppBundle;
                Console.WriteLine($":: {ANDROID_APP_BUNDLE} env var detected, set buildAppBundle to {value}.");
            }
            else
            {
                Console.WriteLine($":: {ANDROID_APP_BUNDLE} env var detected but the value \"{value}\" is not a boolean.");

            }
#else
            Console.WriteLine($":: {ANDROID_APP_BUNDLE} env var detected but does not work with lower Unity version than 2018.3");
#endif
        }
    }

    private static void HandleAndroidBundleVersionCode()
    {
        if (TryGetEnv(ANDROID_BUNDLE_VERSION_CODE, out string value))
        {
            if (int.TryParse(value, out int version))
            {
                PlayerSettings.Android.bundleVersionCode = version;
                Console.WriteLine($":: {ANDROID_BUNDLE_VERSION_CODE} env var detected, set the bundle version code to {value}.");
            }
            else
                Console.WriteLine($":: {ANDROID_BUNDLE_VERSION_CODE} env var detected but the version value \"{value}\" is not an integer.");
        }
    }

    private static void HandleAndroidKeystore()
    {
#if UNITY_2019_1_OR_NEWER
        PlayerSettings.Android.useCustomKeystore = false;
#endif

        if (!File.Exists(KEYSTORE)) {
            Console.WriteLine($":: {KEYSTORE} not found, skipping setup, using Unity's default keystore");
            return;    
        }

        PlayerSettings.Android.keystoreName = KEYSTORE;

        string keystorePass;
        string keystoreAliasPass;

        if (TryGetEnv(KEY_ALIAS_NAME, out string keyaliasName)) {
            PlayerSettings.Android.keyaliasName = keyaliasName;
            Console.WriteLine($":: using ${KEY_ALIAS_NAME} env var on PlayerSettings");
        } else {
            Console.WriteLine($":: ${KEY_ALIAS_NAME} env var not set, using Project's PlayerSettings");
        }

        if (!TryGetEnv(KEYSTORE_PASS, out keystorePass)) {
            Console.WriteLine($":: ${KEYSTORE_PASS} env var not set, skipping setup, using Unity's default keystore");
            return;
        }

        if (!TryGetEnv(KEY_ALIAS_PASS, out keystoreAliasPass)) {
            Console.WriteLine($":: ${KEY_ALIAS_PASS} env var not set, skipping setup, using Unity's default keystore");
            return;
        }
#if UNITY_2019_1_OR_NEWER
        PlayerSettings.Android.useCustomKeystore = true;
#endif
        PlayerSettings.Android.keystorePass = keystorePass;
        PlayerSettings.Android.keyaliasPass = keystoreAliasPass;
    }
}