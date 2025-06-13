using ARKit;
using Common;
using Extensions;
using UnityEngine;

namespace Modules.LevelManaging.Assets
{
    internal static class Extensions
    {
        public static BlendShapeVisualizer AddOrGetBlendShapeVisualizer(this GameObject target)
        {
            if (Application.isEditor)
            {
                return target.AddOrGetComponent<EditorBlendShapeVisualizer>();
            }

            #if UNITY_IOS
            if (DeviceInformationHelper.DeviceSupportsTrueDepth())
            {
                return target.AddOrGetComponent<ARKitBlendShapeVisualizer>();
            }
            #endif
            
            return target.AddOrGetComponent<ARCoreBlendShapeVisualizer>();
        }
    }
}