//
//  Bridge.hpp
//  Purchases
//
//  Created by César de la Vega  on 1/7/20.
//  Copyright © 2020 RevenueCat. All rights reserved.
//

typedef void (* UnityCommandCallback)(const char *objectName, const char *commandName, const char *commandData);

extern "C" {

    void ConnectCallback(UnityCommandCallback callback);
    
    void CallMethod(const char *objectName, const char *commandName, const char *commandData);
    
    void _RCSetupPurchases(const char *gameObject, const char *apiKey, const char *appUserId, const bool observerMode);
    
    void _RCGetOfferings();
}
