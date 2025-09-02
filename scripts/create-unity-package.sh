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

if [ ! -z "$CI" ]; then
    # In CI (Linux), Unity doesn't traverse symlinks properly, so copy the folder
    echo "🔧 CI environment detected - copying RevenueCat folder instead of symlinking"
    echo "📁 Copying: $PWD/RevenueCat → $PROJECT/Assets/RevenueCat"
    cp -r "$PWD/RevenueCat" "$PROJECT/Assets/"
    
    # Verify copy was successful
    if [ -d "$PROJECT/Assets/RevenueCat" ]; then
        echo "✅ RevenueCat folder copied successfully"
    else
        echo "❌ Failed to copy RevenueCat folder!"
        exit 1
    fi
else
    # Local development (macOS) - use symlink for convenience
    echo "🔗 Creating symlink: $PWD/RevenueCat → $PROJECT/Assets/RevenueCat"
    ln -s "$PWD/RevenueCat" "$PROJECT/Assets/"
    
    # Verify symlink was created successfully
    if [ -L "$PROJECT/Assets/RevenueCat" ]; then
        echo "✅ Symlink created successfully"
    else
        echo "❌ Failed to create symlink!"
        rm -f $SYMBOLIC_LINK_PATH
        exit 1
    fi
fi

# Common verification for both approaches
if [ -d "$PROJECT/Assets/RevenueCat" ]; then
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
    echo "❌ Failed to access RevenueCat folder!"
    rm -rf "$PROJECT/Assets/RevenueCat" 2>/dev/null
    exit 1
fi

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

echo "🔍 Checking assembly definitions in Subtester project:"
find "$PROJECT/Assets" -name "*.asmdef" -type f -exec basename {} \;
echo ""

PLUGINS_FOLDER="$PWD/RevenueCat/Plugins"

if ! [ -d "$PROJECT" ]; then
    echo "Run this script from the root folder of the repository (e.g. ./scripts/create-unity-package.sh)."
    rm -rf "$PROJECT/Assets/RevenueCat" 2>/dev/null
    exit 1
fi

if [ -z "$UNITY_BIN" ]; then
    echo "😞 Unity not passed as parameter!"
    echo "Pass the location of Unity. Something like ./scripts/create-unity-package.sh -u /Applications/Unity/Hub/Editor/6000.2.2f1/Unity.app/Contents/MacOS/Unity"
    echo "Note: This script is optimized for Unity 6.2 (6000.2.x) but should work with Unity 2021.3+ versions"
    rm -rf "$PROJECT/Assets/RevenueCat" 2>/dev/null
    exit 1
fi

# Verify Unity binary exists and is executable
if [ ! -x "$UNITY_BIN" ]; then
    echo "😞 Unity binary not found or not executable at: $UNITY_BIN"
    rm -rf "$PROJECT/Assets/RevenueCat" 2>/dev/null
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
        rm -rf "$PROJECT/Assets/RevenueCat" 2>/dev/null
        exit 1
    fi
    
    if [ ! -f "$EDM_FILE" ]; then
        echo "❌ Failed to download External Dependency Manager"
        rm -rf "$PROJECT/Assets/RevenueCat" 2>/dev/null
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
    -force-free -quit -batchmode -logFile /dev/stdout \
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

if [ $UNITY_EXIT_CODE -eq 0 ] && [ -f "$PACKAGE" ]; then
    echo "✅ Unity package created successfully: $PACKAGE"
else
    echo "❌ Unity package creation failed!"
    echo "   Unity exit code: $UNITY_EXIT_CODE"
    if [ ! -f "$PACKAGE" ]; then
        echo "   Package file not found: $PACKAGE"
    fi
    # Cleanup RevenueCat folder/symlink
    rm -rf "$PROJECT/Assets/RevenueCat"
    exit 1
fi

# Cleanup RevenueCat folder/symlink
rm -rf "$PROJECT/Assets/RevenueCat"
