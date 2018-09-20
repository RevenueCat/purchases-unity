#!/bin/bash

IOS_SDK_VERSION=1.1.4
ANDROID_SDK_VERSION=1.3.5

UNITY_DIR=`pwd`
IOS_UNITY_DIR=$UNITY_DIR/Plugins/iOS/

TEMP_DIR=`mktemp -d -t "purchases-unity"`
pushd $TEMP_DIR

# Download the iOS SDK
mkdir -p ios
pushd ios

IOS_SOURCE_URL=https://github.com/RevenueCat/purchases-ios/archive/$IOS_SDK_VERSION.zip

wget -qO- $IOS_SOURCE_URL | bsdtar -xvf-
pushd purchases-ios-$IOS_SDK_VERSION

find Purchases/Classes -iname '*.h' -exec cp \{\} /$IOS_UNITY_DIR/ \;
find Purchases/Classes -iname '*.m' -exec cp \{\} /$IOS_UNITY_DIR/ \;
rm $IOS_UNITY_DIR/Purchase_macOS.h

ls
#popd


# https://github.com/RevenueCat/purchases-ios/archive/1.1.4.zip
# https://repo1.maven.org/maven2/com/revenuecat/purchases/purchases/1.3.5/purchases-1.3.5.aar

#popd