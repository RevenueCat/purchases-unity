#!/bin/bash
IOS_SDK_VERSION=3.0.0

UNITY_DIR=`pwd`
IOS_UNITY_DIR=$UNITY_DIR/RevenueCat/Plugins/iOS/

TEMP_DIR=`mktemp -d -t "purchases-unity"`
pushd $TEMP_DIR

# Download the iOS SDK
mkdir -p ios
pushd ios

IOS_SOURCE_URL=https://github.com/RevenueCat/purchases-ios/archive/$IOS_SDK_VERSION.zip

wget -qO- $IOS_SOURCE_URL | bsdtar -xvf-
pushd purchases-ios-$IOS_SDK_VERSION

find Purchases -iname '*.h' -exec cp \{\} /$IOS_UNITY_DIR/ \;
find Purchases -iname '*.m' -exec cp \{\} /$IOS_UNITY_DIR/ \;
rm ${IOS_UNITY_DIR}Purchases_macOS.h

popd