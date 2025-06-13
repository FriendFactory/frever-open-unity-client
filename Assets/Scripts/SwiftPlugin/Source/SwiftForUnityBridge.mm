//
//  SwiftForUnityBridge.m
//  SwiftPlugin
//
//  Created by Zhuoyue Wang on 2020-05-18.
//  Copyright © 2020 Zhuoyue Wang. All rights reserved.
//
#import <AVFoundation/AVFoundation.h>
#import <SceneKit/SceneKit.h>
#import <UnityFramework/UnityFramework-Swift.h>

#pragma mark - C interface

extern "C" {
    
     using InitSdkCallback = void (*)(bool);

     void _initializeAmplitudeExperiment(const char *apiKey, const char *instanceName, InitSdkCallback callback)
     {
         [[AmplitudeExperiment shared] InitializeWithKey:[NSString stringWithUTF8String:apiKey] instanceName:[NSString stringWithUTF8String:instanceName] callback:^(BOOL isSuccess) {
             callback(isSuccess);
         }];
     }

     char* _getVariantValue(const char *key) {
         NSString *returnString = [[AmplitudeExperiment shared] GetVariantValueWithKey:[NSString stringWithUTF8String:key]];
         char* cStringCopy(const char* string);
         return cStringCopy([returnString UTF8String]);
     }

    char* _getPayloadValue(const char *key) {
        NSString *returnString = [[AmplitudeExperiment shared] GetPayloadValueWithKey:[NSString stringWithUTF8String:key]];
        char* cStringCopy(const char* string);
        return cStringCopy([returnString UTF8String]);
    }
    
    char* _getVariantsList () {
        NSString *returnString = [[AmplitudeExperiment shared] GetVariantsList];
        char* cStringCopy(const char* string);
        return cStringCopy([returnString UTF8String]);
    }

     char* _sayHelloToUnity() {
          NSString *returnString = [[SwiftForUnity shared]       SayHelloToUnity];
          char* cStringCopy(const char* string);
          return cStringCopy([returnString UTF8String]);
     }

      void _viewDidLoad() {
           [[SwiftForUnity shared]       viewDidLoad];
      }

      bool _getFaceVertices( float* outValues, int outputLength, float* outCenterValues, int outputCenterLength )
      {
        // get facemesh vertices
        NSArray *results = [[SwiftForUnity shared] getFaceVertices];
        int count = (int)[results count];
        int copy = count > outputLength ? outputLength : count;
        for (int i = 0; i < copy; ++i)
        {
            outValues[i] = [[results objectAtIndex:i] floatValue];
        }

        bool lastFrameCapturedVerts = [[SwiftForUnity shared] getWhetherLastFrameCapturedVerts];

        // get the getCenterTransform
        NSArray *resultsCenter = [[SwiftForUnity shared] getCenterTransform];
        int countCenter = (int)[results count];
        copy = countCenter > outputCenterLength ? outputCenterLength : countCenter;
        for (int i = 0; i < copy; ++i)
        {
            outCenterValues[i] = [[resultsCenter objectAtIndex:i] floatValue];
        }
        return lastFrameCapturedVerts;
      }

      uint _getLastRotation()
      {
        return [[SwiftForUnity shared] getLastRotation];
      }
}

char* cStringCopy(const char* string){
     if (string == NULL){
          return NULL;
     }
     char* res = (char*)malloc(strlen(string)+1);
     strcpy(res, string);
     return res;
}
