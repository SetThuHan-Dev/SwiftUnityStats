#import <Foundation/Foundation.h>
#include "UnityFramework/UnityFramework-Swift.h"

#pragma mark - Functions

void _startTracking()
{
    [[UnityPlugin shared] startTracking];
}

void _stopTracking()
{
    [[UnityPlugin shared] stopTracking];
}

