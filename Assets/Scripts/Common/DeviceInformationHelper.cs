using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
#if UNITY_IOS
using UnityEngine.iOS;
#endif

namespace Common
{
    [UsedImplicitly]
    public sealed class DeviceInformationHelper
    {
        #if UNITY_IOS

            private static readonly DeviceGeneration[] _devicesWithoutTrueDepth =
            {
                DeviceGeneration.iPhone6S,
                DeviceGeneration.iPhone6SPlus,
                DeviceGeneration.iPhone7,
                DeviceGeneration.iPhone7Plus,
                DeviceGeneration.iPhone8,
                DeviceGeneration.iPhone8Plus,
                DeviceGeneration.iPad1Gen,
                DeviceGeneration.iPad2Gen,
                DeviceGeneration.iPad3Gen,
                DeviceGeneration.iPad4Gen,
                DeviceGeneration.iPad5Gen,
                DeviceGeneration.iPad6Gen,
                DeviceGeneration.iPad7Gen,
                DeviceGeneration.iPad8Gen,
                DeviceGeneration.iPadAir1,
                DeviceGeneration.iPadAir2,
                DeviceGeneration.iPadAir4Gen,
                DeviceGeneration.iPadMini1Gen,
                DeviceGeneration.iPadMini2Gen,
                DeviceGeneration.iPadMini3Gen,
                DeviceGeneration.iPadMini4Gen,
                DeviceGeneration.iPadPro1Gen,
                DeviceGeneration.iPadPro2Gen,
                DeviceGeneration.iPadPro3Gen,
                DeviceGeneration.iPadPro4Gen,
                DeviceGeneration.iPadPro10Inch1Gen,
                DeviceGeneration.iPadPro10Inch2Gen
            };

            private static readonly DeviceGeneration[] _unknownDevices =
            {
                DeviceGeneration.Unknown,
                DeviceGeneration.iPadUnknown,
                DeviceGeneration.iPhoneUnknown,
                DeviceGeneration.iPodTouchUnknown,
            };

        #endif

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public static bool DeviceSupportsTrueDepth()
        {
            // For Android devices IsDeviceWithoutTrueDepth always returns true.
            #if UNITY_IOS
            return !_devicesWithoutTrueDepth.Contains(Device.generation) || Application.isEditor;
            #else
	            return false;
            #endif
        }

        public static bool DeviceUsesARFoundations()
        {
            #if UNITY_IOS || UNITY_EDITOR
                return DeviceSupportsTrueDepth();
            #else
                return true;
            #endif
        }

        public static bool IsUnknownDeviceGeneration()
        {
            #if UNITY_IOS && !UNITY_EDITOR
                return _unknownDevices.Contains(Device.generation);
            #else
                return false;
            #endif
        }

        public static bool IsLowEndDevice()
        {
            return SystemInfo.systemMemorySize < 2500;
        }
    }
}