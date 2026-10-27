#!/usr/bin/env python3
"""Run the SDK's actual Xcode resource-copy script against temporary build products.

Run on macOS with: python3 scripts/test-ios-resource-bundles.py
Uses the system PlistBuddy and rsync; no Unity installation or Python packages are needed.
"""

import os
from pathlib import Path
import plistlib
import re
import shlex
import shutil
import subprocess
import sys
import tempfile
import unittest


ROOT = Path(__file__).resolve().parents[1]
FRAMEWORK_GUID = "111111111111111111111111"
MAIN_GUID = "222222222222222222222222"
CORE = "RevenueCat_RevenueCat.bundle"
UI = "RevenueCat_RevenueCatUI.bundle"


class ResourceBundleTests(unittest.TestCase):
    def setUp(self):
        source = (ROOT / "RevenueCat/Editor/RevenueCatPostInstall.cs").read_text()
        match = re.search(
            r'private const string CopyResourceBundlesScript = @"((?:[^"]|"")*)";',
            source,
        )
        self.assertIsNotNone(match, "Could not extract the SDK's resource-copy script")
        self.script = (
            match[1].replace('""', '"')
            .replace("__UNITY_FRAMEWORK_GUID__", FRAMEWORK_GUID)
            .replace("__UNITY_MAIN_GUID__", MAIN_GUID)
        )
        temporary = tempfile.TemporaryDirectory(prefix="RevenueCat resource tests ")
        self.addCleanup(temporary.cleanup)
        self.root = Path(temporary.name)
        self.products = self.root / "Build Products"
        self.project = self.root / "Unity-iPhone.xcodeproj"
        self.app = self.root / "Target/My App.app"
        self.installed_app = self.root / "Install/My App.app"
        for path in [self.products, self.project, self.app]:
            path.mkdir(parents=True)
        self.env = dict(
            os.environ,
            PROJECT_FILE_PATH=str(self.project),
            BUILT_PRODUCTS_DIR=str(self.products),
            TARGET_BUILD_DIR=str(self.app.parent),
            UNLOCALIZED_RESOURCES_FOLDER_PATH=self.app.name,
            INSTALL_DIR=str(self.installed_app.parent),
            ACTION="build",
        )
        self.write_project(["PurchasesHybridCommon", "PurchasesHybridCommonUI"])
        for name in [CORE, UI]:
            self.make_bundle(self.products / name, "spm." + name, "SPM resources")

    def write_project(self, products, orphaned_products=(), main_products=()):
        objects = {
            FRAMEWORK_GUID: {
                "isa": "PBXNativeTarget",
                "name": "UnityFramework",
                "packageProductDependencies": [],
            },
            MAIN_GUID: {
                "isa": "PBXNativeTarget",
                "name": "Unity-iPhone",
                "packageProductDependencies": [],
            },
        }
        for index, name in enumerate(list(products) + list(main_products) + list(orphaned_products), 1):
            guid = f"{index:024X}"
            objects[guid] = {"isa": "XCSwiftPackageProductDependency", "productName": name}
            if index <= len(products):
                objects[FRAMEWORK_GUID]["packageProductDependencies"].append(guid)
            elif index <= len(products) + len(main_products):
                objects[MAIN_GUID]["packageProductDependencies"].append(guid)
        for guid in [FRAMEWORK_GUID, MAIN_GUID]:
            if not objects[guid]["packageProductDependencies"]:
                del objects[guid]["packageProductDependencies"]
        (self.project / "project.pbxproj").write_bytes(plistlib.dumps({"objects": objects}))

    def make_bundle(self, path, identifier, payload):
        path.mkdir(parents=True, exist_ok=True)
        (path / "Info.plist").write_bytes(plistlib.dumps({"CFBundleIdentifier": identifier}))
        (path / "payload.txt").write_text(payload)

    def run_script(self, expected_exit=0):
        result = subprocess.run(["/bin/sh", "-c", self.script], env=self.env, capture_output=True, text=True)
        self.assertEqual(result.returncode, expected_exit, result.stdout + result.stderr)

    def assert_payload(self, app, name, expected):
        self.assertFalse((app / name).is_symlink())
        self.assertEqual((app / name / "payload.txt").read_text(), expected)

    def test_copies_active_bundles_with_spaces_in_paths(self):
        self.run_script()
        for name in [CORE, UI]:
            self.assert_payload(self.app, name, "SPM resources")

    def test_ui_product_alone_includes_transitive_core_resources(self):
        self.write_project(["PurchasesHybridCommonUI"])
        self.run_script()
        for name in [CORE, UI]:
            self.assert_payload(self.app, name, "SPM resources")

    def test_copies_packages_attached_to_app_target(self):
        self.write_project([], main_products=["RevenueCatUI"])
        self.run_script()
        for name in [CORE, UI]:
            self.assert_payload(self.app, name, "SPM resources")

    def test_does_not_copy_unused_or_orphaned_products(self):
        self.write_project(["OtherSDK"], orphaned_products=["PurchasesHybridCommonUI"])
        self.run_script()
        self.assertEqual(list(self.app.iterdir()), [])

    def test_core_only_ignores_stale_ui_build_products(self):
        self.write_project(["PurchasesHybridCommon"])
        self.run_script()
        self.assert_payload(self.app, CORE, "SPM resources")
        self.assertFalse((self.app / UI).exists())

    def test_archive_dereferences_links_and_survives_source_deletion(self):
        self.env["ACTION"] = "install"
        originals = self.root / "Uninstalled Products"
        originals.mkdir()
        for name in [CORE, UI]:
            shutil.move(self.products / name, originals / name)
            (self.products / name).symlink_to(Path("../Uninstalled Products") / name)
        self.run_script()
        shutil.rmtree(originals)
        for app in [self.app, self.installed_app]:
            for name in [CORE, UI]:
                self.assert_payload(app, name, "SPM resources")

    def test_incremental_copy_removes_stale_files_but_preserves_other_resources(self):
        self.run_script()
        (self.app / "unrelated.txt").write_text("Keep me")
        (self.app / UI / "obsolete.txt").write_text("Remove me")
        (self.products / UI / "payload.txt").write_text("Updated resources")
        self.run_script()
        self.assert_payload(self.app, UI, "Updated resources")
        self.assertFalse((self.app / UI / "obsolete.txt").exists())
        self.assertEqual((self.app / "unrelated.txt").read_text(), "Keep me")

    def test_replaces_existing_destination_symlink(self):
        (self.app / UI).symlink_to(self.products / UI)
        self.run_script()
        shutil.rmtree(self.products)
        self.assert_payload(self.app, UI, "SPM resources")

    def check_dependency_switch_with_clean_build(self, archive=False):
        if archive:
            self.env["ACTION"] = "install"
        apps = [self.app, self.installed_app] if archive else [self.app]
        self.run_script()
        self.write_project([])
        # Switching managers requires cleaning the previous build products first.
        shutil.rmtree(self.products)
        self.products.mkdir()
        for app in apps:
            shutil.rmtree(app)
            self.make_bundle(app / UI, "org.cocoapods.RevenueCatUI", "CocoaPods resources")
        self.run_script()
        for app in apps:
            self.assert_payload(app, UI, "CocoaPods resources")
            self.assertFalse((app / CORE).exists())
        # Clean again, then let SPM rebuild its resources after switching back.
        for app in apps:
            shutil.rmtree(app)
            app.mkdir(parents=True)
        self.write_project(["PurchasesHybridCommonUI"])
        for name in [CORE, UI]:
            self.make_bundle(self.products / name, "spm." + name, "New SPM resources")
        self.run_script()
        for app in apps:
            for name in [CORE, UI]:
                self.assert_payload(app, name, "New SPM resources")

    def test_spm_cocoapods_spm_with_clean_build(self):
        self.check_dependency_switch_with_clean_build()

    def test_archive_spm_cocoapods_spm_with_clean_build(self):
        self.check_dependency_switch_with_clean_build(archive=True)

    def test_cocoapods_preserves_existing_resources_despite_stale_spm_products(self):
        self.write_project([])
        self.make_bundle(self.app / UI, "org.cocoapods.RevenueCatUI", "CocoaPods resources")
        self.run_script()
        self.assert_payload(self.app, UI, "CocoaPods resources")
        self.assertFalse((self.app / CORE).exists())

    def test_missing_expected_bundle_fails(self):
        shutil.rmtree(self.products / UI)
        result = subprocess.run(["/bin/sh", "-c", self.script], env=self.env, capture_output=True)
        self.assertNotEqual(result.returncode, 0)

    def test_missing_project_fails_instead_of_treating_it_as_cocoapods(self):
        (self.project / "project.pbxproj").unlink()
        result = subprocess.run(["/bin/sh", "-c", self.script], env=self.env, capture_output=True)
        self.assertNotEqual(result.returncode, 0)

    def check_copy_failure(self, action, failing_copy):
        self.env["ACTION"] = action
        binary_dir = self.root / "bin"
        binary_dir.mkdir()
        rsync = binary_dir / "rsync"
        real_rsync = shlex.quote(shutil.which("rsync"))
        rsync.write_text(
            '#!/bin/sh\n'
            # Some rsync versions launch a receiver even for local copies.
            f'if [ "$1" = "--server" ]; then exec {real_rsync} "$@"; fi\n'
            'count=0\n[ ! -f "$COUNTER" ] || count=$(cat "$COUNTER")\n'
            'count=$((count + 1))\nprintf "%s" "$count" > "$COUNTER"\n'
            '[ "$count" -ne "$FAILING_COPY" ] || exit 23\n'
            f'exec {real_rsync} "$@"\n'
        )
        rsync.chmod(0o755)
        self.env.update(
            PATH=str(binary_dir) + os.pathsep + self.env["PATH"],
            COUNTER=str(self.root / "copy-count"),
            FAILING_COPY=str(failing_copy),
        )
        self.run_script(expected_exit=23)

    def test_build_copy_failure_propagates(self):
        self.check_copy_failure("build", 1)

    def test_archive_first_copy_failure_propagates(self):
        self.check_copy_failure("install", 1)

    def test_archive_install_copy_failure_propagates(self):
        self.check_copy_failure("install", 2)


if __name__ == "__main__":
    if sys.platform != "darwin":
        sys.exit("These tests require macOS (PlistBuddy and rsync).")
    unittest.main(verbosity=2)
