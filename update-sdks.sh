#!/bin/bash
IOS_SDK_VERSION=2.1.1
ANDROID_SDK_VERSION=2.1.0

UNITY_DIR=`pwd`
IOS_UNITY_DIR=$UNITY_DIR/Plugins/RevenueCat/Plugins/iOS/

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

mkdir -p android
pushd android

ANDROID_FILENAME=purchases-$ANDROID_SDK_VERSION.aar
ANDROID_SOURCE_URL=https://repo1.maven.org/maven2/com/revenuecat/purchases/purchases/$ANDROID_SDK_VERSION/$ANDROID_FILENAME
wget $ANDROID_SOURCE_URL
cp $ANDROID_FILENAME $UNITY_DIR/Plugins/RevenueCat/Plugins/Android


#popd
