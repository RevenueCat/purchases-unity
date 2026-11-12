#if UNITY_IOS
using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

namespace RevenueCat.Editor
{
    // TODO: before merging 11.0-dev into main, delete this temporary export override and its .meta. (SDK-4602)
    // Also remove PHCDevBranchTests, its .meta, and its CircleCI test-filter entry after pinning the PHC 21 release.
    public static class RevenueCatPHCDevBranch
    {
        [PostProcessBuild(1100)]
        public static void OnPostProcessBuild(BuildTarget target, string path)
        {
            if (target != BuildTarget.iOS) return;
            var projectPath = PBXProject.GetPBXProjectPath(path);
            var original = File.ReadAllText(projectPath);
            var updated = UseDevBranch(original);
            if (updated != original) File.WriteAllText(projectPath, updated);
        }

        public static string UseDevBranch(string project)
        {
            // Preserve the resolver's product references; change only PHC's requirement.
            var objects = new Regex(
                @"(?ms)^(?<indent>[ \t]+)[A-Fa-f0-9]{24}(?: /\*.*?\*/)? = \{\r?\n(?<body>.*?)^\k<indent>\};");
            return objects.Replace(project, match =>
            {
                var body = match.Groups["body"].Value;
                if (!body.Contains("isa = XCRemoteSwiftPackageReference;") ||
                    !body.Contains("github.com/RevenueCat/purchases-hybrid-common")) return match.Value;

                var requirement = new Regex(@"requirement = \{[^}]*\};");
                if (requirement.Matches(body).Count != 1)
                    throw new InvalidOperationException("Expected one PHC Swift package requirement");

                var updated = requirement.Replace(body,
                    "requirement = { kind = branch; branch = \"21.0-dev\"; };");
                return match.Value.Replace(body, updated);
            });
        }
    }
}
#endif
