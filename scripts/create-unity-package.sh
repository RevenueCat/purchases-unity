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
FOLDERS_TO_EXPORT=$(cd $PROJECT; find Assets/RevenueCat/* Assets/PlayServicesResolver Assets/ExternalDependencyManager -type d -prune)
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
    echo "😞 Unity not passed as parameter!"
    echo "Pass the location of Unity. Something like ./scripts/create-unity-package.sh /Applications/Unity/Hub/Editor/2019.3.10f1/Unity.app/Contents/MacOS/Unity"
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

# if [[ $? -ne 0 ]]; then
#   echo "😱 Exporting has failed! Checkout exportlog.txt"
#   echo "😱 Unity shouldn't be running when executing the script."
#   exit $return_code
# else
#   echo "🚀 Exporting finished! Path: $PACKAGE" 
# fi

# open .