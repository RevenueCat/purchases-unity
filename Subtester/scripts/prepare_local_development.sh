#!/bin/bash

PROJECT_DIRECTORY=$1
LOCAL_PURCHASES_HYBRID_COMMON_DIRECTORY=$2
SETTINGS_GRADLE_FILE=$PROJECT_DIRECTORY/settings.gradle

# This is so the next line is guaranteed to be in a new line. Otherwise, it would append to the contents of the latest line instead, 
# and there's no empty line at the end of the file, so it just appends it right next to the text of the latest line
echo "" >> $PROJECT_DIRECTORY/settings.gradle
echo "" >> $PROJECT_DIRECTORY/gradle.properties

echo "includeBuild('$LOCAL_PURCHASES_HYBRID_COMMON_DIRECTORY')" >> $PROJECT_DIRECTORY/settings.gradle

android_tools_version=$(cat $LOCAL_PURCHASES_HYBRID_COMMON_DIRECTORY/build.gradle | grep ext.android_tools | awk '{print($3)}' | sed "s/'//g")
sed -i .bck s/3.4.0/$android_tools_version/ $PROJECT_DIRECTORY/build.gradle

echo "android.useAndroidX=true" >> $PROJECT_DIRECTORY/gradle.properties
echo "android.enableJetifier=true" >> $PROJECT_DIRECTORY/gradle.properties
