using System.Collections.Generic;
using System.Linq;
using Modules.CameraSystem.CameraAnimations;
using UnityEngine;

namespace Modules.CameraSystem.Extensions
{
    public static class CameraAnimationFrameExtension
    {
        private static readonly IReadOnlyCollection<CameraAnimationProperty> TransformProperties =
            new[]
            {
                CameraAnimationProperty.PositionX, CameraAnimationProperty.PositionY, CameraAnimationProperty.PositionZ,
                CameraAnimationProperty.RotationX, CameraAnimationProperty.RotationY, CameraAnimationProperty.RotationZ
            };
        
        private static readonly IReadOnlyCollection<CameraAnimationProperty> CinemachineProperties =
            new[]
            {
                CameraAnimationProperty.AxisX, CameraAnimationProperty.AxisY, CameraAnimationProperty.Dutch,
                CameraAnimationProperty.HeightRadius, CameraAnimationProperty.OrbitRadius
            };
        
        private static readonly IReadOnlyCollection<CameraAnimationProperty> PostProcessingProperties =
            new[]
            {
                CameraAnimationProperty.DepthOfField, CameraAnimationProperty.FocusDistance, CameraAnimationProperty.FieldOfView
            };

        private const float PROPERTY_VALUES_POSSIBLE_DEVIATION = 0.001f;
        
        public static bool HasEqualCinemachineValues(this CameraAnimationFrame frame1, CameraAnimationFrame frame2)
        {
            foreach (var prop in frame1)
            {
                if(!prop.Key.IsCinemachineProperty()) continue;

                if (!IsPropertyValuesEqual(prop.Value, frame2.GetValue(prop.Key)))
                    return false;
            }

            return true;
        }
        
        public static bool HasEqualPostProcessingValues(this CameraAnimationFrame frame1, CameraAnimationFrame frame2)
        {
            foreach (var prop in frame1)
            {
                if(!prop.Key.IsPostProcessingProperty()) continue;
                
                if (!IsPropertyValuesEqual(prop.Value, frame2.GetValue(prop.Key)))
                    return false;
            }

            return true;
        }

        private static bool IsPostProcessingProperty(this CameraAnimationProperty prop)
        {
            return PostProcessingProperties.Contains(prop);
        }

        private static bool IsCinemachineProperty(this CameraAnimationProperty prop)
        {
            return CinemachineProperties.Contains(prop);
        }

        public static bool IsTransformProperty(this CameraAnimationProperty prop)
        {
            return TransformProperties.Contains(prop);
        }

        private static bool HasAllTransformProperties(this CameraAnimationFrame frame)
        {
            return TransformProperties.All(x => frame.Any(_ => _.Key == x));
        }

        internal static AnimationType GetAnimationType(this CameraAnimationFrame frame)
        {
            return frame.HasAllTransformProperties() ? AnimationType.TransformBased : AnimationType.CinemachineBased;
        }

        private static bool IsPropertyValuesEqual(float frame1Value, float frame2Value)
        {
            return Mathf.Abs(frame1Value - frame2Value) < PROPERTY_VALUES_POSSIBLE_DEVIATION;
        }
    }
}
