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
ln -s "$PWD/RevenueCat" "$PROJECT/Assets/"
# This removes the purchases-unity package dependency from the Subtester project
# to export it, otherwise it will fail due to duplicated files with the package dependency.
awk '!/com.revenuecat.purchases-unity/' $MANIFEST_JSON_PATH > temp && mv temp $MANIFEST_JSON_PATH
FOLDERS_TO_EXPORT=$(cd $PROJECT; find Assets/RevenueCat/* Assets/PlayServicesResolver Assets/ExternalDependencyManager -type d -prune)
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
    echo "Pass the location of Unity. Something like ./scripts/create-unity-package.sh -u /Applications/Unity/Hub/Editor/6000.2.0f1/Unity.app/Contents/MacOS/Unity"
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
    wget https://github.com/googlesamples/unity-jar-resolver/raw/master/external-dependency-manager-latest.unitypackage -P $PROJECT
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
else
    $UNITY_BIN -gvh_disable \
    -nographics \
    -projectPath $PROJECT \
    -force-free -quit -batchmode -logFile exportlog.txt \
    -disable-assembly-updater \
    -importPackage $PROJECT/external-dependency-manager-latest.unitypackage \
    -exportPackage $FOLDERS_TO_EXPORT $PACKAGE
fi

echo "Unity package created"

rm $SYMBOLIC_LINK_PATH
