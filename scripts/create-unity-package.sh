#!/usr/bin/env bash

# Default values
VERBOSE=false

while getopts u:v option
do
case "${option}"
in
u) UNITY_BIN=${OPTARG};;
v) VERBOSE=true;;
esac
done

# Function to print verbose messages
verbose_echo() {
    if [ "$VERBOSE" = true ]; then
        echo "[VERBOSE] $1"
    fi
}

PROJECT="$PWD/Subtester"
PACKAGE="$PWD/Purchases.unitypackage"

verbose_echo "Project path: $PROJECT"
verbose_echo "Package output path: $PACKAGE"

# Subtester uses the purchases-unity SDK as a package, but we use it to export a .unitypackage.
# Unity won't export files from package dependencies so we need to add the SDK files inside the Subtester/Assets folder,
# Additionally, we need to remove the UPM dependency from the manifest to export it,
# otherwise it will cause issues due to duplicated symbols.
SYMBOLIC_LINK_PATH="$PROJECT/Assets/RevenueCat"
MANIFEST_JSON_PATH="$PROJECT/Packages/manifest.json"

verbose_echo "Symbolic link path: $SYMBOLIC_LINK_PATH"
verbose_echo "Manifest JSON path: $MANIFEST_JSON_PATH"
# Here we are adding a symbolic link in the Subtester/Assets project to the RevenueCat scripts so they are exported
# as part of the .unitypackage
verbose_echo "Listing source RevenueCat folder contents:"
if [ "$VERBOSE" = true ]; then
    ls -la "$PWD/RevenueCat/"
fi

if [ ! -z "$CI" ]; then
    # In CI (Linux), Unity doesn't traverse symlinks properly, so copy the folder
    echo "🔧 CI environment detected - copying RevenueCat folder instead of symlinking"
    verbose_echo "Copying: $PWD/RevenueCat → $PROJECT/Assets/RevenueCat"
    verbose_echo "CI detected: $CI"
    verbose_echo "Using copy operation instead of symlink for CI compatibility"
    cp -r "$PWD/RevenueCat" "$PROJECT/Assets/"
    
    # Also copy RevenueCatUI folder for compilation (not exported yet)
    echo "🔧 Copying RevenueCatUI folder for compilation"
    verbose_echo "Copying: $PWD/RevenueCatUI → $PROJECT/Assets/RevenueCatUI"
    cp -r "$PWD/RevenueCatUI" "$PROJECT/Assets/"
    
    # Verify copy was successful
    if [ -d "$PROJECT/Assets/RevenueCat" ]; then
        echo "✅ RevenueCat folder copied successfully"
    else
        echo "❌ Failed to copy RevenueCat folder!"
        exit 1
    fi
    
    if [ -d "$PROJECT/Assets/RevenueCatUI" ]; then
        echo "✅ RevenueCatUI folder copied successfully"
    else
        echo "❌ Failed to copy RevenueCatUI folder!"
        exit 1
    fi
else
    # Local development (macOS) - use symlink for convenience
    verbose_echo "Creating symlink: $PWD/RevenueCat → $PROJECT/Assets/RevenueCat"
    verbose_echo "Local development environment detected"
    verbose_echo "Using symlink operation for local development"
    ln -s "$PWD/RevenueCat" "$PROJECT/Assets/"
    
    # Also create symlink for RevenueCatUI folder for compilation (not exported yet)
    verbose_echo "Creating symlink: $PWD/RevenueCatUI → $PROJECT/Assets/RevenueCatUI"
    ln -s "$PWD/RevenueCatUI" "$PROJECT/Assets/"
    
    # Verify symlink was created successfully
    if [ -L "$PROJECT/Assets/RevenueCat" ]; then
        echo "✅ RevenueCat symlink created successfully"
    else
        echo "❌ Failed to create RevenueCat symlink!"
        rm -f $SYMBOLIC_LINK_PATH
        exit 1
    fi
    
    if [ -L "$PROJECT/Assets/RevenueCatUI" ]; then
        echo "✅ RevenueCatUI symlink created successfully"
    else
        echo "❌ Failed to create RevenueCatUI symlink!"
        rm -f "$PROJECT/Assets/RevenueCatUI"
        exit 1
    fi
fi

# Common verification for both approaches
if [ -d "$PROJECT/Assets/RevenueCat" ]; then
    verbose_echo "Listing RevenueCat folder contents in project:"
    if [ "$VERBOSE" = true ]; then
        ls -la "$PROJECT/Assets/RevenueCat/"
        echo ""
    fi
    verbose_echo "Checking for Scripts folder specifically:"
    if [ -d "$PROJECT/Assets/RevenueCat/Scripts" ]; then
        echo "  ✅ Scripts folder exists"
        verbose_echo "Scripts folder contents:"
        if [ "$VERBOSE" = true ]; then
            ls -la "$PROJECT/Assets/RevenueCat/Scripts/" | head -10
        fi
    else
        echo "  ❌ Scripts folder missing!"
    fi
    verbose_echo "Checking assembly definition files:"
    if [ "$VERBOSE" = true ]; then
        find "$PROJECT/Assets/RevenueCat" -name "*.asmdef" -type f
    fi
else
    echo "❌ Failed to access RevenueCat folder!"
    rm -rf "$PROJECT/Assets/RevenueCat" "$PROJECT/Assets/RevenueCatUI" 2>/dev/null
    exit 1
fi

# This removes the purchases-unity package dependency from the Subtester project
# to export it, otherwise it will fail due to duplicated files with the package dependency.
verbose_echo "Original manifest.json contents:"
if [ "$VERBOSE" = true ]; then
    cat $MANIFEST_JSON_PATH
fi
verbose_echo "Removing com.revenuecat.purchases-unity and com.revenuecat.purchases-ui-unity dependencies from manifest.json"
awk '!/com.revenuecat.purchases-unity/ && !/com.revenuecat.purchases-ui-unity/' $MANIFEST_JSON_PATH > temp && mv temp $MANIFEST_JSON_PATH
verbose_echo "Modified manifest.json contents:"
if [ "$VERBOSE" = true ]; then
    cat $MANIFEST_JSON_PATH
fi
# Build export folders list, checking what actually exists
FOLDERS_TO_EXPORT=""
verbose_echo "Building export folders list..."
cd $PROJECT
if [ -d "Assets/RevenueCat" ]; then
    REVENUECAT_FOLDERS=$(find Assets/RevenueCat/* -type d -prune 2>/dev/null | tr '\n' ' ')
    FOLDERS_TO_EXPORT="$FOLDERS_TO_EXPORT $REVENUECAT_FOLDERS"
    verbose_echo "Found RevenueCat folders: $REVENUECAT_FOLDERS"
fi
if [ -d "Assets/PlayServicesResolver" ]; then
    FOLDERS_TO_EXPORT="$FOLDERS_TO_EXPORT Assets/PlayServicesResolver"
    verbose_echo "Found PlayServicesResolver folder"
fi  
if [ -d "Assets/ExternalDependencyManager" ]; then
    FOLDERS_TO_EXPORT="$FOLDERS_TO_EXPORT Assets/ExternalDependencyManager"
    verbose_echo "Found ExternalDependencyManager folder"
fi
cd - > /dev/null

echo "📁 Folders to export: $FOLDERS_TO_EXPORT"

verbose_echo "Checking assembly definitions in Subtester project:"
if [ "$VERBOSE" = true ]; then
    find "$PROJECT/Assets" -name "*.asmdef" -type f -exec basename {} \;
fi

PLUGINS_FOLDER="$PWD/RevenueCat/Plugins"

if ! [ -d "$PROJECT" ]; then
    echo "Run this script from the root folder of the repository (e.g. ./scripts/create-unity-package.sh)."
    rm -rf "$PROJECT/Assets/RevenueCat" "$PROJECT/Assets/RevenueCatUI" 2>/dev/null
    exit 1
fi

if [ -z "$UNITY_BIN" ]; then
    echo "😞 Unity not passed as parameter!"
    echo "Pass the location of Unity. Something like ./scripts/create-unity-package.sh -u /Applications/Unity/Hub/Editor/6000.2.2f1/Unity.app/Contents/MacOS/Unity"
    echo "Usage: ./scripts/create-unity-package.sh -u <unity_path> [-v]"
    echo "  -u <unity_path>  Path to Unity binary"
    echo "  -v              Enable verbose output"
    echo "Note: This script is optimized for Unity 6.2 (6000.2.x) but should work with Unity 2021.3+ versions"
    rm -rf "$PROJECT/Assets/RevenueCat" "$PROJECT/Assets/RevenueCatUI" 2>/dev/null
    exit 1
fi

# Verify Unity binary exists and is executable
verbose_echo "Checking Unity binary at: $UNITY_BIN"
if [ ! -x "$UNITY_BIN" ]; then
    echo "😞 Unity binary not found or not executable at: $UNITY_BIN"
    rm -rf "$PROJECT/Assets/RevenueCat" "$PROJECT/Assets/RevenueCatUI" 2>/dev/null
    exit 1
fi
verbose_echo "Unity binary verified successfully"

if [ -d "$PROJECT/Assets/PlayServicesResolver" ]; then
    verbose_echo "PlayServicesResolver folder found in assets. It will be deleted and reimported."
    verbose_echo "Removing existing PlayServicesResolver folder"
    rm -rf $PROJECT/Assets/PlayServicesResolver
fi

if [ -d "$PROJECT/Assets/ExternalDependencyManager" ]; then
    verbose_echo "ExternalDependencyManager folder found in assets. It will be deleted and reimported."
    verbose_echo "Removing existing ExternalDependencyManager folder"
    rm -rf $PROJECT/Assets/ExternalDependencyManager
fi

if [ -f $PROJECT/external-dependency-manager-*.unitypackage ]; then
    verbose_echo "External dependency manager plugin found. It will be added to the unitypackage."
    verbose_echo "Using existing External Dependency Manager package"
else
    echo "⬇️ Downloading External Dependency Manager..."
    EDM_URL="https://github.com/googlesamples/unity-jar-resolver/raw/master/external-dependency-manager-latest.unitypackage"
    EDM_FILE="$PROJECT/external-dependency-manager-latest.unitypackage"
    
    verbose_echo "Download URL: $EDM_URL"
    verbose_echo "Download destination: $EDM_FILE"
    
    # Try wget first (more reliable in CI), fallback to curl
    if command -v wget >/dev/null 2>&1; then
        verbose_echo "Using wget to download External Dependency Manager"
        wget "$EDM_URL" -O "$EDM_FILE"
    elif command -v curl >/dev/null 2>&1; then
        verbose_echo "Using curl to download External Dependency Manager"
        curl -L "$EDM_URL" -o "$EDM_FILE"
    else
        echo "❌ Neither wget nor curl found. Please install one of them."
        rm -rf "$PROJECT/Assets/RevenueCat" "$PROJECT/Assets/RevenueCatUI" 2>/dev/null
        exit 1
    fi
    
    if [ ! -f "$EDM_FILE" ]; then
        echo "❌ Failed to download External Dependency Manager"
        rm -rf "$PROJECT/Assets/RevenueCat" "$PROJECT/Assets/RevenueCatUI" 2>/dev/null
        exit 1
    fi
fi

if [ -f $PACKAGE ]; then
    verbose_echo "Old package found. Removing it."
    rm $PACKAGE
fi

echo "📦 Creating Purchases.unitypackage, this may take a minute."

# Unity 6.2+ optimizations:
# -disable-assembly-updater: Speeds up package creation by skipping assembly updates
# -gvh_disable: Required for External Dependency Manager compatibility
verbose_echo "Starting Unity package creation process..."
if [ ! -z "$CI" ] ; then
    verbose_echo "Running Unity in CI mode with xvfb-run"
    verbose_echo "Unity command: xvfb-run --auto-servernum --server-args='-screen 0 640x480x24' $UNITY_BIN -gvh_disable -nographics -silent-crashes -projectPath $PROJECT -force-free -quit -batchmode -logFile /dev/stdout -disable-assembly-updater -importPackage $PROJECT/external-dependency-manager-latest.unitypackage -exportPackage $FOLDERS_TO_EXPORT $PACKAGE"
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
    verbose_echo "Running Unity in local mode"
    verbose_echo "Unity command: $UNITY_BIN -gvh_disable -nographics -projectPath $PROJECT -force-free -quit -batchmode -logFile exportlog.txt -disable-assembly-updater -importPackage $PROJECT/external-dependency-manager-latest.unitypackage -exportPackage $FOLDERS_TO_EXPORT $PACKAGE"
    $UNITY_BIN -gvh_disable \
    -nographics \
    -projectPath $PROJECT \
    -force-free -quit -batchmode -logFile exportlog.txt \
    -disable-assembly-updater \
    -importPackage $PROJECT/external-dependency-manager-latest.unitypackage \
    -exportPackage $FOLDERS_TO_EXPORT $PACKAGE
    UNITY_EXIT_CODE=$?
fi
verbose_echo "Unity process completed with exit code: $UNITY_EXIT_CODE"

if [ $UNITY_EXIT_CODE -eq 0 ] && [ -f "$PACKAGE" ]; then
    echo "✅ Unity package created successfully: $PACKAGE"
    verbose_echo "Package file size: $(du -h "$PACKAGE" | cut -f1)"
else
    echo "❌ Unity package creation failed!"
    echo "   Unity exit code: $UNITY_EXIT_CODE"
    if [ ! -f "$PACKAGE" ]; then
        echo "   Package file not found: $PACKAGE"
    fi
    verbose_echo "Cleaning up RevenueCat and RevenueCatUI folders/symlinks due to failure"
    # Cleanup RevenueCat and RevenueCatUI folders/symlinks
    rm -rf "$PROJECT/Assets/RevenueCat"
    rm -rf "$PROJECT/Assets/RevenueCatUI"
    exit 1
fi

# Cleanup RevenueCat and RevenueCatUI folders/symlinks
verbose_echo "Cleaning up RevenueCat and RevenueCatUI folders/symlinks after successful package creation"
rm -rf "$PROJECT/Assets/RevenueCat"
rm -rf "$PROJECT/Assets/RevenueCatUI"
