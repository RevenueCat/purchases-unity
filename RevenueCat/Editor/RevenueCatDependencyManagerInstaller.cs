#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.PackageManager;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;
#if UNITY_2022_3_OR_NEWER
using UnityEditor.PackageManager.Requests;
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
        if (!IsSupportedEditor())
        {
            LogUnsupportedEditor();
            return;
        }

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
        LogUnsupportedEditor();
#endif
    }

    private static void LogUnsupportedEditor()
    {
        Debug.LogError($"RevenueCat: no External Dependency Manager found, and Unity's External Dependency Manager requires " +
                       $"Unity 2022.3.33 or newer (this editor is {Application.unityVersion}). The RevenueCat SDK requires " +
                       "Unity 2022.3.33 or newer; please upgrade Unity.");
    }

#if UNITY_2022_3_OR_NEWER
    // Unity has no patch-level define symbol, and 2022.3.0 to 2022.3.32 lack the PackageInfo APIs Unity's EDM uses.
    private static bool IsSupportedEditor()
    {
        var parts = Application.unityVersion.Split('.');
        if (parts.Length < 3 || !int.TryParse(parts[0], out var major) || !int.TryParse(parts[1], out var minor))
        {
            return true;
        }
        if (major != 2022 || minor != 3)
        {
            return true;
        }
        var patchDigits = new string(parts[2].TakeWhile(char.IsDigit).ToArray());
        return int.TryParse(patchDigits, out var patch) && patch >= 33;
    }
#endif

    private static bool IsAnyDependencyManagerPresent()
    {
        if (PackageInfo.GetAllRegisteredPackages().Any(package =>
                package.name == UnityEdmPackage || package.name == Edm4uPackage))
        {
            return true;
        }
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
