#!/usr/bin/env bash
PROJECT="$PWD/Subtester"
PACKAGE="$PWD/Purchases.unitypackage"
PACKAGE_OBSERVER_MODE="$PWD/Purchases_NoInAppBillingService.unitypackage"
FOLDERS_TO_EXPORT=$(cd $PROJECT; find Assets/RevenueCat/* Assets/PlayServicesResolver Assets/ExternalDependencyManager -type d -prune ! -name ObserverMode.xml)
PLUGINS_FOLDER="$PWD/RevenueCat/Plugins"

if ! [ -d "$PROJECT" ]; then
    echo "Run this script from the root folder of the repository (e.g. ./scripts/create-unity-package.sh)."
    exit 1
fi

# while true; do
#     read -p "Have you deleted the External Dependency Manager from the Subtester Packages?" yn
#     case $yn in
#         [Yy]* ) break;;
#         [Nn]* ) exit;;
#         * ) echo "Please answer yes or no.";;
#     esac
# done

if [ -z "$UNITY_BIN" ]; then
    echo "😞 UNITY_BIN environment variable is not defined!"
    echo "Set it to something like /Applications/Unity/Hub/Editor/2019.3.10f1/Unity.app/Contents/MacOS/Unity"
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

if [ -f $PROJECT/external-dependency-manager-*.unitypackage ]; 
then
    echo "👌 External dependency manager plugin found. It will be added to the unitypackage."
else
    echo "⚠️  External dependency manager plugin not found. Please download the latest version and locate it inside the Subtester folder."
    exit 1
fi

if [ -f $PACKAGE ]; 
then
    echo "📦 Old package found. Removing it."
    rm $PACKAGE
fi

echo "📦 Creating Purchases.unitypackage, this may take a minute."

$UNITY_BIN -gvh_disable -projectPath $PROJECT -force-free -quit -batchmode -logFile exportlog.txt \
    -importPackage $PROJECT/external-dependency-manager-*.unitypackage \
    -exportPackage $FOLDERS_TO_EXPORT $PACKAGE >& /dev/null

if [[ $? -ne 0 ]]; then
  echo "😱 Exporting has failed! Checkout exportlogs.txt"
  echo "😱 Unity shouldn't be running when executing the script."
  exit $return_code
else
  echo "🚀 Exporting finished for the regular package! Path: $PACKAGE" 
fi

echo "📦 Moving com.android.billingclient.billing-no-service.aar to Revenuecat/Plugins/Android"
cp $PWD/ObserverMode/com.android.billingclient.billing-no-service.aar $PLUGINS_FOLDER/Android

echo "📦 Modifying RevenueCatDependencies.xml to use observer mode dependencies"
mkdir $PWD/Temp
mv $PLUGINS_FOLDER/Editor/RevenueCatDependencies.xml $PWD/Temp/RevenueCatDependencies.xml.bck
cp $PWD/ObserverMode/RevenueCatDependencies.xml $PLUGINS_FOLDER/Editor/RevenueCatDependencies.xml

echo "📦 Creating Purchases_NoInAppBillingService.unitypackage, this may take a minute."
$UNITY_BIN -gvh_disable -projectPath $PROJECT -force-free -quit -batchmode -logFile exportlog.txt \
    -importPackage $PROJECT/external-dependency-manager-*.unitypackage \
    -exportPackage $FOLDERS_TO_EXPORT $PACKAGE_OBSERVER_MODE >& /dev/null

if [[ $? -ne 0 ]]; then
  echo "😱 Exporting has failed! Checkout exportlogs.txt"
  echo "😱 Unity shouldn't be running when executing the script."
  exit $return_code
else
  echo "🚀 Exporting finished for the observer mode package! Path: $PACKAGE_OBSERVER_MODE" 
fi

echo "📦 Cleaning up..."
rm $PLUGINS_FOLDER/Android/com.android.billingclient.billing-no-service.aar
rm $PLUGINS_FOLDER/Editor/RevenueCatDependencies.xml
mv $PWD/Temp/RevenueCatDependencies.xml.bck $PLUGINS_FOLDER/Editor/RevenueCatDependencies.xml
rm -rf $PWD/Temp

open .