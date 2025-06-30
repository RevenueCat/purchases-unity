# RevenueCat UI Unity SDK Changelog

## 1.0.0 (TBD)

### ✨ New Features

- **Paywalls**: Present native RevenueCat paywalls with `RevenueCatUI.PresentPaywall()`
- **Customer Center**: Present customer subscription management with `RevenueCatUI.PresentCustomerCenter()`
- **Conditional Paywalls**: Present paywalls only if user lacks entitlement with `RevenueCatUI.PresentPaywallIfNeeded()`
- **Platform Support**: iOS 15.0+ and Android API 21+ support
- **Unity Package Manager**: Full UPM support as part of purchases-unity monorepo

### 🏗️ Architecture

- **Monorepo Structure**: Integrated into purchases-unity repository as `RevenueCatUI/`
- **Hybrid Common Wrapper**: Uses purchases-hybrid-common-ui for platform implementations
- **Unity Conventions**: PascalCase APIs, async/await patterns, proper error handling
- **Platform Abstraction**: Clean separation between Unity C# API and native implementations

### 📱 Platform Features

#### iOS
- Uses `PaywallProxy` and `CustomerCenterProxy` from purchases-hybrid-common-ui
- Objective-C++ bridge for Unity integration
- iOS 15.0+ requirement for native UI components

#### Android
- Uses `presentPaywallFromFragment` and `ShowCustomerCenter` from purchases-hybrid-common-ui  
- Java bridge with proper Unity integration
- FragmentActivity requirement for paywall presentation
- Proper build.gradle configuration with Java 8 compatibility 