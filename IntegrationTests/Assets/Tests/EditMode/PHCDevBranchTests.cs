#if UNITY_IOS
using NUnit.Framework;
using RevenueCat.Editor;

namespace RevenueCat.Tests
{
    public class PHCDevBranchTests
    {
        [Test]
        public void ChangesOnlyPHCAndPreservesProductReferences()
        {
            const string project = @"
		111111111111111111111111 /* PHC */ = {
			isa = XCRemoteSwiftPackageReference;
			repositoryURL = ""https://github.com/RevenueCat/purchases-hybrid-common.git"";
			requirement = {
				kind = exactVersion;
				version = 20.8.0;
			};
		};
		222222222222222222222222 /* Other */ = {
			isa = XCRemoteSwiftPackageReference;
			repositoryURL = ""https://github.com/example/other.git"";
			requirement = { kind = exactVersion; version = 1.0.0; };
		};
		333333333333333333333333 /* Product */ = {
			isa = XCSwiftPackageProductDependency;
			package = 111111111111111111111111;
			productName = PurchasesHybridCommonUI;
		};
";
            var updated = RevenueCatPHCDevBranch.UseDevBranch(project);
            StringAssert.Contains("kind = branch; branch = \"21.0-dev\";", updated);
            StringAssert.DoesNotContain("version = 20.8.0", updated);
            StringAssert.Contains("kind = exactVersion; version = 1.0.0;", updated);
            StringAssert.Contains("package = 111111111111111111111111;", updated);
            Assert.AreEqual(updated, RevenueCatPHCDevBranch.UseDevBranch(updated));
        }
    }
}
#endif
