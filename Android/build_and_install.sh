#!/bin/bash
./gradlew assembleRelease sourcesJar
mv purchasesunity/build/outputs/aar/purchasesunity-release.aar ../Revenuecat/Plugins/Android/purchasesunity.aar
mv purchasesunity/build/libs/purchasesunity-sources.jar ../Revenuecat/Plugins/Android/purchasesunity-sources.jar