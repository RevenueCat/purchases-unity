#!/usr/bin/env bash
while getopts u: option
do
case "${option}"
in
u) UNITY_BIN=${OPTARG};;
esac
done

PROJECT="$PWD/Subtester"
PACKAGE="$PWD/Purchases.unitypackage"

# Subtester uses the purchases-unity SDK as a package, but we use it to export a .unitypackage.
# Unity won't export files from package dependencies so we need to add the SDK files inside the Subtester/Assets folder,
# Additionally, we need to remove the UPM dependency from the manifest to export it,
# otherwise it will cause issues due to duplicated symbols.
SYMBOLIC_LINK_PATH="$PROJECT/Assets/RevenueCat"
MANIFEST_JSON_PATH="$PROJECT/Packages/manifest.json"
# Here we are adding a symbolic link in the Subtester/Assets project to the RevenueCat scripts so they are exported
# as part of the .unitypackage
echo "📁 Source RevenueCat folder contents:"
ls -la "$PWD/RevenueCat/"
echo ""

echo "🔗 Creating symlink: $PWD/RevenueCat → $PROJECT/Assets/RevenueCat"
ln -s "$PWD/RevenueCat" "$PROJECT/Assets/"

# Verify symlink was created successfully
if [ -L "$PROJECT/Assets/RevenueCat" ]; then
    echo "✅ Symlink created successfully"
    echo "📁 RevenueCat folder contents:"
    ls -la "$PROJECT/Assets/RevenueCat/"
    echo ""
    echo "📂 Checking for Scripts folder specifically:"
    if [ -d "$PROJECT/Assets/RevenueCat/Scripts" ]; then
        echo "  ✅ Scripts folder exists"
        echo "  📄 Scripts contents:"
        ls -la "$PROJECT/Assets/RevenueCat/Scripts/" | head -10
    else
        echo "  ❌ Scripts folder missing!"
    fi
    echo ""
    echo "📂 Checking assembly definition files:"
    find "$PROJECT/Assets/RevenueCat" -name "*.asmdef" -type f
    echo ""
else
    echo "❌ Failed to create symlink!"
    rm -f $SYMBOLIC_LINK_PATH
    exit 1
fi

# Fix: In CI environments, .asmdef files may not be properly visible through symlinks
# Copy assembly definition files explicitly to ensure Unity can find them
echo "🔧 Ensuring assembly definition files are accessible..."
ASMDEF_FILES_COPIED=0

# Copy .asmdef files from Scripts folder
if [ -f "$PWD/RevenueCat/Scripts/revenuecat.purchases-unity.asmdef" ]; then
    echo "📋 Copying Scripts assembly definition..."
    cp "$PWD/RevenueCat/Scripts/revenuecat.purchases-unity.asmdef" "$PROJECT/Assets/RevenueCat/Scripts/"
    if [ -f "$PWD/RevenueCat/Scripts/revenuecat.purchases-unity.asmdef.meta" ]; then
        cp "$PWD/RevenueCat/Scripts/revenuecat.purchases-unity.asmdef.meta" "$PROJECT/Assets/RevenueCat/Scripts/"
    fi
    ASMDEF_FILES_COPIED=$((ASMDEF_FILES_COPIED + 1))
fi

# Copy .asmdef files from Editor folder
if [ -f "$PWD/RevenueCat/Editor/revenuecat.purchases-unity.Editor.asmdef" ]; then
    echo "📋 Copying Editor assembly definition..."
    cp "$PWD/RevenueCat/Editor/revenuecat.purchases-unity.Editor.asmdef" "$PROJECT/Assets/RevenueCat/Editor/"
    if [ -f "$PWD/RevenueCat/Editor/revenuecat.purchases-unity.Editor.asmdef.meta" ]; then
        cp "$PWD/RevenueCat/Editor/revenuecat.purchases-unity.Editor.asmdef.meta" "$PROJECT/Assets/RevenueCat/Editor/"
    fi
    ASMDEF_FILES_COPIED=$((ASMDEF_FILES_COPIED + 1))
fi

echo "✅ Copied $ASMDEF_FILES_COPIED assembly definition files"
echo "📂 Verification - assembly definition files now visible:"
find "$PROJECT/Assets/RevenueCat" -name "*.asmdef" -type f
echo ""

# This removes the purchases-unity package dependency from the Subtester project
# to export it, otherwise it will fail due to duplicated files with the package dependency.
echo "📄 Original manifest.json:"
cat $MANIFEST_JSON_PATH
echo ""
awk '!/com.revenuecat.purchases-unity/' $MANIFEST_JSON_PATH > temp && mv temp $MANIFEST_JSON_PATH
echo "📄 Modified manifest.json:"
cat $MANIFEST_JSON_PATH
echo ""
# Build export folders list, checking what actually exists
FOLDERS_TO_EXPORT=""
cd $PROJECT
if [ -d "Assets/RevenueCat" ]; then
    REVENUECAT_FOLDERS=$(find Assets/RevenueCat/* -type d -prune 2>/dev/null | tr '\n' ' ')
    FOLDERS_TO_EXPORT="$FOLDERS_TO_EXPORT $REVENUECAT_FOLDERS"
fi
if [ -d "Assets/PlayServicesResolver" ]; then
    FOLDERS_TO_EXPORT="$FOLDERS_TO_EXPORT Assets/PlayServicesResolver"
fi  
if [ -d "Assets/ExternalDependencyManager" ]; then
    FOLDERS_TO_EXPORT="$FOLDERS_TO_EXPORT Assets/ExternalDependencyManager"
fi
cd - > /dev/null

echo "📁 Folders to export: $FOLDERS_TO_EXPORT"

# Check if there are any assembly definition files that might affect compilation order
echo "🔍 Checking assembly definitions in Subtester project:"
find "$PROJECT/Assets" -name "*.asmdef" -type f -exec basename {} \;
echo ""

# Note: We need to keep all Subtester scripts intact as they're part of the testing infrastructure
# Give Unity a moment to recognize the new symlinked assemblies
echo "⏳ Allowing Unity to recognize symlinked assemblies..."
sleep 2

PLUGINS_FOLDER="$PWD/RevenueCat/Plugins"

# Unity 6.2 compatibility: Set build cache cleanup for clean package generation
# This ensures consistent package generation across different Unity versions
export UNITY_BUILD_CACHE_CLEAN=1

if ! [ -d "$PROJECT" ]; then
    echo "Run this script from the root folder of the repository (e.g. ./scripts/create-unity-package.sh)."
    rm $SYMBOLIC_LINK_PATH
    exit 1
fi

if [ -z "$UNITY_BIN" ]; then
    echo "😞 Unity not passed as parameter!"
    echo "Pass the location of Unity. Something like ./scripts/create-unity-package.sh -u /Applications/Unity/Hub/Editor/6000.2.2f1/Unity.app/Contents/MacOS/Unity"
    echo "Note: This script is optimized for Unity 6.2 (6000.2.x) but should work with Unity 2021.3+ versions"
    rm $SYMBOLIC_LINK_PATH
    exit 1
fi

# Verify Unity binary exists and is executable
if [ ! -x "$UNITY_BIN" ]; then
    echo "😞 Unity binary not found or not executable at: $UNITY_BIN"
    rm $SYMBOLIC_LINK_PATH
    exit 1
fi

if [ -d "$PROJECT/Assets/PlayServicesResolver" ]; then
    echo "PlayServicesResolver folder found in assets. It will be deleted and reimported."
    rm -rf $PROJECT/Assets/PlayServicesResolver
fi

if [ -d "$PROJECT/Assets/ExternalDependencyManager" ]; then
    echo "ExternalDependencyManager folder found in assets. It will be deleted and reimported."
    rm -rf $PROJECT/Assets/ExternalDependencyManager
fi

if [ -f $PROJECT/external-dependency-manager-*.unitypackage ]; then
    echo "👌 External dependency manager plugin found. It will be added to the unitypackage."
else
    echo "⬇️ Downloading External Dependency Manager..."
    EDM_URL="https://github.com/googlesamples/unity-jar-resolver/raw/master/external-dependency-manager-latest.unitypackage"
    EDM_FILE="$PROJECT/external-dependency-manager-latest.unitypackage"
    
    # Try wget first (more reliable in CI), fallback to curl
    if command -v wget >/dev/null 2>&1; then
        wget "$EDM_URL" -O "$EDM_FILE"
    elif command -v curl >/dev/null 2>&1; then
        curl -L "$EDM_URL" -o "$EDM_FILE"
    else
        echo "❌ Neither wget nor curl found. Please install one of them."
        rm $SYMBOLIC_LINK_PATH
        exit 1
    fi
    
    # Verify download succeeded
    if [ ! -f "$EDM_FILE" ]; then
        echo "❌ Failed to download External Dependency Manager"
        rm $SYMBOLIC_LINK_PATH
        exit 1
    fi
fi

if [ -f $PACKAGE ]; then
    echo "📦 Old package found. Removing it."
    rm $PACKAGE
fi

echo "📦 Creating Purchases.unitypackage, this may take a minute."

# Unity 6.2+ optimizations:
# -disable-assembly-updater: Speeds up package creation by skipping assembly updates
# -gvh_disable: Required for External Dependency Manager compatibility
if [ ! -z "$CI" ] ; then
    xvfb-run --auto-servernum --server-args='-screen 0 640x480x24' $UNITY_BIN -gvh_disable \
    -nographics \
    -silent-crashes \
    -projectPath $PROJECT \
    -force-free -quit -batchmode -logFile \
    -disable-assembly-updater \
    -importPackage $PROJECT/external-dependency-manager-latest.unitypackage \
    -exportPackage $FOLDERS_TO_EXPORT $PACKAGE
    UNITY_EXIT_CODE=$?
else
    $UNITY_BIN -gvh_disable \
    -nographics \
    -projectPath $PROJECT \
    -force-free -quit -batchmode -logFile exportlog.txt \
    -disable-assembly-updater \
    -importPackage $PROJECT/external-dependency-manager-latest.unitypackage \
    -exportPackage $FOLDERS_TO_EXPORT $PACKAGE
    UNITY_EXIT_CODE=$?
fi

# Unity export completed

# Check if Unity succeeded and package was actually created
if [ $UNITY_EXIT_CODE -eq 0 ] && [ -f "$PACKAGE" ]; then
    echo "✅ Unity package created successfully: $PACKAGE"
else
    echo "❌ Unity package creation failed!"
    echo "   Unity exit code: $UNITY_EXIT_CODE"
    if [ ! -f "$PACKAGE" ]; then
        echo "   Package file not found: $PACKAGE"
    fi
    rm -f $SYMBOLIC_LINK_PATH
    exit 1
fi

rm $SYMBOLIC_LINK_PATH
