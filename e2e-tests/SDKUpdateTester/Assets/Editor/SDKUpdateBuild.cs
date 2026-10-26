// Created by Antonio Pallares. Copyright (c) 2026 RevenueCat, Inc.

using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Xml;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
using UnityEditor.PackageManager;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif
#if UNITY_ANDROID
using UnityEditor.Android;
#endif

static class SDKUpdateBuild
{
    private const string PackageName = "com.revenuecat.purchases-unity";
    private static string Output => Environment.GetEnvironmentVariable("SDK_UPDATE_OUTPUT");
    private static string ExpectedVersion => Environment.GetEnvironmentVariable("SDK_UPDATE_VERSION");

    public static void Build()
    {
        var purchases = SDKUpdateScene.Create();
        var scriptPath = AssetDatabase.GetAssetPath(MonoScript.FromMonoBehaviour(purchases));
        if (scriptPath != "Packages/" + PackageName + "/Scripts/Purchases.cs")
            throw new Exception("Purchases must come from the selected Unity package: " + scriptPath);

        var package = PackageInfo.FindForAssetPath(scriptPath);
        bool released = Environment.GetEnvironmentVariable("SDK_UPDATE_VARIANT") == "release";
        var expectedSource = released ? PackageSource.Registry : PackageSource.Local;
        if (package == null || package.source != expectedSource || package.version != ExpectedVersion)
            throw new Exception("Unexpected RevenueCat package source or version");
        if (!released && Path.GetFullPath(package.resolvedPath) !=
            Path.GetFullPath(Environment.GetEnvironmentVariable("SDK_UPDATE_LOCAL_PACKAGE")))
            throw new Exception("The local build must resolve this checkout's RevenueCat package");

        var iosWrapper = File.ReadAllText(Path.Combine(package.resolvedPath, "Plugins/iOS/PurchasesUnityHelper.m"));
        var androidWrapper = File.ReadAllText(Path.Combine(package.resolvedPath, "Plugins/Android/PurchasesWrapper.java"));
        var iosVersion = Regex.Match(iosWrapper, "platformFlavorVersion\\s*\\{\\s*return @\"([^\"]+)\"").Groups[1].Value;
        var androidVersion = Regex.Match(androidWrapper, "PLUGIN_VERSION = \"([^\"]+)\"").Groups[1].Value;
        if (iosVersion != package.version || androidVersion != package.version)
            throw new Exception("The Unity package and its native wrappers must have the same SDK version");

        var apiKey = Environment.GetEnvironmentVariable("MAESTRO_TEST_STORE_API_KEY");
        if (string.IsNullOrEmpty(apiKey) || !apiKey.StartsWith("test_"))
            throw new Exception("MAESTRO_TEST_STORE_API_KEY must contain a Test Store key");
        Directory.CreateDirectory(Output);
        Directory.CreateDirectory("Assets/Resources");
        File.WriteAllText("Assets/Resources/SDKUpdateBuildInfo.json", JsonUtility.ToJson(
            new SDKUpdateTestApp.BuildInfo { apiKey = apiKey, sdkVersion = package.version, sdkSource = package.source.ToString() }));
        File.WriteAllText(Path.Combine(Output, "version.txt"), package.version + "\n");
        File.WriteAllText(Path.Combine(Output, "source.txt"), package.source + "\n");
        File.WriteAllText(Path.Combine(Output, "resolved-sdk.txt"),
            $"{PackageName}@{package.version}\nsource: {package.source}\npath: {package.resolvedPath}\n" +
            $"Purchases.cs SHA256: {Hash(File.ReadAllBytes(Path.Combine(package.resolvedPath, "Scripts/Purchases.cs")))}\n" +
            $"iOS wrapper version: {iosVersion}\nAndroid wrapper version: {androidVersion}\n" +
            $"{File.ReadAllText(Path.Combine(package.resolvedPath, "Plugins/Editor/RevenueCatDependencies.xml"))}\n");
        File.Copy("Packages/packages-lock.json", Path.Combine(Output, "packages-lock.json"), true);
        AssetDatabase.Refresh();

        PlayerSettings.productName = "SDKUpdateTester";
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, "com.revenuecat.SDKUpdateTester");
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.revenuecat.SDKUpdateTester");
        PlayerSettings.Android.bundleVersionCode = released ? 1 : 2;
        PlayerSettings.Android.useCustomKeystore = false;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel35;
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.X86_64;
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.OpenGLES3 });
        PlayerSettings.iOS.sdkVersion = iOSSdkVersion.SimulatorSDK;
        PlayerSettings.iOS.simulatorSdkArchitecture = AppleMobileArchitectureSimulator.ARM64;
        EditorUserBuildSettings.buildAppBundle = false;
        EditorUserBuildSettings.exportAsGoogleAndroidProject = false;

        bool ios = EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS;
        if (!ios)
        {
            var template = "Assets/Plugins/Android/mainTemplate.gradle";
            File.WriteAllText(template, File.ReadAllText(template).Replace("SDK_UPDATE_PHC_VERSION", HybridCommonVersion()));
        }
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/Main.unity" },
            locationPathName = Path.Combine(Output, ios ? "xcode" : "SDKUpdateTester.apk"),
            target = ios ? BuildTarget.iOS : BuildTarget.Android,
            options = BuildOptions.Development
        });
        if (report.summary.result != BuildResult.Succeeded)
            throw new Exception("SDK update app build failed: " + report.summary.totalErrors + " errors");
    }

    internal static string HybridCommonVersion()
    {
        var package = PackageInfo.FindForAssetPath("Packages/" + PackageName);
        var document = new XmlDocument();
        document.Load(Path.Combine(package.resolvedPath, "Plugins/Editor/RevenueCatDependencies.xml"));
        var ios = document.SelectSingleNode("//remoteSwiftPackage").Attributes["version"].Value;
        var android = document.SelectSingleNode("//androidPackage").Attributes["spec"].Value;
        if (!android.EndsWith(":[" + ios + "]"))
            throw new Exception("RevenueCat's declared iOS and Android hybrid-common versions differ");
        return ios;
    }

    private static string Hash(byte[] bytes)
    {
        using var sha = SHA256.Create();
        return string.Concat(sha.ComputeHash(bytes).Select(value => value.ToString("x2")));
    }

#if UNITY_IOS
    [PostProcessBuild(1000)]
    public static void AddSwiftPackage(BuildTarget target, string path)
    {
        if (target != BuildTarget.iOS) return;
        var project = new PBXProject();
        var projectPath = PBXProject.GetPBXProjectPath(path);
        project.ReadFromFile(projectPath);
        var package = project.AddRemotePackageReferenceAtVersion(
            "https://github.com/RevenueCat/purchases-hybrid-common.git", HybridCommonVersion());
        project.AddRemotePackageFrameworkToProject(project.GetUnityFrameworkTargetGuid(),
            "PurchasesHybridCommon", package, false);
        project.WriteToFile(projectPath);
    }
#endif
}

#if UNITY_ANDROID
class SDKUpdateGradleDependencies : IPostGenerateGradleAndroidProject
{
    public int callbackOrder => 1000;

    public void OnPostGenerateGradleAndroidProject(string path)
    {
        var reportPath = Environment.GetEnvironmentVariable("SDK_UPDATE_OUTPUT") + "/resolved-native-dependencies.txt";
        var expected = SDKUpdateBuild.HybridCommonVersion();
        File.AppendAllText(Path.Combine(path, "build.gradle"), @"
tasks.register('verifySdkUpdateDependencies') {
    doLast {
        def components = configurations.debugRuntimeClasspath.incoming.resolutionResult.allComponents
        def revenueCat = components.collect { it.moduleVersion }.findAll { it?.group == 'com.revenuecat.purchases' }
        if (!revenueCat.any { it.name == 'purchases-hybrid-common' && it.version == '" + expected + @"' }) {
            throw new GradleException('The app must use the native dependency declared by its Unity SDK package')
        }
        new File('" + reportPath.Replace("\\", "\\\\").Replace("'", "\\'") + @"').text =
            revenueCat.collect { it.toString() }.sort().join('\n') + '\n'
    }
}
preBuild.dependsOn verifySdkUpdateDependencies
");
    }
}
#endif
