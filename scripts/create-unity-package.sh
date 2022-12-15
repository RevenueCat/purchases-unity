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
PACKAGE_UNITY_IAP="$PWD/Purchases-UnityIAP.unitypackage"

# Subtester uses the purchases-unity SDK as a package, but we use it to export a .unitypackage.
# Unity won't export files from package dependencies so we need to add the SDK files inside the Subtester/Assets folder,
# Additionally, we need to remove the UPM dependency from the manifest to export it,
# otherwise it will cause issues due to duplicated symbols.
SYMBOLIC_LINK_PATH="$PROJECT/Assets/RevenueCat"
MANIFEST_JSON_PATH="$PROJECT/Packages/manifest.json"
# Here we are adding a symbolic link in the Subtester/Assets project to the RevenueCat scripts so they are exported
# as part of the .unitypackage
ln -s "$PWD/RevenueCat" "$PROJECT/Assets/"
# This removes the purchases-unity package dependency from the Subtester project to export it, otherwise it
# will fail due to duplicated files with the package dependency.
awk '!/com.revenuecat.purchases-unity/' $MANIFEST_JSON_PATH > temp && mv temp $MANIFEST_JSON_PATH

FOLDERS_TO_EXPORT=$(cd $PROJECT; find Assets/RevenueCat/* Assets/PlayServicesResolver Assets/ExternalDependencyManager -type d -prune)
PLUGINS_FOLDER="$PWD/RevenueCat/Plugins"

if ! [ -d "$PROJECT" ]; then
    echo "Run this script from the root folder of the repository (e.g. ./scripts/create-unity-package.sh)."
    rm $SYMBOLIC_LINK_PATH
    exit 1
fi

if [ -z "$UNITY_BIN" ]; then
    echo "😞 Unity not passed as parameter!"
    echo "Pass the location of Unity. Something like ./scripts/create-unity-package.sh -u /Applications/Unity/Hub/Editor/2019.3.10f1/Unity.app/Contents/MacOS/Unity"
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

if [ ! -z "$CI" ] ; then
    xvfb-run --auto-servernum --server-args='-screen 0 640x480x24' $UNITY_BIN -gvh_disable \
    -nographics \
    -silent-crashes \
    -projectPath $PROJECT \
    -force-free -quit -batchmode -logFile \
    -importPackage $PROJECT/external-dependency-manager-latest.unitypackage \
    -exportPackage $FOLDERS_TO_EXPORT $PACKAGE
else
    $UNITY_BIN -gvh_disable \
    -nographics \
    -projectPath $PROJECT \
    -force-free -quit -batchmode -logFile exportlog.txt \
    -importPackage $PROJECT/external-dependency-manager-latest.unitypackage \
    -exportPackage $FOLDERS_TO_EXPORT $PACKAGE
fi

echo "Unity package created. Updating dependency for Unity IAP compatibility"

export REGULAR_PACKAGE_NAME="com.revenuecat.purchases:purchases-hybrid-common:"
export UNITY_IAP_PACKAGE_NAME="com.revenuecat.purchases:purchases-hybrid-common-unityiap:"

sed -i -e "s/spec=\"$REGULAR_PACKAGE_NAME/spec=\"$UNITY_IAP_PACKAGE_NAME/" \
./RevenueCat/Plugins/Editor/RevenueCatDependencies.xml

echo "📦 Creating Purchases-UnityIAP.unitypackage, this may take a minute."

if [ ! -z "$CI" ] ; then
    xvfb-run --auto-servernum --server-args='-screen 0 640x480x24' $UNITY_BIN -gvh_disable \
    -nographics \
    -silent-crashes \
    -projectPath $PROJECT \
    -force-free -quit -batchmode -logFile \
    -importPackage $PROJECT/external-dependency-manager-latest.unitypackage \
    -exportPackage $FOLDERS_TO_EXPORT $PACKAGE_UNITY_IAP
else
    $UNITY_BIN -gvh_disable \
    -nographics \
    -projectPath $PROJECT \
    -force-free -quit -batchmode -logFile exportlog.txt \
    -importPackage $PROJECT/external-dependency-manager-latest.unitypackage \
    -exportPackage $FOLDERS_TO_EXPORT $PACKAGE_UNITY_IAP
fi

rm $SYMBOLIC_LINK_PATH
