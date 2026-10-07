#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
#if UNITY_2022_3_OR_NEWER
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;
#endif

[InitializeOnLoad]
public static class RevenueCatDependencyManagerInstaller
{
    private const string UnityEdmPackage = "com.unity.external-dependency-manager";
    private const string UnityEdmVersion = "2.1.1";
    private const string Edm4uPackage = "com.google.external-dependency-manager";
    private const string SessionKey = "RevenueCat.DependencyManagerInstaller.Attempted";

#if UNITY_2022_3_OR_NEWER
    private static AddRequest _addRequest;
#endif

    static RevenueCatDependencyManagerInstaller()
    {
        if (AssetDatabase.IsAssetImportWorkerProcess())
        {
            return;
        }

        // delayCall is not invoked in batch mode, afterAssemblyReload is.
        if (Application.isBatchMode)
        {
            AssemblyReloadEvents.afterAssemblyReload += Run;
        }
        else
        {
            EditorApplication.delayCall += Run;
        }
    }

    private static void Run()
    {
        AssemblyReloadEvents.afterAssemblyReload -= Run;

        if (SessionState.GetBool(SessionKey, false) || IsAnyDependencyManagerPresent())
        {
            return;
        }
        SessionState.SetBool(SessionKey, true);

#if UNITY_2022_3_OR_NEWER
        if (Application.isBatchMode)
        {
            Debug.LogWarning($"RevenueCat: no External Dependency Manager found. Add \"{UnityEdmPackage}\": \"{UnityEdmVersion}\" " +
                             "to Packages/manifest.json, or import Google's EDM4U, before building.");
            return;
        }

        Debug.Log($"RevenueCat: no External Dependency Manager found. Installing {UnityEdmPackage}@{UnityEdmVersion} " +
                  "through the Package Manager.");
        _addRequest = Client.Add($"{UnityEdmPackage}@{UnityEdmVersion}");
        EditorApplication.update += PollAddRequest;
#else
        Debug.LogError("RevenueCat: no External Dependency Manager found. Unity's External Dependency Manager requires " +
                       "Unity 2022.3 or newer. Upgrade Unity, or import Google's EDM4U 1.2.190 " +
                       "(https://github.com/googlesamples/unity-jar-resolver) before building.");
#endif
    }

    private static bool IsAnyDependencyManagerPresent()
    {
#if UNITY_2022_3_OR_NEWER
        if (PackageInfo.IsPackageRegistered(UnityEdmPackage) || PackageInfo.IsPackageRegistered(Edm4uPackage))
        {
            return true;
        }
#endif
        return AppDomain.CurrentDomain.GetAssemblies().Any(assembly =>
        {
            var name = assembly.GetName().Name;
            return name == "Google.JarResolver" || name == "Google.IOSResolver";
        });
    }

#if UNITY_2022_3_OR_NEWER
    private static void PollAddRequest()
    {
        if (!_addRequest.IsCompleted)
        {
            return;
        }
        EditorApplication.update -= PollAddRequest;

        if (_addRequest.Status == StatusCode.Success)
        {
            Debug.Log($"RevenueCat: installed {_addRequest.Result.name}@{_addRequest.Result.version}.");
        }
        else
        {
            Debug.LogError($"RevenueCat: failed to install {UnityEdmPackage}: {_addRequest.Error?.message}. " +
                           "Install \"External Dependency Manager\" from Window > Package Manager (Unity Registry), " +
                           "or import Google's EDM4U.");
        }
        _addRequest = null;
    }
#endif
}
#endif
